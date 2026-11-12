using System.Collections.ObjectModel;
using AdmJudoTeam.Application.Common;
using AdmJudoTeam.Core.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AdmJudoTeam.Application.ViewModels;

/// <summary>
/// VM of the club selector home screen ("Mis clubes"): lists only the
/// registered clubs and lets the user pick one as the active club context.
/// With zero clubs registered the view forces the club creation route.
/// </summary>
public partial class HomeViewModel : BaseViewModel
{
    private readonly IJudoRepository _repository;
    private readonly IClubContextService _clubContext;

    public HomeViewModel(IJudoRepository repository, IClubContextService clubContext)
    {
        _repository = repository;
        _clubContext = clubContext;
    }

    /// <summary>Registered clubs shown as selectable cards.</summary>
    public ObservableCollection<ClubCardDto> Clubs { get; } = new();

    /// <summary>True when at least one club is registered.</summary>
    public bool AnyClubRegistered => Clubs.Count > 0;

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

            OnPropertyChanged(nameof(AnyClubRegistered));
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

    /// <summary>Marks the tapped club as the active club context.</summary>
    [RelayCommand]
    private void SelectClub(int clubId) => _clubContext.SelectClub(clubId);
}
