# SklepCS-Manager

## Description
SklepCS Manager is a plugin designed to integrate with https://sklepcs.pl/. 

## Features
- Permission management from the database
- Main SMS shop features:
  - Purchasing a plan via SMS
## Dependencies
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases) v159
- [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)

## Instalation
1. Download the [latest release](https://github.com/CS-GEJMERZY/SklepCS-Manager/releases/latest)
2. Unzip the package and upload files to **_csgo/addons/counterstrikesharp/plugins_**

## Configuration
Upon the first launch, the **SklepCS-Manager.json**  file will be automatically created in **_csgo/addons/counterstrikesharp/configs/plugins/SklepCS-Manager_**
```
{
  "Settings": {
    "Prefix": " {red}[Sklepsms] ",
    "LoggingLevel": 3,
    "Database": {
      "DBHostname": "www.yoursite.com",
      "DBPort": 3306,
      "DBDatabase": "sklepcs_maintable",
      "DBUser": "user_123456",
      "DBPassword": "passwordtodb123"
    }
  },
  "Sklepcs": {
    "WebFeaturesEnabled": true,
    "WebsiteURL": "www.sklepcs.pl/yourshop",
    "ApiKey": "1234567890",
    "ServerTag": "server1"
  },
  "Groups": [
    {
      "RequiredFlags": "p",
      "Permissions": [
        "@sklepcs/default",
        "@sklepcs/2"
      ]
    }
  ],
  "ConfigVersion": 1
}
```
### Logging levels
Add up all the wanted log types. Use 0 if you don't want to log anything:
```
Purchase success - 1
Purchase error - 2
Web API  errors - 4
```

## Commands
- **!uslugi**: Displays a list of all active services for a player.
- **!sklepsms**: Opens the main shop menu.
- **!kupsrodkami**: Allows the player to purchase services using wallet money.
- **!kupsms**: Shows information about sending an SMS code.
- **!kodsms**: Receiving services by using an SMS code.
