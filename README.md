# EzSaver 
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue)](http://makeapullrequest.com) [![License: MIT](https://img.shields.io/badge/License-MIT-blue)](https://ebukaracer.github.io/ebukaracer/md/LICENSE.html)

A Flexible and Secure Saver for Unity Games, Enabling JSON Serialization and Secure File Storage.

 [View in DocFx](https://ebukaracer.github.io/EzSaver)
## Features
- Supports encryption(AES)
- Custom-type JSON serialization
- Supports file saving of various extensions
- Includes a Menu window for various operations
- Includes a Demo to help you quickly get started

## Installation
_Inside the Unity Editor using the Package Manager:_
- Click the **(+)** button in the Package Manager and select **"Add package from Git URL"** (requires Unity 2019.4 or later).
-  Paste the Git URL of this package into the input box: https://github.com/ebukaracer/EzSaver.git#upm
-  Click **Add** to install the package.
-  If your project uses **Assembly Definitions**, make sure to add a reference to this package under **Assembly Definition References**. 
    - For more help, see [this guide](https://ebukaracer.github.io/ebukaracer/md/SETUPGUIDE.html).

## Dependencies
This package depends on **Newtonsoft Json (v3.2.1)**. If it's not already available in your project, you can install it via the Unity Package Manager:
- Click the **(+)** button and select **"Add package by name"**
- Enter the package name: `com.unity.nuget.newtonsoft-json`
-  Click **Add**

## Setup
After installation, use the menu options:
- `Racer > EzSaver > Create EzSaverConfig?` to initialize and create the required `config` asset, necessary for the package to work.
- `Racer > EzSaver > Import Elements` to import the prebuilt elements(prefabs) of this package, which will speed up your workflow.
## Quick Usage
After importing the package's **Elements**, locate the **EzSaverManager** prefab and add it to the scene where you want to enable save functionality.
#### Initialize a save-file:
``` csharp
internal class Person  
{  
    public int Age { get; set; }  
}

private Person _person;
private int _highscore;
private EzSaverCore _ezSaverCore;

private void Awake()    
{  
    // Initializes and sets up a save-file  
    _ezSaverCore = EzSaverManager.Instance.GetSave("Save.txt");  
      
    // Alternatively, you can initialize it with a JSON string-literal  
    // _ezSaverCore = EzSaverManager.Instance.GetSave(@"{""Highscore"": 1}", isJsonStringLiteral: true);

    // Access previously saved content or use the type's default value  
    _person = _ezSaverCore.Read("Person", new Person());  
    _highscore = _ezSaverCore.Read("Highscore", _highscore);  
}
```

#### Update save data: 
``` csharp
public void AddScore(int amount)  
{
	_highscore += amount;  
	_person.Age = _highscore;
	
	// Store their changes temporarily
	_ezSaverCore  
	    .Write("Person", _person)  
	    .Write("Highscore", _highscore);
}
```

#### Save changes to save-file (📌):
``` csharp
// Find a suitable place to call Save() once.
private void OnDisable()  
{
	// Commit the overall changes to the initialized 'Save.txt' file
	_ezSaverCore.Save();
}
```

#### Clear save data:
``` csharp
_ezSaverCore  
    .Clear("Person", _person)  
    .Clear("Highscore", _highscore);
```

#### Delete save-file:
``` csharp
_ezSaverCore.DeleteFile();
```

## Samples and Best Practices
- Always generate a unique set of keys for each project and back them up securely. Avoid using the default keys provided during installation.
- In the case of any updates to newer versions, use the menu option: `Racer > EzSaver > Import Elements(Force)` 
- Optionally import this package's demo from the package manager's `Samples` tab.
- To remove this package completely(leaving no trace), navigate to: `Racer > EzSaver > Remove package`

## FAQs
**Q1. What platforms are supported?**
> Tested on Android, Windows, and WebGL (see Q2). Probably supported on macOS and iOS.

**Q2. Will it really work for WebGL builds?**
> Partially. Since WebGL doesn't allow file system access, save-files cannot be stored traditionally. As a workaround, you can initialize save data as a string literal and use **PlayerPrefs** to store the final save state.

**Q3. Is save data encrypted? How can I manage the keys?**
> Yes, encryption is supported and optional during initialization. The current encryption keys can be found in the Config Asset under:  
> `Racer > EzSaver > Menu > Config Asset`.

**Q4. What could cause data loss?**
> Imagine this scenario: You deploy a project `xyz` using credentials `abc` (save-keys). Later, if you generate new credentials `cba` for the same project `xyz`, all previous save data tied to `abc` will be overwritten and lost. To prevent this, always back up your credentials and restore them when needed using the Config Asset.

**Q5. Does that mean I should never change the current credentials?**
> Not necessarily! You can generate new credentials anytime during development. If potential data loss isn't a concern, feel free to proceed.

**Q6. Can I modify the contents of the save file?**
> Absolutely — as long as encryption is disabled. Modifying an encrypted save file will cause decryption to fail, resulting in data loss. It’s recommended to only modify the plain JSON string when encryption is turned off.

**Q7. Can different save files have different credentials within the same project?**
> Technically yes, but it’s risky and may lead to data corruption or loss. A safer approach is to generate and back up a unique set of credentials per project — for example:  
> `Project A` → `abc`,  
> `Project B` → `xyz`.  
> However, multiple projects can still share the same credentials if necessary.

**Q8. What if I forget to save after making modifications?**
> If automatic saving is enabled (which it is by default), **EzSaverManager.cs** will handle this for you. Note that automatic saving and on-the-go saving only work if you initialized from a file source. If initialized from a string literal, you'll need to manually define save trigger points.

## [Contributing](https://ebukaracer.github.io/ebukaracer/md/CONTRIBUTING.html) 
Contributions are welcome! Please open an issue or submit a pull request.