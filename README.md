# GoFileSharp

A .NET library for the GoFile.io API written in C#

# Requirments
- NewtonSoft.Json

# Examples

### Setting the API token
The `GoFile.ApiToken` will be used for all requests using a `GoFileFolder` or `GoFileFile` object.
```cs
using GoFileSharp;

GoFile.ApiToken = "123-456-789-101112";
```

### Get your root folder
```cs
var rootFolder = await GoFile.GetMyRootFolderAsync();
```

### Creating a folder
```cs
var newFolder = await rootFolder.CreateFolderAsync("My New Folder");
```

### Uploading a File
```cs
var fileInfo = new FileInfo(@"c:\path\to\a\file.txt");

// optional progress object for handling progress updates
var uploadProgress = new Progress<double>((percent) => 
{
    Console.WriteLine($"Upload: {fileInfo.Name} @ {percent}%");
});

var uploadedFile = await newFolder.UploadIntoAsync(fileInfo, uploadProgress);
```

### Downloading a file
```cs
var destinationFile = new FileInfo(@"c:\path\to\save\file.txt");

// optional progress object for handling progress updates
var downloadProgress = new Progress<double>((percent) =>
{
    Console.WriteLine($"Download: {destinationFile.Name} @ {percent}%");
});

var downloadResult = await uploadedFile.DownloadAsync(destinationFile, false, uploadProgress);
```

### Getting a specific file or folder inside another folder
```cs
var someFile = rootFolder.FindFile("somefile.txt");
var someFolder = await rootFolder.FindFolderAsync("Some Folder");
```

### Copying Content
```cs
// Copy data into a folder object
var myDestFolder = await rootFolder.FindFolderAsync("my destination folder");

var copyResult = await myDestFolder.CopyIntoAsync(new GoFileSharp.Interfaces.IContent[] { someFile, someFolder });

// Or copy content to a destination folder
var fileCopyResult = await someFile.CopyToAsync(myDestFolder);
var folderCopyReult = await someFolder.CopyToAsync(myDestFolder);
```

### Delete a file or folder
```cs
var fileDeleteResult = await someFile.DeleteAsync();
var folderDeleteResult = await someFolder.DeleteAsync();
```
