using AdmJudoTeam.Application.Common;
using AdmJudoTeam.Core.Entities;
using AdmJudoTeam.Core.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AdmJudoTeam.Application.ViewModels;

/// <summary>
/// VM de la pantalla de alta/edición del club (dojo). Alta con
/// <c>LoadAsync(null)</c> (ruta /club/nuevo) o edición del club indicado.
/// </summary>
public partial class ClubViewModel : BaseViewModel
{
    private readonly IJudoRepository _repository;
    private readonly IPhotoStorageService _photoStorage;
    private readonly IClubContextService _clubContext;

    public ClubViewModel(IJudoRepository repository, IPhotoStorageService photoStorage, IClubContextService clubContext)
    {
        _repository = repository;
        _photoStorage = photoStorage;
        _clubContext = clubContext;
    }

    public Club Editing { get; private set; } = new();

    public bool IsNew => Editing.Id == 0;

    public string FormTitle => IsNew ? "Alta del club / dojo" : "Datos del club / dojo";

    /// <summary>Id of the selected club, or null when none is selected.</summary>
    public int? SelectedClubId => _clubContext.SelectedClubId;

    /// <summary>False when no club is selected; the edit route redirects to the selector.</summary>
    public bool HasSelectedClub => _clubContext.SelectedClubId.HasValue;

    /// <summary>Id returned by the last successful save.</summary>
    public int? LastSavedClubId { get; private set; }

    /// <summary>
    /// True when the last save created a new club; the view then navigates to
    /// the club panel (the new club is already the active club context).
    /// </summary>
    public bool SavedNewClub { get; private set; }

    private bool _isConfirmingDelete;
    private bool _deleted;
    private int? _lastDeletedClubId;

    /// <summary>True while the two-step delete confirmation panel is armed.</summary>
    public bool IsConfirmingDelete
    {
        get => _isConfirmingDelete;
        private set => SetProperty(ref _isConfirmingDelete, value);
    }

    /// <summary>
    /// True after the club was deleted; the view then navigates back to the
    /// club selector.
    /// </summary>
    public bool Deleted
    {
        get => _deleted;
        private set => SetProperty(ref _deleted, value);
    }

    /// <summary>Id of the club deleted by the last delete command.</summary>
    public int? LastDeletedClubId
    {
        get => _lastDeletedClubId;
        private set => SetProperty(ref _lastDeletedClubId, value);
    }

    /// <summary>
    /// In-memory data URL preview of the club logo; never persisted.
    /// </summary>
    public string? LogoPreview { get; private set; }

