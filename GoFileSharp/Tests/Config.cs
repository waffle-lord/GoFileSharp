using Newtonsoft.Json;

namespace Tests;

public class Config
{
    /// <summary>
    /// The GoFile API token to use during testing
    /// </summary>
    [JsonProperty("api-token")]
    public string ApiToken { get; set; }
    
    /// <summary>
    /// The Content Id of the folder on GoFile to use during testing
    /// </summary>
    /// <remarks>Contents of this folder are volatile</remarks>
    [JsonProperty("test-folder-id")]
    public string TestFolderId { get; set; }
}

// Example json
// {
//     "api-token": "",
//     "test-folder-id": ""
// }