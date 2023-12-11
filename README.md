# SklepCS-Manager

## Description
SklepCS Manager is a plugin designed to integrate with https://sklepcs.pl/. 

## Features
- Permission management from the database
- Main SMS shop features:
  - Purchasing a plan via SMS
## Dependencies
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases)
- [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)

## Instalation
1. Download the [latest release](https://github.com/CS-GEJMERZY/SklepCS-Manager/releases/latest)
2. Unzip the package and upload files to **_csgo/addons/counterstrikesharp/plugins_**

## Configuration
Upon the first launch, the _**SklepCS-Manager.json**_  file will be automatically created in **_csgo/addons/counterstrikesharp/configs/plugins/SklepCS-Manager**

## Commands
- **css_uslugi** - Displays all active services for the player.
usage: !uslugi
- **css_sklepsms** - Lists available plans along with their prices.
- - usage: !sklepsms
- **css_kupusluge** - Prints information about SMS, operator, etc.
- - usage: !kupusluge <id>
- **css_kupusluge** - Performs the action of buying a plan 
- - usage: !kupsms <id> <smscode>

!
