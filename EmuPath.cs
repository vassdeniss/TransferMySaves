namespace TransferMySaves;

public class EmuPath
{
    public string DsPath { get; set; } = null!;

    public string DsSavestatesPath { get; set; } = null!;

    public string PcPath { get; set; } = null!;

    public string PcSavestatesPath { get; set; } = null!;

    public void DetermineFromToPaths(string selectedSource, string selectedTarget, out string fromPath, out string fromStates, out string toPath, out string toStates)
    {
        fromPath = selectedSource switch
        {
            "3DS" => this.DsPath,
            "PC" => this.PcPath,
            _ => throw new ArgumentOutOfRangeException(nameof(selectedSource), selectedSource, "Source does not exist!")
        };

        fromStates = selectedSource switch
        {
            "3DS" => this.DsSavestatesPath,
            "PC" => this.PcSavestatesPath,
            _ => throw new ArgumentOutOfRangeException(nameof(selectedSource), selectedSource, "Source does not exist!")
        };

        toPath = selectedTarget switch
        {
            "3DS" => this.DsPath,
            "PC" => this.PcPath,
            _ => throw new ArgumentOutOfRangeException(nameof(selectedSource), selectedSource, "Source does not exist!")
        };

        toStates = selectedTarget switch
        {
            "3DS" => this.DsPath,
            "PC" => this.PcSavestatesPath,
            _ => throw new ArgumentOutOfRangeException(nameof(selectedSource), selectedSource, "Source does not exist!")
        };
    }
}
