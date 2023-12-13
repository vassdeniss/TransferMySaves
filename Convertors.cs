using System.IO.Compression;

namespace TransferMySaves;

public class Convertors
{
    public static Func<string, Task> GetCorrectConvertor(string convertorString)
    {
        switch (convertorString)
        {
            case "SrmToSavGz":
                return SrmToSavGz;
            default:
                throw new ArgumentOutOfRangeException(nameof(convertorString), convertorString, "Convertor type does not exist");
        }
    }

    public static async Task ApplyConverter(Func<string, Task> converter, FileInfo[] files)
    {
        foreach (FileInfo file in files)
        {
            await converter(file.FullName);
        }
    }

    private static async Task SrmToSavGz(string filePath)
    {
        if (Path.GetExtension(filePath) != ".srm")
        {
            return;
        }

        string newFilePath = Path.ChangeExtension(filePath, ".sav");
        File.Move(filePath, newFilePath);

        await using (FileStream sourceFileStream = File.OpenRead(newFilePath))
        {
            await using (FileStream compressedFileStream = File.Create($"{newFilePath}.gz"))
            {
                await using (GZipStream gzipStream = new(compressedFileStream, CompressionMode.Compress))
                {
                    await sourceFileStream.CopyToAsync(gzipStream);
                }
            }
        }

        File.Delete(newFilePath);
    }
}
