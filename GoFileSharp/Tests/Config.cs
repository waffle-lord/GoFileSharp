using System.Diagnostics;
using Newtonsoft.Json;

namespace Tests;

public class Config
{
    private static string _configPath = Path.GetFullPath("../../../tests_config.json");
    public static Config Load()
    {
        var apiToken =Environment.GetEnvironmentVariable("GOFILE_API_TOKEN");

        // use environment vars during github actions if they are present
        if (!string.IsNullOrWhiteSpace(apiToken))
            return new Config() { ApiToken = apiToken };
        
        
        // load config from file for local test runs
        try
        {
            Debug.WriteLine($"Loading Config: {_configPath}");
            var json = File.ReadAllText(_configPath);
            return JsonConvert.DeserializeObject<Config>(json, new JsonSerializerSettings {MissingMemberHandling = MissingMemberHandling.Error});
        }
        catch (FileNotFoundException)
        {
            var exampleJson = JsonConvert.SerializeObject(new Config(), new JsonSerializerSettings {Formatting = Formatting.Indented});
            File.WriteAllText(_configPath, exampleJson);
            Debug.WriteLine($"Config File Created, please update it and run again:\nPath: {_configPath}");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception Thrown: {ex.Message}\n{ex.StackTrace}");
            Environment.Exit(0);
        }

        return null;
    }

    /// <summary>
    /// The GoFile API token to use during testing
    /// </summary>
    [JsonProperty("api-token")]
    public string ApiToken { get; set; } = "your_api_token_here";
}