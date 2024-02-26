namespace Tests;

[TestClass]
public class GoFileTests
{
    [AssemblyInitialize]
    public void Setup(TestContext ctx)
    {
        //todo: load config data
    }

    [AssemblyCleanup]
    public void TearDown(TestContext ctx)
    {
        //todo: ya mum
    }
    
    [TestMethod]
    public void GetRootFolder(){}
    
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