using System.Diagnostics;
using GoFileSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tests;

[TestClass]
public class GoFileTests
{
    private static Config _config;
    private static HttpClient _client = new();

    private static async Task ClearTestFolder()
    {
        Debug.WriteLine("=== Clearing Test Folder ===");
        var response =
            await _client.GetAsync(
                $"https://api.gofile.io/getContent?token={_config.ApiToken}&contentId={_config.TestFolderId}");

        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail("Test folder does not exist");
        }

        var content = await response.Content.ReadAsStringAsync();

        var data = JObject.Parse(content);

        Assert.IsNotNull(data);
        Assert.IsTrue(data["status"].Value<string>() == "ok");
        
        // todo: convert jarray to string[]
        var ids = data["childs"]["childs"].Value<JArray>();
        Assert.IsNotNull(ids);

        if (ids.Length == 0)
        {
            return;
        }
        
        Debug.WriteLine($" -> Deleting: {string.Join('|', ids)}");

        var requestDictionary = new Dictionary<string, string>
        {
            { "token", _config.ApiToken },
            { "contentsId", string.Join(',', ids) }
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "https://api.gofile.io/deleteContent")
        {
            Content = new FormUrlEncodedContent(requestDictionary)
        };

        var deleted = await _client.SendAsync(request);

        Assert.IsTrue(deleted.IsSuccessStatusCode);

        var deletedContent = await deleted.Content.ReadAsStringAsync();
        var delResponse = JObject.Parse(deletedContent);
        Assert.IsNotNull(delResponse);
        Assert.IsTrue(delResponse["status"].Value<string>() == "ok");
    }

    [AssemblyInitialize]
    public static void Setup(TestContext ctx)
    {
        Debug.WriteLine("=== Assembly Setup ===");
        _config = Config.Load();

        GoFile.ApiToken = _config.ApiToken;

        ClearTestFolder().GetAwaiter().GetResult();
    }

    [AssemblyCleanup]
    public static void Teardown()
    {
        ClearTestFolder().GetAwaiter().GetResult();
        _client.Dispose();
    }

    [TestMethod]
    public async Task GetRootFolder()
    {
        var root = await GoFile.GetMyRootFolderAsync();

        Assert.IsNotNull(root);
        Assert.IsNotNull(root.Name == "root");
        Assert.IsTrue(root.IsRoot);
        Assert.IsTrue(root.ParentFolderId == null);
    }

    [TestMethod]
    public async Task CreateFolder()
    {
        var folderName = "createTest";
        var testFolder = await GoFile.GetFolder(_config.TestFolderId);

        var createdFolder = await testFolder.CreateFolderAsync(folderName);

        Assert.IsNotNull(createdFolder);
        Assert.IsTrue(createdFolder.ParentFolderId == testFolder.Id);
        Assert.IsTrue(createdFolder.Name == folderName);
    }

    [TestMethod]
    public void GetFolderById()
    {
    }

    [TestMethod]
    public void FindFile()
    {
    }

    [TestMethod]
    public void FindFolder()
    {
    }

    [TestMethod]
    public void UploadFile()
    {
    }
}