namespace AdmJudoTeam.Core.Interfaces;

/// <summary>
/// Captura y almacenamiento de fotos de perfil. Las imágenes se guardan como
/// archivos físicos en FileSystem.AppDataDirectory; la base solo guarda la ruta.
/// </summary>
public interface IPhotoStorageService
{
    /// <summary>
    /// Toma una foto con la cámara y la guarda en disco. Devuelve la ruta
    /// absoluta del archivo, o null si el usuario cancela.
    /// </summary>
    Task<string?> CapturePhotoAsync(string filePrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lets the user pick an image from the gallery and saves it to disk.
    /// Returns the absolute file path, or null if the user cancels.
    /// </summary>
    Task<string?> PickPhotoAsync(string filePrefix, CancellationToken cancellationToken = default);

    Task<bool> DeletePhotoAsync(string filePath);
}
