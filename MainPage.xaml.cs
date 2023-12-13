using FluentFTP;

namespace TransferMySaves;

public partial class MainPage : ContentPage
{
    private readonly string _ftpHost = "192.168.0.23";
    private readonly int _ftpPort = 5000;

    public MainPage()
    {
        this.InitializeComponent();
    }

    private async void Button_OnClicked(object? sender, EventArgs e)
    {
        CancellationToken token = new();

        using AsyncFtpClient ftp = new(this._ftpHost, this._ftpPort);

        await ftp.Connect(token);

        await ftp.DownloadFile(
            @"C:\Users\vassd\Desktop\Pokemon - Emerald Version (USA, Europe).srm",
            "/retroarch/cores/savefiles/gpSP/Pokemon - Emerald Version (USA, Europe).srm",
            FtpLocalExists.Overwrite,
            FtpVerify.Retry, token: token);
    }
}

