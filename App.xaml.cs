using Serilog;

namespace TransferMySaves;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "log.txt"))
            .CreateLogger();

        this.MainPage = new AppShell();

        Current.UserAppTheme = AppTheme.Light;
    }
}
