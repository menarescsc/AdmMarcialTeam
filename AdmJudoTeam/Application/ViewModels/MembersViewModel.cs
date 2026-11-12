using System.Collections.ObjectModel;
using AdmJudoTeam.Application.Common;
using AdmJudoTeam.Core.Entities;
using AdmJudoTeam.Core.Enums;
using AdmJudoTeam.Core.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AdmJudoTeam.Application.ViewModels;

/// <summary>
/// VM de la pantalla de gestión de atletas: ficha completa (grado, categoría
/// IJF, foto local, ficha médica de emergencia) con alta, edición y
/// activación/desactivación. La foto vive como archivo en disco; la vista
/// recibe solo una vista previa en memoria.
/// </summary>
public partial class MembersViewModel : BaseViewModel
{
    private readonly IJudoRepository _repository;
    private readonly IPhotoStorageService _photoStorage;
    private readonly IClubContextService _clubContext;

    public MembersViewModel(IJudoRepository repository, IPhotoStorageService photoStorage, IClubContextService clubContext)
    {
        _repository = repository;
        _photoStorage = photoStorage;
        _clubContext = clubContext;
    }

    /// <summary>False when no club is selected; the view redirects to the selector.</summary>
    public bool HasSelectedClub => _clubContext.SelectedClubId.HasValue;

    public ObservableCollection<Member> Members { get; } = new();

    public Member Editing { get; private set; } = new();

    public bool IsEditing { get; private set; }

    public string FormTitle => Editing.Id == 0 ? "Nuevo atleta" : "Editar atleta";

    public string? PhotoPreview { get; private set; }

    private bool _showInactive;

    public bool ShowInactive
    {
        get => _showInactive;
        set => SetProperty(ref _showInactive, value);
    }

    public IReadOnlyList<(Gender Value, string Label)> GenderOptions { get; } = EnumDisplay.GetGenderOptions();
    public IReadOnlyList<(BeltGrade Value, string Label)> BeltOptions { get; } = EnumDisplay.GetBeltOptions();
    public IReadOnlyList<(WeightCategory Value, string Label)> MenWeightOptions { get; } = EnumDisplay.GetMenWeightOptions();
    public IReadOnlyList<(WeightCategory Value, string Label)> WomenWeightOptions { get; } = EnumDisplay.GetWomenWeightOptions();

    public string BeltLabel(BeltGrade grade) => EnumDisplay.GetBeltLabel(grade);

    public string WeightLabel(WeightCategory category) => Core.Models.WeightCategoryInfo.GetLabel(category);

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
            await LoadCoreAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudieron cargar los atletas: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void New()
    {
        if (_clubContext.SelectedClubId is not int clubId)
        {
            SetStatus("Seleccione un club primero.", isError: true);
            return;
        }

        Editing = new Member { ClubId = clubId, BirthDate = DateTime.Today, IsActive = true };
        IsEditing = true;
        RefreshPhotoPreview();
        NotifyEditingChanged();
    }

    [RelayCommand]
    private void Edit(Member member)
    {
        Editing = Clone(member);
        IsEditing = true;
        RefreshPhotoPreview();
        NotifyEditingChanged();
    }

    [RelayCommand]
    private void Cancel()
    {
        IsEditing = false;
        NotifyEditingChanged();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Editing.FirstName) || string.IsNullOrWhiteSpace(Editing.LastName))
        {
            SetStatus("Nombre y apellido son obligatorios.", isError: true);
            return;
        }

        try
        {
            Editing.Id = await _repository.SaveMemberAsync(Editing).ConfigureAwait(false);
            SetStatus("Ficha del atleta guardada.");
            IsEditing = false;
            await LoadCoreAsync().ConfigureAwait(false);
            NotifyEditingChanged();
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo guardar la ficha: {ex.Message}", isError: true);
        }
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(Member member)
    {
        try
        {
            member.IsActive = !member.IsActive;
            await _repository.SaveMemberAsync(member).ConfigureAwait(false);
            await LoadCoreAsync().ConfigureAwait(false);
            SetStatus(member.IsActive ? "Atleta reactivado." : "Atleta desactivado.");
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo actualizar el estado: {ex.Message}", isError: true);
        }
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            var prefix = Editing.Id == 0 ? "member_new" : $"member_{Editing.Id}";
            var path = await _photoStorage.CapturePhotoAsync(prefix).ConfigureAwait(false);
            if (path is null)
            {
                SetStatus("No se pudo capturar la foto.", isError: true);
                return;
            }

            Editing.PhotoFilePath = path;
            RefreshPhotoPreview();
            SetStatus("Foto capturada. Guarde la ficha para conservarla.");
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo capturar la foto: {ex.Message}", isError: true);
        }
    }

    [RelayCommand]
    private async Task RemovePhotoAsync()
    {
        if (Editing.PhotoFilePath is null)
        {
            return;
        }

        await _photoStorage.DeletePhotoAsync(Editing.PhotoFilePath).ConfigureAwait(false);
        Editing.PhotoFilePath = null;
        PhotoPreview = null;
        OnPropertyChanged(nameof(PhotoPreview));
    }

    private async Task LoadCoreAsync()
    {
        if (_clubContext.SelectedClubId is not int clubId)
        {
            Members.Clear();
            SetStatus("Seleccione un club primero.", isError: true);
            return;
        }

        var members = await _repository.GetMembersAsync(clubId, activeOnly: !ShowInactive).ConfigureAwait(false);
        Members.Clear();
        foreach (var member in members)
        {
            Members.Add(member);
        }
    }

    private void RefreshPhotoPreview()
    {
        PhotoPreview = null;
        var path = Editing.PhotoFilePath;
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            try
            {
                var base64 = Convert.ToBase64String(File.ReadAllBytes(path));
                PhotoPreview = $"data:image/jpeg;base64,{base64}";
            }
            catch (IOException)
            {
                PhotoPreview = null;
            }
        }

        OnPropertyChanged(nameof(PhotoPreview));
    }

    private void NotifyEditingChanged()
    {
        OnPropertyChanged(nameof(Editing));
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
    }

    private static Member Clone(Member source) => new()
    {
        Id = source.Id,
        ClubId = source.ClubId,
        FirstName = source.FirstName,
        LastName = source.LastName,
        BirthDate = source.BirthDate,
        Gender = source.Gender,
        BeltGrade = source.BeltGrade,
        WeightCategory = source.WeightCategory,
        ActualWeightKg = source.ActualWeightKg,
        PhotoFilePath = source.PhotoFilePath,
        EmergencyContactName = source.EmergencyContactName,
        EmergencyContactPhone = source.EmergencyContactPhone,
        MedicalNotes = source.MedicalNotes,
        IsActive = source.IsActive,
        CreatedAt = source.CreatedAt
    };
}
