# EzSaver 
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue)](http://makeapullrequest.com) [![License: MIT](https://img.shields.io/badge/License-MIT-blue)](https://ebukaracer.github.io/ebukaracer/md/LICENSE.html)

A Flexible and Secure Saver for Unity Games, Enabling JSON Serialization and Secure File Storage.

 [Read Docs](https://ebukaracer.github.io/EzSaver)
## Features
- Supports encryption/decryption(AES)
- Custom-type JSON serialization/deserialization
- Supports file saving of various extensions
- Includes a Demo to help you quickly get started.

## Installation
 *In unity editor inside package manager:*
- Hit `(+)`, choose `Add package from Git URL`(Unity 2019.4+)
- Paste the `URL` for this package inside the box: https://github.com/ebukaracer/EzSaver.git#upm
- Hit `Add`
- If you're using assembly definition in your project, be sure to add this package's reference under: `Assembly Definition References` or check out [this](https://ebukaracer.github.io/ebukaracer/md/SETUPGUIDE.html)

## Quick Usage

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
	// Initializes and sets up a save file
	_ezSaverCore = new EzSaverCore("Save.txt");
	
	// Access previously saved content or use type's default values
	_person = _ezSaverCore.Read("Person", new Person());
	_highscore = _ezSaverCore.Read("Highscore", _highscore);
}
```

#### Update data to be saved: 
``` csharp
public void AddScore(int amount)  
{
	_highscore += amount;  
	_person.Age = _highscore;
}
```

#### Store changes temporarily:
``` csharp
_ezSaverCore  
    .Write("Person", _person)  
    .Write("Highscore", _highscore);
```

#### Save changes to save-file (📌):
``` csharp
// Find a suitable method to call Save() once.
private void OnDestroy()  
{
	_ezSaverCore.Save();
}
```

#### Remove save-items:
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
Check out this package's sample scene by importing it from the package manager *sample's tab* and exploring the scripts for the recommended approach for saving and loading data easily.

*To remove this package completely(leaving no trace), navigate to: `Racer > EzSaver > Remove package`*

## Dependencies
This package depends on **newtonsoft-json; v3.2.1**, install from the unity package manager:
1. Hit `(+)` and select `Add package by name` 
2. Paste the package name: `com.unity.nuget.newtonsoft-json` 
3. Hit `Add`

## [Contributing](https://ebukaracer.github.io/ebukaracer/md/CONTRIBUTING.html) 
Contributions are welcome! Please open an issue or submit a pull request.