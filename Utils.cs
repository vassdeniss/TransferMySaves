using FluentFTP;

namespace TransferMySaves;

public class Utils
{
    private static readonly DirectoryInfo Directory = new(FileSystem.Current.AppDataDirectory);

    public static async Task DownloadFromPath(
        AsyncFtpClient client, 
        string path, 
        CancellationToken token,
        IProgress<FtpProgress> progress,
        ProgressBar bar)
    {
        bar.Progress = 0;
        FtpListItem[] files = await client.GetListing(path, token);
        await client.DownloadFiles(
            Directory.FullName,
            files.Select(file => file.FullName),
            token: token,
            progress: progress);
    }

    public static async Task UploadToPath(
        AsyncFtpClient client,
        string path,
        CancellationToken token,
        IProgress<FtpProgress> progress,
        ProgressBar bar)
    {
        bar.Progress = 0;
        foreach (FileInfo file in Directory.GetFiles())
        {
            await client.UploadFile(
                file.FullName,
                $"{path}/{file.Name}",
                token: token,
                progress: progress);
        }
    }
}
