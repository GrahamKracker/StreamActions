using System.Diagnostics;
using System.IO.Compression;
using Newtonsoft.Json;

namespace SeleniumWrapper;

public class DriverHandler
{
    public static void DownloadLatest(string cacheFolder)
    {
        using HttpClient client = new();

        dynamic? json = JsonConvert.DeserializeObject(
            client.GetStringAsync("https://raw.githubusercontent.com/GrahamKracker/StreamActions/refs/heads/main/remotevars.json").Result);

        var driverExecutable = Path.Combine(cacheFolder, "edgedriver_win64", "msedgedriver.exe");

        var fileVersionInfo = File.Exists(driverExecutable) ? FileVersionInfo.GetVersionInfo(driverExecutable) : null;

        if (!File.Exists(driverExecutable) || fileVersionInfo?.FileVersion != json?.driverVersion.ToString())
        {
            if(json == null)
                throw new InvalidOperationException("Failed to get driver version from github");

            if(Directory.Exists(Path.Combine(cacheFolder, "edgedriver_win64")))
                Directory.Delete(Path.Combine(cacheFolder, "edgedriver_win64"), true);

            var response =
                client.GetAsync($"https://msedgedriver.azureedge.net/{json.driverVersion}/edgedriver_win64.zip").Result;
            using var stream = response.Content.ReadAsStreamAsync().Result;

            using var fileStream = File.Create(Path.Combine(cacheFolder, "edgedriver_win64.zip"));

            stream.CopyTo(fileStream);

            fileStream.Close();

            //extract the zip

            ZipFile.ExtractToDirectory(Path.Combine(cacheFolder, "edgedriver_win64.zip"),
                Path.Combine(cacheFolder, "edgedriver_win64"), true);

            File.Delete(Path.Combine(cacheFolder, "edgedriver_win64.zip"));
        }
    }
}