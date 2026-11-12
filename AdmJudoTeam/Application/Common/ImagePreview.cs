namespace AdmJudoTeam.Application.Common;

/// <summary>
/// Builds in-memory data URLs for image previews rendered in views. Images live
/// as files on disk and the database stores only their paths; previews are
/// generated on demand and never persisted.
/// </summary>
public static class ImagePreview
{
    /// <summary>
    /// Reads the image file and returns a data URL suitable for an img src, or
    /// null when the path is empty, missing, or unreadable.
    /// </summary>
    public static string? ToDataUrl(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var base64 = Convert.ToBase64String(File.ReadAllBytes(filePath));
            return $"data:{GetMimeType(filePath)};base64,{base64}";
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static string GetMimeType(string filePath) => Path.GetExtension(filePath).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "image/jpeg",
    };
}
