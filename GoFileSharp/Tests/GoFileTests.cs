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

    /// <summary>
    /// A 2sec delay to help reduce any potential rate limiting during testing
    /// </summary>
    private static async Task DelayTwoSeconds()
    {
        await Task.Delay(2000);
    }

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
        _client.Dispose();
    }

    [TestMethod]
    public async Task GetRootFolder()
    {
        await DelayTwoSeconds();
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
        await DelayTwoSeconds();

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
        await DelayTwoSeconds();

        var folder = await GoFile.GetFolderAsync(_config.TestFolderId);
        
        Assert.IsNotNull(folder);
        Assert.IsTrue(_config.TestFolderId == folder.Id);
        Assert.IsTrue(!string.IsNullOrWhiteSpace(folder.Name));
        Assert.IsTrue(folder.Type == "folder");
    }

    [TestMethod]
    public async Task FindFile()
    {
        await DelayTwoSeconds();

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
        await DelayTwoSeconds();

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
        await DelayTwoSeconds();

        Debug.WriteLine($"Uploading: {_testFile.FullName}");
        
        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);

        var uploadedFile = await testFolder.UploadIntoAsync(_testFile);
        
        Assert.IsNotNull(uploadedFile);
        Assert.IsTrue(uploadedFile.Name == _testFile.Name);
        Assert.IsTrue(uploadedFile.Type == "file");
        Assert.IsTrue(uploadedFile.Size == _testFile.Length);
    }

    [TestMethod]
    public async Task CopyFileTo()
    {
        await DelayTwoSeconds();

        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);
        var copyFileFolder = await testFolder.CreateFolderAsync("copyFileToSource");
        var testFile = await copyFileFolder.UploadIntoAsync(_testFile);
        var copyTargetFolder = await copyFileFolder.CreateFolderAsync("copyFileToTarget");

        var copied = await testFile.CopyToAsync(copyTargetFolder);
        
        await copyTargetFolder.RefreshAsync();

        var copiedFile = copyTargetFolder.FindFile(_testFile.Name);
        
        Assert.IsTrue(copied);
        Assert.IsNotNull(copiedFile);
        Assert.IsTrue(copyTargetFolder.Contents.Count > 0);
        Assert.IsTrue(_testFile.Length == copiedFile.Size);
    }
    
    [TestMethod]
    public async Task CopyFolderTo()
    {
        await DelayTwoSeconds();

        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);
        var sourceFolder = await testFolder.CreateFolderAsync("copyFolderToSource");
        var targetFolder = await testFolder.CreateFolderAsync("copyFolderToTarget");
    
        await sourceFolder.UploadIntoAsync(_testFile);
        
        var copied = await sourceFolder.CopyToAsync(targetFolder);
    
        await sourceFolder.RefreshAsync();
        await targetFolder.RefreshAsync();
        
        Assert.IsTrue(copied);
        Assert.IsNotNull(sourceFolder.Contents);
        Assert.IsNotNull(targetFolder.Contents);
        Assert.IsTrue(sourceFolder.Contents.Count > 0);
        Assert.IsTrue(targetFolder.Contents.Count > 0);
    
        var targetTestFolder = await targetFolder.FindFolderAsync(targetFolder.Contents[0].Name);
        
        Assert.IsNotNull(targetTestFolder);
        Assert.IsTrue(targetTestFolder.Contents.Count > 0);
        
        var targetTestFileName = targetTestFolder.Contents[0].Name;
        
        Assert.IsTrue(sourceFolder.Contents[0].Name == targetTestFileName);
    }
    
    [TestMethod]
    public async Task CopyFolderInto()
    {
        await DelayTwoSeconds();

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
        
        var targetTestFolder = await targetFolder.FindFolderAsync(targetFolder.Contents[0].Name);

        Assert.IsNotNull(targetTestFolder);
        Assert.IsTrue(targetTestFolder.Contents.Count > 0);
        
        var targetTestFileName = targetTestFolder.Contents[0].Name;
        
        Assert.IsTrue(sourceFolder.Contents[0].Name == targetTestFileName);
    }

    [TestMethod]
    public async Task SetFileOptions()
    {
        await DelayTwoSeconds();

        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);
        var fileOptionsFolder = await testFolder.CreateFolderAsync("fileOptions");
        var testFile = await fileOptionsFolder.UploadIntoAsync(_testFile);

        var directLinkSet = await testFile.SetDirectLink(true);

        await testFile.RefreshAsync();
        
        Assert.IsTrue(directLinkSet);
        Assert.IsNotNull(testFile.DirectLink);
    }

    [TestMethod]
    public async Task SetFolderOptions()
    {
        var desc = "This is a description";
        var tags = new List<string>() { "testing", "stuff" };
        var pass = "test123";
        var expiry = DateTime.Now.AddDays(5);
        
        await DelayTwoSeconds();

        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);
        var folderOptionsFolder = await testFolder.CreateFolderAsync("folderOptions");

        Assert.IsTrue(await folderOptionsFolder.SetDescription(desc));
        Assert.IsTrue(await folderOptionsFolder.SetTags(tags));
        Assert.IsTrue(await folderOptionsFolder.SetPublic(true));
        Assert.IsTrue(await folderOptionsFolder.SetExpire(expiry));
        Assert.IsTrue(await folderOptionsFolder.SetPassword(pass));
    }
    
    [TestMethod]
    public async Task DeleteFolder()
    {
        await DelayTwoSeconds();

        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);
        var delFolder = await testFolder.CreateFolderAsync("delFolder");

        Assert.IsTrue(await delFolder.DeleteAsync());
        Assert.IsNull(await GoFile.GetFolderAsync(delFolder.Id, true));
    }

    [TestMethod]
    public async Task DeleteFile()
    {
        await DelayTwoSeconds();

        var testFolder = await GoFile.GetFolderAsync(_config.TestFolderId);
        var delFileFolder = await testFolder.CreateFolderAsync("delFile");
        var delFile = await delFileFolder.UploadIntoAsync(_testFile);
        
        Assert.IsTrue(await delFile.DeleteAsync());
        await delFileFolder.RefreshAsync();
        Assert.IsTrue(delFileFolder.Contents.Count == 0);
    }
}