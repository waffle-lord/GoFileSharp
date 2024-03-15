[![Run Tests](https://github.com/waffle-lord/GoFileSharp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/waffle-lord/GoFileSharp/actions/workflows/dotnet.yml) [![Publish Nuget](https://github.com/waffle-lord/GoFileSharp/actions/workflows/nuget.yml/badge.svg?branch=main)](https://github.com/waffle-lord/GoFileSharp/actions/workflows/nuget.yml) <img alt="NuGet Version" src="https://img.shields.io/nuget/v/GoFileSharp?label=GoFileSharp Nuget">



# GoFileSharp

A .NET library for the GoFile.io API written in C#

[Available on Nuget](https://www.nuget.org/packages/GoFileSharp)

# Requirments
- NewtonSoft.Json

# Examples

### Setting up
You can create a new `GoFile` instance to access the API, optionally passing in a GoFileOptions object

Options Include:
- `ApiToken`: The token to use with all requests, defaults to `null`
- `PreferredZone`: The preferred zone to use when uploading files, defaults to `Any`

> [!WARNING]
> Not providing an api token will mean limited api access
```cs
using GoFileSharp;

var goFile = new GoFile(new GoFileOptions
{
    ApiToken = "123-456-7890",
    PreferredZone = ServerZone.NorthAmerica
});
```

### Get your account details
```cs
var account = await goFile.GetMyAccountAsync();
```

### Get your root folder
```cs
var rootFolder = await goFile.GetMyRootFolderAsync();
```

### Creating a folder
```cs
var newFolder = await rootFolder.CreateFolderAsync("My New Folder");
```

### Uploading a file
```cs
var fileInfo = new FileInfo(@"c:\path\to\a\file.txt");

// optional progress object for handling progress updates
var uploadProgress = new Progress<double>((percent) => 
{
    Console.WriteLine($"Upload: {fileInfo.Name} @ {percent}%");
});

var uploadedFile = await newFolder.UploadIntoAsync(fileInfo, uploadProgress);
```

### Adding/Updating a direct link
You can add a direct link to files and folders and optionally pass in a `DirectLinkOptinosBuilder` to set the options on the link.

If you already have a direct link, you can also update it
```cs
var optionsBuuilder = new DirectLinkOptionsBuilder()
    .WithExpireTime(DateTime.Now.AddDays(5))
    .AddAllowedDomain("waffle-lord.com")
    .AddAuth("user", "password");

// you can also add the options when creating the initial link here
var directLink = await uploadedFile.AddDirectLink();

var updatedLink = await uploadedFile.UpdateDirectLink(directLink, optionsBuuilder);
```

### Removing a direct link
```cs
// returns true if the link was removed, otherwise false
var removed = await uploadedFile.RemoveDirectLink(updatedLink);
```

### Setting file/folder options
Setting an option will always return a bool value. True is success, otherwise false
```cs
// files can only have their name updated
var ok = await uploadedFile.SetName("my new name.txt");

// folders have quite a few more options
ok = await newFolder.SetName("some name here");
ok = await newFolder.SetDescription("my cool folder description");
ok = await newFolder.SetExpire(DateTime.Now.AddDays(5));
ok = await newFolder.SetPublic(true);
ok = await newFolder.SetPassword("password");
ok = await newFolder.SetTags(new[] {"tag1", "tag2", "tag3"}.ToList());
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

### Copying content
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