    /// <summary>
    /// Loads the form: <paramref name="clubId"/> null opens create mode,
    /// otherwise the given club is loaded for editing.
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync(int? clubId)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            SetStatus(null);
            LastSavedClubId = null;
            SavedNewClub = false;
            IsConfirmingDelete = false;
            Deleted = false;
            LastDeletedClubId = null;
            Editing = clubId.HasValue
                ? await _repository.GetClubAsync(clubId.Value).ConfigureAwait(false) ?? new Club()
                : new Club();
            RefreshLogoPreview();
            NotifyEditingChanged();
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo cargar el club: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Editing.Name))
        {
            SetStatus("Ingrese el nombre del club o dojo.", isError: true);
            return;
        }

        try
        {
            var wasNew = Editing.Id == 0;
            Editing.Id = await _repository.SaveClubAsync(Editing).ConfigureAwait(false);
            LastSavedClubId = Editing.Id;
            SavedNewClub = wasNew;
            if (wasNew)
            {
                // A freshly created club becomes the active club context so the
                // panel and every scoped screen open on it right away.
                _clubContext.SelectClub(Editing.Id);
            }

            SetStatus("Datos del club guardados.");
            NotifyEditingChanged();
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudieron guardar los datos: {ex.Message}", isError: true);
        }
    }

    [RelayCommand]
    private async Task PickLogoAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var path = await _photoStorage.PickPhotoAsync(LogoFilePrefix()).ConfigureAwait(false);
            if (path is null)
            {
                SetStatus("No se pudo seleccionar la imagen.", isError: true);
                return;
            }

            await ReplaceLogoAsync(path).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo seleccionar la imagen: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CaptureLogoAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var path = await _photoStorage.CapturePhotoAsync(LogoFilePrefix()).ConfigureAwait(false);
            if (path is null)
            {
                SetStatus("No se pudo capturar la foto.", isError: true);
                return;
            }

            await ReplaceLogoAsync(path).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo capturar la foto: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RemoveLogoAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var previous = Editing.LogoPath;
            Editing.LogoPath = null;
            RefreshLogoPreview();

            if (!string.IsNullOrWhiteSpace(previous))
            {
                await _photoStorage.DeletePhotoAsync(previous).ConfigureAwait(false);
            }

            SetStatus("Logo eliminado. Guarde los datos para confirmar el cambio.");
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo quitar el logo: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Arms the two-step delete confirmation (edit mode only).</summary>
    [RelayCommand]
    private void AskDelete()
    {
        if (Editing.Id == 0)
        {
            return;
        }

        SetStatus(null);
        IsConfirmingDelete = true;
    }

    /// <summary>Disarms the delete confirmation.</summary>
    [RelayCommand]
    private void CancelDelete() => IsConfirmingDelete = false;

    /// <summary>
    /// Deletes the club with all of its data (repository cascade) and clears the
    /// active club context so the view can return to the club selector.
    /// </summary>
    [RelayCommand]
    private async Task DeleteClubAsync()
    {
        if (IsBusy || Editing.Id == 0)
        {
            // Edit mode only: an unsaved club has nothing to delete.
            return;
        }

        try
        {
            IsBusy = true;
            SetStatus(null);

            var clubId = Editing.Id;
            var logoPath = Editing.LogoPath;
            // Capture the photo paths before the cascade removes the member rows.
            var memberPhotoPaths = (await _repository.GetMembersAsync(clubId, activeOnly: false).ConfigureAwait(false))
                .Select(m => m.PhotoFilePath)
                .ToList();

            await _repository.DeleteClubAsync(Editing).ConfigureAwait(false);

            // Best-effort file cleanup after the database delete: leftover files
            // are harmless, so a failure must never surface as a delete error.
            await TryDeletePhotosAsync(memberPhotoPaths.Prepend(logoPath)).ConfigureAwait(false);

            _clubContext.Clear();
            IsConfirmingDelete = false;
            LastDeletedClubId = clubId;
            Deleted = true;
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo eliminar el club: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Deletes the given photo files, swallowing every failure so one missing
    /// or locked file cannot block the rest of the cleanup.
    /// </summary>
    private async Task TryDeletePhotosAsync(IEnumerable<string?> paths)
    {
        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            try
            {
                await _photoStorage.DeletePhotoAsync(path).ConfigureAwait(false);
            }
            catch
            {
                // Best-effort cleanup only: keep deleting the remaining files.
            }
        }
    }

    private string LogoFilePrefix() => Editing.Id == 0 ? "club_logo_new" : $"club_logo_{Editing.Id}";

    /// <summary>
    /// Points the logo to the new file and deletes the replaced one so orphan
    /// files are not left on disk.
    /// </summary>
    private async Task ReplaceLogoAsync(string newPath)
    {
        var previous = Editing.LogoPath;
        Editing.LogoPath = newPath;
        RefreshLogoPreview();

        if (!string.IsNullOrWhiteSpace(previous) &&
            !string.Equals(previous, newPath, StringComparison.OrdinalIgnoreCase))
        {
            await _photoStorage.DeletePhotoAsync(previous).ConfigureAwait(false);
        }

        SetStatus("Logo actualizado. Guarde los datos para conservarlo.");
    }

    private void RefreshLogoPreview()
    {
        LogoPreview = ImagePreview.ToDataUrl(Editing.LogoPath);
        OnPropertyChanged(nameof(LogoPreview));
    }

    private void NotifyEditingChanged()
    {
        OnPropertyChanged(nameof(Editing));
        OnPropertyChanged(nameof(IsNew));
        OnPropertyChanged(nameof(FormTitle));
    }
}
