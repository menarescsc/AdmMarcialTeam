using AdmJudoTeam.Core.Interfaces;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace AdmJudoTeam.Infrastructure.Device;

/// <summary>
/// Comparte archivos generados (nómina PDF) usando el share sheet nativo.
/// </summary>
public sealed class FileShareService : IFileShareService
{
    public Task ShareFileAsync(string filePath, string title)
    {
        if (!File.Exists(filePath))
        {
            return Task.CompletedTask;
        }

        return Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = title,
            File = new ShareFile(filePath)
        });
    }
}
