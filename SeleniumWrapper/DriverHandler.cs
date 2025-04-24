using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace SeleniumWrapper;

public class DriverHandler
{
    public static void DownloadLatest(string cacheFolder)
    {
        using HttpClient client = new();

        dynamic json = JsonSerializer.Deserialize<dynamic>(
            client.GetStringAsync("https:// github.com/GrahamKracker/StreamActions/remotevars.json").Result);

        var driverExecutable = Path.Combine(cacheFolder, "edgedriver_win64", "msedgedriver.exe");

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(driverExecutable);

        if (!File.Exists(driverExecutable) || fileVersionInfo.FileVersion != json?.driverVersion)
        {
            if(json == null)
                throw new InvalidOperationException("Failed to get driver version from github");

            var response =
                client.GetAsync($"https://msedgedriver.azureedge.net/{json.driverVersion}/edgedriver_win64.zip").Result;
            using var stream = response.Content.ReadAsStreamAsync().Result;

            using var fileStream = File.Create(Path.Combine(cacheFolder, "edgedriver_win64.zip"));

            stream.CopyTo(fileStream);

            fileStream.Close();

            //extract the zip

            ZipFile.ExtractToDirectory(Path.Combine(cacheFolder, "edgedriver_win64.zip"),
                Path.Combine(cacheFolder, "edgedriver_win64"));

            File.Delete(Path.Combine(cacheFolder, "edgedriver_win64.zip"));
        }
    }
}