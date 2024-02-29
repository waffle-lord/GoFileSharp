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
    private static FileInfo _testFile = new FileInfo("../../../Data/test.txt");

    private static async Task ClearTestFolder()
    {
        Debug.WriteLine("-- Clearing Test Folder --");
        var response =
            await _client.GetAsync(
                $"https://api.gofile.io/getContent?token={_config.ApiToken}&contentId={_config.TestFolderId}&cache=false");

        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail("Test folder does not exist");
        }

        var content = await response.Content.ReadAsStringAsync();

        var data = JObject.Parse(content);

        Assert.IsNotNull(data);
        Assert.IsTrue(data["status"].Value<string>() == "ok");

        var ids = data["data"]["childs"].ToObject<string[]>();
        Assert.IsNotNull(ids);
        Assert.IsInstanceOfType<string[]>(ids);

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
        Debug.WriteLine("=== Assembly Teardown ===");
        ClearTestFolder().GetAwaiter().GetResult();
        _client.Dispose();
    }

    [TestMethod]
    public async Task GetRootFolder()
    {
        var root = await GoFile.GetMyRootFolderAsync();

        Assert.IsNotNull(root);
        Assert.IsTrue(root.Name == "root");
        Assert.IsTrue(root.IsRoot);
        Assert.IsTrue(root.ParentFolderId == null);
        Assert.IsTrue(root.Type == "folder");
    }

    [TestMethod]
    public async Task CreateFolder()
    {
        var folderName = "createTest";
        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);

        var createdFolder = await testFolder.CreateFolderAsync(folderName);

        Assert.IsNotNull(createdFolder);
        Assert.IsTrue(createdFolder.ParentFolderId == testFolder.Id);
        Assert.IsTrue(createdFolder.Name == folderName);
        Assert.IsTrue(createdFolder.Type == "folder");
    }

    [TestMethod]
    public async Task GetFolderById()
    {
        var folder = await GoFile.GetFolderAsync(_config.TestFolderId);
        
        Assert.IsNotNull(folder);
        Assert.IsTrue(_config.TestFolderId == folder.Id);
        Assert.IsTrue(!string.IsNullOrWhiteSpace(folder.Name));
        Assert.IsTrue(folder.Type == "folder");
    }

    [TestMethod]
    public async Task FindFile()
    {
        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);
        var findFileFolder = await testFolder.CreateFolderAsync("findFile");

        await findFileFolder.UploadIntoAsync(_testFile);

        await findFileFolder.RefreshAsync();

        var foundFile = findFileFolder.FindFile(_testFile.Name);
        
        Assert.IsNotNull(foundFile);
        Assert.IsTrue(foundFile.Name == _testFile.Name);
        Assert.IsTrue(foundFile.Type == "file");
        Assert.IsTrue(foundFile.Size == _testFile.Length);
    }

    [TestMethod]
    public async Task FindFolder()
    {
        var name = "searchTest";
        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);
        await testFolder.CreateFolderAsync(name);

        await testFolder.RefreshAsync();

        var searchTestFolder = await testFolder.FindFolderAsync(name);
        
        Assert.IsNotNull(searchTestFolder);
        Assert.IsTrue(searchTestFolder.Name == name);
        Assert.IsTrue(searchTestFolder.Type == "folder");
    }

    [TestMethod]
    public async Task UploadFile()
    {
        Debug.WriteLine($"Uploading: {_testFile.FullName}");
        
        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);

        var uploadedFile = await testFolder.UploadIntoAsync(_testFile);
        
        Assert.IsNotNull(uploadedFile);
        Assert.IsTrue(uploadedFile.Name == _testFile.Name);
        Assert.IsTrue(uploadedFile.Type == "file");
        Assert.IsTrue(uploadedFile.Size == _testFile.Length);
    }

    [TestMethod]
    public async Task CopyFolderTo()
    {
        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);
        var sourceFolder = await testFolder.CreateFolderAsync("copyToSource");
        var targetFolder = await testFolder.CreateFolderAsync("copyToTarget");

        await sourceFolder.UploadIntoAsync(_testFile);

        // todo: check if /copyContent endpoint recurses folder contents
        var copied = await sourceFolder.CopyToAsync(targetFolder);

        await sourceFolder.RefreshAsync();
        await targetFolder.RefreshAsync();
        
        Assert.IsTrue(copied);
        Assert.IsNotNull(sourceFolder.Contents);
        Assert.IsNotNull(targetFolder.Contents);
        Assert.IsTrue(sourceFolder.Contents.Count > 0);
        Assert.IsTrue(targetFolder.Contents.Count > 0);
        Assert.IsTrue(sourceFolder.Contents[0].Name == targetFolder.Contents[0].Name);
    }

    [TestMethod]
    public async Task CopyFolderInto()
    {
        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);
        var sourceFolder = await testFolder.CreateFolderAsync("copyIntoSource");
        var targetFolder = await testFolder.CreateFolderAsync("copyIntoTarget");

        await targetFolder.UploadIntoAsync(_testFile);
        await targetFolder.RefreshAsync();

        var copied = await sourceFolder.CopyIntoAsync(targetFolder.Contents.ToArray());
        
        Assert.IsTrue(copied);
        Assert.IsNotNull(sourceFolder.Contents);
        Assert.IsNotNull(targetFolder.Contents);
        Assert.IsTrue(sourceFolder.Contents.Count > 0);
        Assert.IsTrue(targetFolder.Contents.Count > 0);
        Assert.IsTrue(sourceFolder.Contents[0].Name == targetFolder.Contents[0].Name);
    }
}