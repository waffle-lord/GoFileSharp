using System.Diagnostics;
using GoFileSharp;

namespace Tests;

[TestClass]
public class GoFileTests
{
    private static Config _config;
    
    [AssemblyInitialize]
    public static void Setup(TestContext ctx)
    {
        Debug.WriteLine("=== Assembly Setup ===");
        _config = Config.Load();

        GoFile.ApiToken = _config.ApiToken;
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
    public void CreateFolder() {}
    
    [TestMethod]
    public void GetFolderById() {}
    
    [TestMethod]
    public void FindFile() {}
    
    [TestMethod]
    public void FindFolder() {}
    
    [TestMethod]
    public void UploadFile() {}
}