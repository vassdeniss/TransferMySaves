using FluentFTP;

using Microsoft.Extensions.Configuration;

using Serilog;

namespace TransferMySaves;

public partial class MainPage : ContentPage
{
    private readonly Progress<FtpProgress> _progress;
    private readonly IConfiguration _config;

    public MainPage(IConfiguration config)
    {
        this.InitializeComponent();

        this._progress = new(async x =>
        {
            if (x.Progress < 0)
            {
                return;
            }

            await this.progressBar.ProgressTo(x.Progress / 100, 200, Easing.Linear);
        });

        this._config = config;
    }

    private async void TransferButton_OnClicked(object? sender, EventArgs e)
    {
        CancellationToken token = new();

        using AsyncFtpClient ftpFrom = new(this.fromHostEntry.Text);
        if (!string.IsNullOrEmpty(this.fromPortEntry.Text))
        {
            ftpFrom.Port = int.Parse(this.fromPortEntry.Text);
        }
        ftpFrom.LegacyLogger = (level, s) =>
        {
            Log.Information(s);
        };


        using AsyncFtpClient ftpTo = new(this.toHostEntry.Text);
        if (!string.IsNullOrEmpty(this.toPortEntry.Text))
        {
            ftpTo.Port = int.Parse(this.toPortEntry.Text);
        }
        ftpTo.LegacyLogger = (level, s) =>
        {
            Log.Information(s);
        };

        DirectoryInfo directory = new(FileSystem.Current.AppDataDirectory);

        try
        {
            this.transferButton.IsEnabled = false;
            this.activityIndicator.IsVisible = true;
            this.activityIndicator.IsRunning = true;
            this.progressBar.IsVisible = true;

            await ftpFrom.AutoConnect(token);

            IEnumerable<IConfiguration> configurations = this._config.GetChildren();
            foreach (IConfiguration config in configurations)
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }

                EmuPath path = config.Get<EmuPath>()!;

                // Debug purposes
                if (path.PspConverter is null)
                {
                    continue;
                }

                path.DetermineFromToPaths(
                    this.fromPicker.Items[this.fromPicker.SelectedIndex],
                    out string fromPath,
                    out string toPath);

                FtpListItem[] files = await ftpFrom.GetListing(fromPath, FtpListOption.ForceList, token);
                await ftpFrom.DownloadFiles(
                    FileSystem.Current.AppDataDirectory,
                    files.Select(file => file.FullName),
                    FtpLocalExists.Overwrite,
                    FtpVerify.Retry,
                    FtpError.DeleteProcessed,
                    token,
                    this._progress);

                if (path.DsConverter is not null && path.PspConverter is not null)
                {
                    string? convertorString = this.fromPicker.Items[this.fromPicker.SelectedIndex] == "3DS"
                        ? path.PspConverter
                        : path.DsConverter;
                    if (convertorString is not null)
                    {
                        Func<string, Task> convertor = Convertors.GetCorrectConvertor(convertorString);
                        await Convertors.ApplyConverter(convertor, directory.GetFiles());
                    }
                }

                await ftpTo.AutoConnect(token);
                this.progressBar.Progress = 0;
                await ftpTo.UploadFiles(
                    Directory.GetFiles(FileSystem.Current.AppDataDirectory),
                    toPath,
                    token: token,
                    progress: this._progress);
            }

            await this.DisplayAlert("Success!", "Saves transferred!", "Ok");
        }
        catch (Exception exception)
        {
            await this.DisplayAlert("Error!", exception.Message, "Ok");
        }
        finally
        {
            this.progressBar.IsVisible = false;
            this.transferButton.IsEnabled = true;
            this.activityIndicator.IsVisible = false;
            this.activityIndicator.IsRunning = false;

            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            await ftpFrom.Disconnect(token);
            await ftpTo.Disconnect(token);
        }
    }
}
