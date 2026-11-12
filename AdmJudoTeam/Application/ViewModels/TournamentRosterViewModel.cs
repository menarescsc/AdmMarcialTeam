using System.Collections.ObjectModel;
using AdmJudoTeam.Application.Common;
using AdmJudoTeam.Core.Entities;
using AdmJudoTeam.Core.Interfaces;
using AdmJudoTeam.Core.Models;
using CommunityToolkit.Mvvm.Input;

namespace AdmJudoTeam.Application.ViewModels;

/// <summary>
/// VM de la vista de roster de torneo: lista de judokas, inscripción,
/// alerta de diferencia de peso contra la categoría IJF y objetivos tácticos.
/// La vista solo consume esta clase (MVVM estricto).
/// </summary>
public partial class TournamentRosterViewModel : BaseViewModel
{
    private readonly IJudoRepository _repository;
    private readonly ITournamentReportService _reportService;
    private readonly IFileShareService _shareService;
    private readonly IClubContextService _clubContext;
    private readonly Dictionary<int, TournamentEntry> _entriesByMember = new();

    public TournamentRosterViewModel(
        IJudoRepository repository,
        ITournamentReportService reportService,
        IFileShareService shareService,
        IClubContextService clubContext)
    {
        _repository = repository;
        _reportService = reportService;
        _shareService = shareService;
        _clubContext = clubContext;
    }

    /// <summary>False when no club is selected; the view redirects to the selector.</summary>
    public bool HasSelectedClub => _clubContext.SelectedClubId.HasValue;

    public ObservableCollection<Tournament> Tournaments { get; } = new();
    public ObservableCollection<RosterEntryDto> Roster { get; } = new();

    /// <summary>Borrador del formulario de alta de campeonato.</summary>
    public Tournament Draft { get; } = new()
    {
        StartDate = DateTime.Today,
        WeighInDate = DateTime.Today
    };

    private int _selectedTournamentId;

    public int SelectedTournamentId
    {
        get => _selectedTournamentId;
        set => SetProperty(ref _selectedTournamentId, value);
    }

    private string? _lastReportPath;

    /// <summary>Ruta de la última nómina PDF generada.</summary>
    public string? LastReportPath
    {
        get => _lastReportPath;
        private set => SetProperty(ref _lastReportPath, value);
    }

