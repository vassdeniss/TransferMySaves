namespace TransferMySaves;

public class EmuPath
{
    public string DsPath { get; set; } = null!;

    public string PspPath { get; set; } = null!;

    public string? DsConverter { get; set; }

    public string? PspConverter { get; set; }

    public void DetermineFromToPaths(string selectedSource, out string fromPath, out string toPath)
    {
        fromPath = selectedSource switch
        {
            "3DS" => this.DsPath,
            "PSP" => this.PspPath,
            _ => throw new ArgumentOutOfRangeException(nameof(selectedSource), selectedSource, "Source does not exist!")
        };

        toPath = selectedSource switch
        {
            "3DS" => this.PspPath,
            "PSP" => this.DsPath,
            _ => throw new ArgumentOutOfRangeException(nameof(selectedSource), selectedSource, "Source does not exist!")
        };
    }
}
