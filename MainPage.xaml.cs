using FluentFTP;

using Microsoft.Extensions.Configuration;

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

        this.emulatorView.ItemsSource = this._config.GetChildren().Select(x => x.Key);
    }

    private async void TransferButton_OnClicked(object? sender, EventArgs e)
    {
        CancellationToken token = new();

        using AsyncFtpClient ftpFrom = new(this.fromHostEntry.Text);
        if (!string.IsNullOrEmpty(this.fromPortEntry.Text))
        {
            ftpFrom.Port = int.Parse(this.fromPortEntry.Text);
        }


        using AsyncFtpClient ftpTo = new(this.toHostEntry.Text);
        if (!string.IsNullOrEmpty(this.toPortEntry.Text))
        {
            ftpTo.Port = int.Parse(this.toPortEntry.Text);
        }

        try
        {
            this.transferButton.IsEnabled = false;
            this.activityIndicator.IsVisible = true;
            this.activityIndicator.IsRunning = true;
            this.progressBar.IsVisible = true;

            await ftpFrom.AutoConnect(token);
            await ftpTo.AutoConnect(token);

            List<string> consoles = this.emulatorView.SelectedItems.Cast<string>().ToList();
            foreach (string console in consoles)
            {
                EmuPath path = this._config.GetRequiredSection(console).Get<EmuPath>()!;
                path.DetermineFromToPaths(
                    this.fromPicker.Items[this.fromPicker.SelectedIndex],
                    this.toPicker.Items[this.toPicker.SelectedIndex],
                    out string fromPath,
                    out string fromStates,
                    out string toPath,
                    out string toStates);

                await Utils.DownloadFromPath(ftpFrom, fromPath, token, this._progress, this.progressBar);
                await Utils.DownloadFromPath(ftpFrom, fromStates, token, this._progress, this.progressBar);

                await Utils.UploadToPath(ftpTo, toPath, token, this._progress, this.progressBar);
                await Utils.UploadToPath(ftpTo, toStates, token, this._progress, this.progressBar);
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

            await ftpFrom.Disconnect(token);
            await ftpTo.Disconnect(token);
        }
    }
}
