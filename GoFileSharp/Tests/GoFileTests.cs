using System.Diagnostics;
using System.Net;
using System.Text;
using GoFileSharp;
using GoFileSharp.Model;
using GoFileSharp.Model.GoFileData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tests;

[TestClass]
public class GoFileTests
{
    private static GoFile _goFile;
    private static Config _config;
    private static HttpClient _client = new();
    private static FileInfo _testFile = new FileInfo("../../../Data/test.txt");
    private static string _testFolderId;
    private static string _testFolderName = Setup_GetRandomFolderName();

    #region Setup Methods
    /// <summary>
    /// A 2sec delay to help reduce any potential rate limiting during testing
    /// </summary>
    private static async Task DelayTwoSeconds()
    {
        await Task.Delay(2000);
    }

    private static string Setup_GetRandomFolderName()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var nameTail = new char[10];
        var rand = new Random();

        for (int i = 0; i < nameTail.Length; i++)
        {
            nameTail[i] = chars[rand.Next(chars.Length)];
        }

        return $"GFS_TEST_{new string(nameTail)}";
    }

    private static async Task<string> Setup_GetAccountId()
    {
        Debug.WriteLine($"-- [SETUP] Getting Account Id --");
        
        var response = await _client.GetAsync($"https://api.gofile.io/accounts/getid?token={_config.ApiToken}");
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var data = JObject.Parse(content);
        Assert.IsNotNull(content);
        Assert.IsTrue(data["status"].Value<string>() == "ok");
        var accountId = data["data"]["id"].Value<string>();
        
        Assert.IsTrue(!string.IsNullOrWhiteSpace(accountId));

        return accountId;
    }
    private static async Task<string> Setup_GetRootFolder()
    {
        Debug.WriteLine($"-- [SETUP] Getting Root Folder --");

        var accountId = await Setup_GetAccountId();
        var response = await _client.GetAsync($"https://api.gofile.io/accounts/{accountId}?token={_config.ApiToken}");
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var data = JObject.Parse(content);
        Assert.IsNotNull(content);
        Assert.IsTrue(data["status"].Value<string>() == "ok");
        var rootId = data["data"]["rootFolder"].Value<string>();
        
        Assert.IsTrue(!string.IsNullOrWhiteSpace(rootId));

        return rootId;
    }

    private static async Task Setup_CreateTestFolder()
    {
        Debug.WriteLine($"-- [SETUP] Creating Test Folder: {_testFolderName} --");

        var rootId = await Setup_GetRootFolder();
        
        var requestDictionary = new Dictionary<string, string>
        {
            { "token", _config.ApiToken },
            { "parentFolderId", rootId },
            { "folderName", _testFolderName}
        };

        var json = JsonConvert.SerializeObject(requestDictionary);
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.gofile.io/contents/createfolder")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var data = JObject.Parse(content);
        Assert.IsNotNull(content);
        Assert.IsTrue(data["status"].Value<string>() == "ok");
        _testFolderId = data["data"]["folderId"].Value<string>();
        
        Assert.IsTrue(!string.IsNullOrWhiteSpace(_testFolderId));
    }

    public static async Task Setup_RemoveTestFolder()
    {
        Debug.WriteLine($"-- [SETUP] Removing Test Folder: {_testFolderName} --");
        
        var requestParams = new Dictionary<string, string>
        {
            { "token", _config.ApiToken },
            { "contentsId", _testFolderId }
        };

        var json = JsonConvert.SerializeObject(requestParams);

        var request = new HttpRequestMessage(HttpMethod.Delete, "https://api.gofile.io/contents")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        
        var deleted = await _client.SendAsync(request);

        deleted.EnsureSuccessStatusCode();
    }
    #endregion
    
    [AssemblyInitialize]
    public static void Setup(TestContext ctx)
    {
        Debug.WriteLine("=== Assembly Setup ===");
        _config = Config.Load();

        _goFile = new GoFile(new GoFileOptions
        {
            ApiToken = _config.ApiToken
        });
        
        Setup_CreateTestFolder().GetAwaiter().GetResult();
    }

    [AssemblyCleanup]
    public static void Teardown()
    {
        Debug.WriteLine("=== Assembly Teardown ===");
        Setup_RemoveTestFolder().GetAwaiter().GetResult();
        _client.Dispose();
    }

    [TestMethod]
    public async Task GetRootFolder()
    {
        await DelayTwoSeconds();
        var root = await _goFile.GetMyRootFolderAsync();

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
        var testFolder = await _goFile.GetFolderAsync(_testFolderId);

        var createdFolder = await testFolder.CreateFolderAsync(folderName);

        Assert.IsNotNull(createdFolder);
        Assert.IsNotNull(createdFolder.Id);
        Assert.IsTrue(createdFolder.ParentFolderId == testFolder.Id);
        Assert.IsTrue(createdFolder.Name == folderName);
        Assert.IsTrue(createdFolder.Type == "folder");
    }

    [TestMethod]
    public async Task GetFolderById()
    {
        await DelayTwoSeconds();

        var folder = await _goFile.GetFolderAsync(_testFolderId);
        
        Assert.IsNotNull(folder);
        Assert.IsTrue(_testFolderId == folder.Id);
        Assert.IsTrue(!string.IsNullOrWhiteSpace(folder.Name));
        Assert.IsTrue(folder.Type == "folder");
    }

    [TestMethod]
    public async Task FindFile()
    {
        await DelayTwoSeconds();

        var testFolder = await _goFile.GetFolderAsync(_testFolderId);
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
        var testFolder = await _goFile.GetFolderAsync(_testFolderId);
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
        
        var testFolder = await _goFile.GetFolderAsync(_testFolderId);

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

        var testFolder = await _goFile.GetFolderAsync(_testFolderId);
        var copyFileFolder = await testFolder.CreateFolderAsync("copyFileToSource");
        var testFile = await copyFileFolder.UploadIntoAsync(_testFile);
        var copyTargetFolder = await copyFileFolder.CreateFolderAsync("copyFileToTarget");

        var copied = await testFile.CopyToAsync(copyTargetFolder);
        
        await copyTargetFolder.RefreshAsync();

        var copiedFile = copyTargetFolder.FindFile(_testFile.Name);
        
        Assert.IsTrue(copied);
        Assert.IsNotNull(copiedFile);
        Assert.IsTrue(copyTargetFolder.Children.Count > 0);
        Assert.IsTrue(_testFile.Length == copiedFile.Size);
    }
    
    [TestMethod]
    public async Task CopyFolderTo()
    {
        await DelayTwoSeconds();

        var testFolder = await _goFile.GetFolderAsync(_testFolderId);
        var sourceFolder = await testFolder.CreateFolderAsync("copyFolderToSource");
        var targetFolder = await testFolder.CreateFolderAsync("copyFolderToTarget");
    
        await sourceFolder.UploadIntoAsync(_testFile);
        
        var copied = await sourceFolder.CopyToAsync(targetFolder);
    
        await sourceFolder.RefreshAsync();
        await targetFolder.RefreshAsync();
        
        Assert.IsTrue(copied);
        Assert.IsNotNull(sourceFolder.Children);
        Assert.IsNotNull(targetFolder.Children);
        Assert.IsTrue(sourceFolder.Children.Count > 0);
        Assert.IsTrue(targetFolder.Children.Count > 0);
    
        var targetTestFolder = await targetFolder.FindFolderAsync(targetFolder.Children[0].Name);
        
        Assert.IsNotNull(targetTestFolder);
        Assert.IsTrue(targetTestFolder.Children.Count > 0);
        
        var targetTestFileName = targetTestFolder.Children[0].Name;
        
        Assert.IsTrue(sourceFolder.Children[0].Name == targetTestFileName);
    }
    
    [TestMethod]
    public async Task CopyFolderInto()
    {
        await DelayTwoSeconds();

        var testFolder = await _goFile.GetFolderAsync(_testFolderId);
        var sourceFolder = await testFolder.CreateFolderAsync("copyIntoSource");
        var targetFolder = await testFolder.CreateFolderAsync("copyIntoTarget");
    
        await sourceFolder.UploadIntoAsync(_testFile);
        await sourceFolder.RefreshAsync();
    
        var copied = await targetFolder.CopyIntoAsync([sourceFolder]);

        await sourceFolder.RefreshAsync();
        await targetFolder.RefreshAsync();
        
        Assert.IsTrue(copied);
        Assert.IsNotNull(sourceFolder.Children);
        Assert.IsNotNull(targetFolder.Children);
        Assert.IsTrue(sourceFolder.Children.Count > 0);
        Assert.IsTrue(targetFolder.Children.Count > 0);
        
        var targetTestFolder = await targetFolder.FindFolderAsync(sourceFolder.Name);

        Assert.IsNotNull(targetTestFolder);
        Assert.IsTrue(targetTestFolder.Children.Count > 0);
        
        var targetTestFileName = targetTestFolder.Children[0].Name;
        
        Assert.IsTrue(sourceFolder.Children[0].Name == targetTestFileName);
    }

    [TestMethod]
    public async Task SetFileOptions()
    {
        await DelayTwoSeconds();

        var newName = "my new name.txt";

        var testFolder = await _goFile.GetFolderAsync(_testFolderId);
        var fileOptionsFolder = await testFolder.CreateFolderAsync("fileOptions");
        var testFile = await fileOptionsFolder.UploadIntoAsync(_testFile);

        var link = await testFile.AddDirectLink();
        
        Assert.IsTrue(await testFile.SetName(newName));
        
        Assert.IsTrue(testFile.Name == newName);
        
        Assert.IsNotNull(link);
        Assert.IsNotNull(link.Id);
        Assert.IsTrue(testFile.DirectLinks.Count > 0);
        Assert.IsTrue(testFile.DirectLinks.First().Id == link.Id);
    }

    [TestMethod]
    public async Task SetFolderOptions()
    {
        var desc = "This is a description";
        var tags = new List<string>() { "testing", "stuff" };
        var pass = "test123";
        var newName = "my new name folder";
        DateTimeOffset expiry = DateTime.Now.AddDays(5);
        
        await DelayTwoSeconds();

        var testFolder = await _goFile.GetFolderAsync(_testFolderId);
        var folderOptionsFolder = await testFolder.CreateFolderAsync("folderOptions");

        var link = await folderOptionsFolder.AddDirectLink();

        Assert.IsTrue(await folderOptionsFolder.SetName(newName));
        Assert.IsTrue(await folderOptionsFolder.SetDescription(desc));
        Assert.IsTrue(await folderOptionsFolder.SetTags(tags));
        Assert.IsTrue(await folderOptionsFolder.SetPublic(true));
        Assert.IsTrue(await folderOptionsFolder.SetExpire(expiry));
        Assert.IsTrue(await folderOptionsFolder.SetPassword(pass));
        
        Assert.IsTrue(folderOptionsFolder.Name == newName);
        Assert.IsNotNull(folderOptionsFolder.Description);
        Assert.IsNotNull(folderOptionsFolder.Expire);
        Assert.IsNotNull(folderOptionsFolder.Tags);
        Assert.IsTrue(folderOptionsFolder.IsPublic);
        Assert.IsTrue(folderOptionsFolder.HasPassword);
        Assert.IsTrue(folderOptionsFolder.Tags == string.Join(",", tags));
        Assert.IsTrue(folderOptionsFolder.Description == desc);
        Assert.IsTrue(folderOptionsFolder.Expire == expiry.ToUnixTimeSeconds());
        
        Assert.IsNotNull(link);
        Assert.IsNotNull(link.Id);
        Assert.IsTrue(folderOptionsFolder.DirectLinks.Count > 0);
        Assert.IsTrue(folderOptionsFolder.DirectLinks.First().Id == link.Id);
    }
    
    [TestMethod]
    public async Task DeleteFolder()
    {
        await DelayTwoSeconds();

        var testFolder = await _goFile.GetFolderAsync(_testFolderId);
        var delFolder = await testFolder.CreateFolderAsync("delFolder");
        await delFolder.UploadIntoAsync(_testFile);

        var delInfo = await delFolder.DeleteAsync();
        Assert.IsTrue(delInfo.Count > 0);
        foreach (var info in delInfo.Values)
        {
            Assert.IsTrue(info.IsOk());
        }
        Assert.IsNull(await _goFile.GetFolderAsync(delFolder.Id, true));
    }

    [TestMethod]
    public async Task DeleteFile()
    {
        await DelayTwoSeconds();

        var testFolder = await _goFile.GetFolderAsync(_testFolderId);
        var delFileFolder = await testFolder.CreateFolderAsync("delFile");
        var delFile = await delFileFolder.UploadIntoAsync(_testFile);

        var delInfo = await delFile.DeleteAsync();
        
        Assert.IsTrue(delInfo.Count > 0);
        foreach (var info in delInfo.Values)
        {
            Assert.IsTrue(info.IsOk());
        }
        await delFileFolder.RefreshAsync();
        Assert.IsTrue(delFileFolder.Children.Count == 0);
    }

    [TestMethod]
    public async Task DirectLinkOptions()
    {
        await DelayTwoSeconds();

        var testFolder = await _goFile.GetFolderAsync(_testFolderId);
        var linkOptionsFolder = await testFolder.CreateFolderAsync("linkOptions");
        var linkOptionsFile = await linkOptionsFolder.UploadIntoAsync(_testFile);

        DateTimeOffset expireTime = DateTime.Now.AddDays(5);
        string[] auths = ["test:blah", "another:thing"];
        string[] domains = ["google.com", "waffle-lord.com"];
        string[] sourceIps = ["192.168.1.1", "192.168.1.2"];

        var optionsBuilder = new DirectLinkOptionsBuilder()
            .WithExpireTime(expireTime)
            .AddAuth("test", "blah")
            .AddAuth("another", "thing")
            .AddAllowedDomain("google.com")
            .AddAllowedDomain("waffle-lord.com")
            .AddAllowedSourceIp(IPAddress.Parse("192.168.1.1"))
            .AddAllowedSourceIp(IPAddress.Parse("192.168.1.2"));


        var link = await linkOptionsFile.AddDirectLink(optionsBuilder);
        
        Assert.IsNotNull(link);
        Assert.IsTrue(link.ExpireTime == expireTime.ToUnixTimeSeconds());

        foreach (var ip in sourceIps)
        {
            Assert.IsTrue(link.SourceIpsAllowed.Contains(ip));
        }
        
        foreach (var domain in domains)
        {
            Assert.IsTrue(link.DomainsAllowed.Contains(domain));
        }
        
        foreach (var login in auths)
        {
            Assert.IsTrue(link.Auth.Contains(login));
        }
    }
}