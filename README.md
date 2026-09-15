# EzSaver 
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue)](http://makeapullrequest.com) [![License: MIT](https://img.shields.io/badge/License-MIT-blue)](https://ebukaracer.github.io/ebukaracer/md/LICENSE.html)

A flexible and secure saver for Unity games, enabling JSON serialization and secure file storage.

 [View in DocFx](https://ebukaracer.github.io/EzSaver)
## Features
- Safe and secure file storage
- Supports encryption (AES)
- Custom-type JSON serialization
- Supports file saving of various extensions
- Menu window for various operations
- Demo to help you quickly get started

## Installation
- Open the Unity Package Manager
- Click the (+) button.
- Select **Install package from git URL**.
- Enter the URL below and click **Install**:
   ```text
   https://github.com/ebukaracer/EzSaver.git#upm
   ```

If your project uses **Assembly Definitions**, add a reference to this package's assembly under **Assembly Definition References**.

For additional setup information, see the [Setup Guide](https://ebukaracer.github.io/ebukaracer/md/SETUPGUIDE.html)

## Dependencies

| Package Name             | ID/URL                                                                                                                            | How to Install using  the Package Manager                                                     |
| ------------------------ | --------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| Newtonsoft Json (v3.2.1) | [com.unity.nuget.newtonsoft-json](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/changelog/CHANGELOG.html) | Select **Install package by name** and enter the *package ID*  and *version* inside the boxes |
| EzUtilities.Common       | https://github.com/ebukaracer/EzUtilities.git#common                                                                              | Select **Install package from git URL** and enter the *package url* inside the box            |

## Setup
After installation, use the Unity Editor menu to set it up.
### 1. Import the package elements
Go to:
```
Racer > EzSaver > Import Elements
```
This imports prebuilt elements included with the package, such as the `EzSaverManager` prefab and optional converters.
### 2. Add to your scene
Go to:
```
Racer > EzSaver > Add EzSaverManager GameObject to Scene
```
The `EzSaverManager` is required when using EzSaver's API.

## Usage Examples

You can then access the singleton instance through:
```
EzSaverManager.Instance
```
### Initialize a save file
You can initialize a save file using `GetSave()`
```csharp
using Racer.EzSaver.Utilities;
using UnityEngine;

public class UsageExample : MonoBehaviour
{
    public class Person
    {
        public int Age { get; set; }
    }

    private Person _person;
    private int _highscore;
    private EzSaverCore _ezSaverCore;

    private void Awake()
    {
        // Initialize the save file.
        _ezSaverCore = EzSaverManager.Instance.GetSave("Save.txt");

        // Alternatively, initialize from a JSON string literal.
        // _ezSaverCore = EzSaverManager.Instance.GetSave(
        //     @"{""Highscore"":1}",
        //     isJsonStringLiteral: true
        // );

        // Read previously saved values.
        // If a value does not exist, the supplied default value is returned.
        _person = _ezSaverCore.Read("Person", new Person());
        _highscore = _ezSaverCore.Read("Highscore", 0);
    }
}
```
### Write save data
Use `Write()` to update values in the current save state.
```csharp
public void AddScore(int amount)
{
    _highscore += amount;
    _person.Age = _highscore;

    _ezSaverCore
        .Write("Person", _person)
        .Write("Highscore", _highscore);
}
```
`Write()` updates the current save data. It does not necessarily write the changes to the physical save file immediately.
### Save changes to the file
Call `Save()` when you want to commit the current changes to the save file.
```csharp
private void OnDisable()
{
    _ezSaverCore.Save();
}
```
You can also enable **Save On Modification** if you want changes to be saved immediately after a `Write()` operation.
### Clear save data
Use `Clear()` to remove individual values from the current save data.
```csharp
_ezSaverCore
    .Clear("Person", _person)
    .Clear("Highscore", _highscore);
```
### Delete a save file
Use `DeleteFile()` to delete the initialized save file.
```csharp
_ezSaverCore.DeleteFile();
```

## Saving Options
EzSaver provides several ways to control when save data is committed.
### Automatic Save on Quit
Enable **Auto Save On Quit** on the `EzSaverManager` to automatically save changes when the application quits.
### Save on Modification
Enable **Save On Modification** to automatically commit changes after a `Write()` operation.
### Manual Saving
If automatic saving is disabled, call:
```csharp
_ezSaverCore.Save();
```
at the appropriate point in your game.

> Automatic saving and modification-based saving work when save data is initialized from a file source. If save data is initialized from a JSON string literal, you need to define your own save trigger points.
### Encryption
EzSaver supports optional AES encryption for save files.

Enable encryption when initializing a save file by using the `useSecurity` parameter.
```csharp
_ezSaverCore = EzSaverManager.Instance.GetSave(
    "Save.txt",
    useSecurity: true
);
```
When encryption is enabled, the contents of the save file should not be manually edited.

## Samples and Best Practices
### Updating the Package
After updating to a newer version, 
- navigate to: `Racer > EzSaver > Import Elements (force)`

This ensures the latest package elements are imported.
### Importing the Demo
A demo is included with the package and can be imported from the Unity Package Manager's **Samples** tab.
### Removing the Package
To remove the package completely, 
- navigate to: `Racer > EzSaver > Remove package`

## FAQs
**What platforms are supported?**
> Tested on Android, Windows, and WebGL (see Q2). Probably supported on macOS and iOS.

**Will it really work for WebGL builds?**
> Partially. Since WebGL doesn't allow file system access, save-files cannot be stored traditionally. As a workaround, you can initialize save data as a string literal and use **PlayerPrefs** to store the final save state.

**Is save data encrypted? How can I manage the keys?**
> Yes, encryption is supported and optional during initialization. The current encryption keys can be found in the Config Asset under:  
> `Assets > Resources > EzSaverConfig`.

**What could cause data loss?**
> Imagine this scenario: You deploy a project `xyz` using credentials `abc` (save-keys). Later, if you generate new credentials `cba` for the same project `xyz`, all previous save data tied to `abc` will be overwritten and lost. To prevent this, always back up your credentials and restore them when needed using the **Config Asset**. 

**Does that mean I should never change the current credentials?**
> Not necessarily! You can generate new credentials anytime during development. If potential data loss isn't a concern, feel free to proceed. Even if credential regeneration causes save data to be overwritten, the system automatically creates a backup of the original save file. This backup uses a naming pattern based on the original filename: 
> For example, if your original save file is `data.json`, the backup will be saved as `data-backup0.json`. 
> This gives you a way to recover the lost data if needed.

**Can I modify the contents of the save file?**
> Absolutely — as long as encryption is disabled. Modifying an encrypted save file will cause decryption to fail, resulting in data loss. It’s recommended to only modify the plain JSON string when encryption is turned off.

**Can different save files have different credentials within the same project?**
> Technically yes, but it’s risky and may lead to data corruption or loss. A safer approach is to generate and back up a unique set of credentials per project — for example:  
> `Project A` → `abc`,  
> `Project B` → `xyz`.  
> However, multiple projects can still share the same credentials if necessary.

**What if I forget to save after making modifications?**
> If automatic saving is enabled (which it is by default), **EzSaverManager.cs** will handle this for you. Just so you know, automatic saving and on-the-go saving only work if you initialized from a file source. If initialized from a string literal, you'll need to define save trigger points manually.

## How to Restore Lost/Overwritten Save Data
First, ensure `Retain Backup File` was toggled on in the **Config Asset**, then:
- Locate the backup file:  
    - Find the backup file (e.g., `data-backup0.json`) in the default save location.
- Recover the original save:
    - Open the backup file.
    - Copy its contents and paste them into your original save file (e.g., overwrite e.g., `data.json` with the contents of `data-backup0.json`).
- Restore the original credentials:
    - Open the **Config Asset**
    - Choose **Restore Credentials File**.
    - From the file menu, select the credentials file that was originally used with your `data.json` save-file (prior to generating the new ones).
- Apply changes:  
    - After restoring the correct credentials for the save-data, the system will recognize and resume from the original saved state.
  
## [Contributing](https://ebukaracer.github.io/ebukaracer/md/CONTRIBUTING.html) 
Contributions are welcome! Please open an issue or submit a pull request.