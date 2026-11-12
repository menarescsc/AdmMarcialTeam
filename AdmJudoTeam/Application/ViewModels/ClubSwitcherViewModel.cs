using System.Collections.ObjectModel;
using AdmJudoTeam.Application.Common;
using AdmJudoTeam.Core.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;

namespace AdmJudoTeam.Application.ViewModels;

/// <summary>
/// VM of the navigation club switcher: lists the registered clubs and switches
/// the active club context without going back to the club selector screen.
/// The navigation menu stays alive across navigations, so its view reloads this
/// VM whenever the location changes to pick up added or deleted clubs.
/// </summary>
public partial class ClubSwitcherViewModel : BaseViewModel
{
    private readonly IJudoRepository _repository;
    private readonly IClubContextService _clubContext;
    private readonly NavigationManager _navigation;

    private bool _isExpanded;

    public ClubSwitcherViewModel(IJudoRepository repository, IClubContextService clubContext, NavigationManager navigation)
    {
        _repository = repository;
        _clubContext = clubContext;
        _navigation = navigation;
    }

    /// <summary>Registered clubs offered by the switcher.</summary>
    public ObservableCollection<ClubCardDto> Clubs { get; } = new();

    /// <summary>False when no club is registered yet; the switcher renders nothing.</summary>
    public bool HasClubs => Clubs.Count > 0;

    /// <summary>Id of the active club, or null when none is selected.</summary>
    public int? SelectedClubId => _clubContext.SelectedClubId;

    /// <summary>Active club shown on the collapsed button; null when none is selected.</summary>
    public ClubCardDto? SelectedClub => Clubs.FirstOrDefault(c => c.Id == _clubContext.SelectedClubId);

    /// <summary>True while the club list is expanded.</summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        private set => SetProperty(ref _isExpanded, value);
    }

    /// <summary>Expands or collapses the club list.</summary>
    [RelayCommand]
    private void ToggleExpanded() => IsExpanded = !IsExpanded;

    /// <summary>Collapses the club list (used by the view after navigation).</summary>
    public void Collapse() => IsExpanded = false;

    /// <summary>Reloads the registered clubs and the active club marker.</summary>
    [RelayCommand]
    private async Task ReloadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            SetStatus(null);
            var clubs = await _repository.GetClubsAsync().ConfigureAwait(false);
            Clubs.Clear();
            foreach (var club in clubs)
            {
                Clubs.Add(new ClubCardDto
                {
                    Id = club.Id,
                    Name = club.Name,
                    Address = club.Address,
                    LogoPreview = ImagePreview.ToDataUrl(club.LogoPath)
                });
            }

            NotifySelectionChanged();
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudieron cargar los clubes: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Switches the active club context and opens its panel.</summary>
    [RelayCommand]
    private void SelectClub(int clubId)
    {
        _clubContext.SelectClub(clubId);
        IsExpanded = false;
        NotifySelectionChanged();
        _navigation.NavigateTo("panel");
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(HasClubs));
        OnPropertyChanged(nameof(SelectedClubId));
        OnPropertyChanged(nameof(SelectedClub));
    }
}