    [RelayCommand]
    private async Task GenerateReportAsync()
    {
        if (SelectedTournamentId == 0)
        {
            SetStatus("Seleccione un torneo para generar la nómina.", isError: true);
            return;
        }

        try
        {
            IsBusy = true;
            var path = await _reportService.GenerateRosterReportAsync(SelectedTournamentId).ConfigureAwait(false);
            LastReportPath = path;
            SetStatus($"Nómina oficial generada: {Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo generar la nómina: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ShareReportAsync()
    {
        if (LastReportPath is null)
        {
            SetStatus("Genere primero la nómina oficial.", isError: true);
            return;
        }

        try
        {
            await _shareService.ShareFileAsync(LastReportPath, "Nómina oficial de pesaje").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo compartir la nómina: {ex.Message}", isError: true);
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            SetStatus(null);

            if (_clubContext.SelectedClubId is not int clubId)
            {
                Tournaments.Clear();
                Roster.Clear();
                SetStatus("Seleccione un club primero.", isError: true);
                return;
            }

            var tournaments = await _repository.GetTournamentsAsync(clubId).ConfigureAwait(false);
            Tournaments.Clear();
            foreach (var tournament in tournaments)
            {
                Tournaments.Add(tournament);
            }

            SelectedTournamentId = Tournaments.FirstOrDefault()?.Id ?? 0;
            await LoadRosterCoreAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo cargar la información: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadRosterAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            SetStatus(null);
            await LoadRosterCoreAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo cargar el roster: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveTournamentAsync()
    {
        if (string.IsNullOrWhiteSpace(Draft.Name))
        {
            SetStatus("Ingrese el nombre del campeonato.", isError: true);
            return;
        }

        if (_clubContext.SelectedClubId is not int clubId)
        {
            SetStatus("Seleccione un club primero.", isError: true);
            return;
        }

        try
        {
            // New tournaments always belong to the active club context.
            Draft.ClubId = clubId;
            var id = await _repository.SaveTournamentAsync(Draft).ConfigureAwait(false);

            // Reiniciar el borrador sin perder la identidad del binding.
            Draft.Id = 0;
            Draft.ClubId = clubId;
            Draft.Name = string.Empty;
            Draft.Location = null;
            Draft.Notes = null;
            Draft.StartDate = DateTime.Today;
            Draft.WeighInDate = DateTime.Today;
            Draft.CreatedAt = DateTime.UtcNow;

            await LoadAsync().ConfigureAwait(false);
            SelectedTournamentId = id;
            await LoadRosterCoreAsync().ConfigureAwait(false);
            SetStatus("Campeonato guardado.");
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo guardar el campeonato: {ex.Message}", isError: true);
        }
    }

    [RelayCommand]
    private async Task ToggleEnrollmentAsync(RosterEntryDto row)
    {
        if (SelectedTournamentId == 0)
        {
            return;
        }

        try
        {
            if (row.IsEnrolled)
            {
                if (_entriesByMember.TryGetValue(row.MemberId, out var existing))
                {
                    await _repository.DeleteTournamentEntryAsync(existing).ConfigureAwait(false);
                    _entriesByMember.Remove(row.MemberId);
                }

                row.IsEnrolled = false;
                row.EntryId = null;
            }
            else
            {
                var entry = new TournamentEntry
                {
                    TournamentId = SelectedTournamentId,
                    MemberId = row.MemberId,
                    RegisteredWeightKg = row.ActualWeightKg ?? 0,
                    TacticalGoals = row.TacticalGoals
                };
                entry.Id = await _repository.SaveTournamentEntryAsync(entry).ConfigureAwait(false);
                _entriesByMember[row.MemberId] = entry;
                row.IsEnrolled = true;
                row.EntryId = entry.Id;
            }
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo actualizar la inscripción: {ex.Message}", isError: true);
        }
    }

    [RelayCommand]
    private async Task SaveGoalsAsync(RosterEntryDto row)
    {
        if (!_entriesByMember.TryGetValue(row.MemberId, out var entry))
        {
            SetStatus("Inscriba al atleta antes de guardar objetivos tácticos.", isError: true);
            return;
        }

        try
        {
            entry.TacticalGoals = row.TacticalGoals;
            await _repository.SaveTournamentEntryAsync(entry).ConfigureAwait(false);
            SetStatus("Objetivos tácticos guardados.");
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudieron guardar los objetivos: {ex.Message}", isError: true);
        }
    }

    private async Task LoadRosterCoreAsync()
    {
        Roster.Clear();
        _entriesByMember.Clear();

        if (_clubContext.SelectedClubId is not int clubId || SelectedTournamentId == 0)
        {
            return;
        }

        var entries = await _repository.GetTournamentEntriesAsync(SelectedTournamentId).ConfigureAwait(false);
        var members = await _repository.GetMembersAsync(clubId, activeOnly: true).ConfigureAwait(false);

        foreach (var entry in entries)
        {
            _entriesByMember[entry.MemberId] = entry;
        }

        foreach (var member in members)
        {
            var entry = _entriesByMember.GetValueOrDefault(member.Id);
            var limitKg = WeightCategoryInfo.GetLimitKg(member.WeightCategory);
            var isOpenCategory = double.IsPositiveInfinity(limitKg);

            Roster.Add(new RosterEntryDto
            {
                MemberId = member.Id,
                EntryId = entry?.Id,
                FullName = $"{member.LastName}, {member.FirstName}",
                CategoryLabel = WeightCategoryInfo.GetLabel(member.WeightCategory),
                IsOpenCategory = isOpenCategory,
                ActualWeightKg = member.ActualWeightKg,
                LimitKg = isOpenCategory ? 0 : limitKg,
                IsEnrolled = entry is not null,
                TacticalGoals = entry?.TacticalGoals
            });
        }
    }
}
