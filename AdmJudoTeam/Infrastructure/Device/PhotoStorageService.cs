using AdmJudoTeam.Core.Interfaces;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace AdmJudoTeam.Infrastructure.Device;

/// <summary>
/// Captura fotos con la cámara o las selecciona de la galería y las guarda como
/// archivos físicos en FileSystem.AppDataDirectory. En SQLite solo se persiste la
/// ruta (nunca Base64 en la base).
/// </summary>
public sealed class PhotoStorageService : IPhotoStorageService
{
    public async Task<string?> CapturePhotoAsync(string filePrefix, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                return null;
            }

            var status = await Permissions.RequestAsync<Permissions.Camera>().ConfigureAwait(false);
            if (status != PermissionStatus.Granted)
            {
                return null;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Foto de perfil del atleta"
            }).ConfigureAwait(false);

            if (photo is null)
            {
                return null;
            }

            var safePrefix = string.IsNullOrWhiteSpace(filePrefix) ? "photo" : filePrefix.Trim();
            var fileName = $"{safePrefix}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.jpg";
            var targetPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            await using var source = await photo.OpenReadAsync().ConfigureAwait(false);
            await using var target = File.Create(targetPath);
            await source.CopyToAsync(target, cancellationToken).ConfigureAwait(false);

            return targetPath;
        }
        catch (FeatureNotSupportedException)
        {
            return null;
        }
        catch (PermissionException)
        {
            return null;
        }
    }

    public async Task<string?> PickPhotoAsync(string filePrefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                Title = "Seleccionar imagen"
            }).ConfigureAwait(false);

            var photo = photos?.FirstOrDefault();
            if (photo is null)
            {
                return null;
            }

            var safePrefix = string.IsNullOrWhiteSpace(filePrefix) ? "photo" : filePrefix.Trim();
            var fileName = $"{safePrefix}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{NormalizeExtension(photo.FileName)}";
            var targetPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            await using var source = await photo.OpenReadAsync().ConfigureAwait(false);
            await using var target = File.Create(targetPath);
            await source.CopyToAsync(target, cancellationToken).ConfigureAwait(false);

            return targetPath;
        }
        catch (FeatureNotSupportedException)
        {
            return null;
        }
        catch (PermissionException)
        {
            return null;
        }
    }

    public Task<bool> DeletePhotoAsync(string filePath)
    {
        try
        {
            // Solo se pueden borrar archivos dentro del almacenamiento de la app.
            var fullPath = Path.GetFullPath(filePath);
            var appDataPath = Path.GetFullPath(FileSystem.AppDataDirectory);
            if (!fullPath.StartsWith(appDataPath, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(false);
            }

            if (!File.Exists(fullPath))
            {
                return Task.FromResult(false);
            }

            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        catch (IOException)
        {
            return Task.FromResult(false);
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Keeps known image extensions from the picked file so previews can infer
    /// the MIME type; anything else falls back to .jpg.
    /// </summary>
    private static string NormalizeExtension(string? fileName)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".webp" ? extension : ".jpg";
    }
}
