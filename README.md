<img src="Design/banner.jpg" style="margin-bottom:10px" />

# Meadow.Foundation

The [Meadow.Foundation](http://developer.wildernesslabs.co/Meadow/Meadow.Foundation/Peripherals/) peripherals library is an open source repository of drivers and libraries that streamline and simplifiy adding hardware to your .NET Meadow IoT application.

Meadow.Foundation makes the task of building connected things easy with Meadow by providing a unified driver and library framework that includes drivers and abstractions for common peripherals such as: sensors, displays, motors, cameras and more. Additionally, it includes utility functions and helpers for common tasks when building connected things.

## Repositories 

Meadow.Foundation is currently split into three (3) GitHub repos:
1. [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation/) which contains the majority of the peripheral and library source code
2. [Meadow.Foundation.Featherwings](https://github.com/WildernessLabs/Meadow.Foundation.Featherwings/) contains drivers for hardware that conforms to the [Adafruit Featherwing](https://learn.adafruit.com/adafruit-feather/) form factor
3. [Meadow.Foundation.Grove](https://github.com/WildernessLabs/Meadow.Foundation.Grove/) contains drivers for [Seeed Studio Grove](https://www.seeedstudio.com/grove.html) modular hardware peripherals

## Repository Status

| Feature | Branch | Status |
| --- | --- | --- |
| Drivers | `develop` | [![Build Status](https://dev.azure.com/WildernessLabs/Meadow/_apis/build/status/WildernessLabs.Meadow.Foundation?repoName=WildernessLabs%2FMeadow.Foundation&branchName=develop)](https://dev.azure.com/WildernessLabs/Meadow/_build/latest?definitionId=6&repoName=WildernessLabs%2FMeadow.Foundation&branchName=develop) |
| Maple | `develop` | [![Maple Build](https://github.com/WildernessLabs/Meadow.Foundation/actions/workflows/build_maple.yml/badge.svg?branch=develop)](https://github.com/WildernessLabs/Meadow.Foundation/actions/workflows/build_maple.yml) |

## Requesting New Drivers

If you have a need for a driver that we don't yet support, you have a couple options:

- Use an existing, similar driver as a template for your new driver.  We accept pull requests, but don't require them.
- Open a new item on the [Issues Tab](https://github.com/WildernessLabs/Meadow.Foundation/issues) and request the driver so we can prioritize it.

# Documentation

You can read more Meadow.Foundation and how to get started in our [developer site](http://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

## Using

To use Meadow.Foundation, simply add a Nuget reference to the core library (for core helpers and drivers), or to the specific peripheral driver you'd like to use, and the core will come with it.

```bash
nuget install Meadow.Foundation
```

## Contributing

Meadow.Foundation, is open source and community powered. We love pull requests, so if you've got a driver to add, send it on over! For each peripheral driver, please include:

 * **Documentation** - Including a Fritzing breadboard schematic on wiring it up, sourcing info, and API docs. Please see other drivers for examples. Documentation is hosted on the Wilderness Labs [DocFx](https://wildernesslabs.github.io/docfx/) site.
 * **Datasheet** - For the peripheral, if applicable.
 * **Sample** - Application illustrating usage of the peripheral.

# License

Copyright 2019-2022, Wilderness Labs Inc.
    
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
    
      http://www.apache.org/licenses/LICENSE-2.0
    
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
 
## Author Credits

Authors: Bryan Costanich, Mark Stevens, Adrian Stevens, Jorge Ramirez, Brian Kim, Frank Krueger, Craig Dunn

## Developer Mode

To make local development easier, there's a script to convert all the PackageReferences to ProjectReferences:

```bash
cd Source
./denugetize.sh
```

Make sure the script is executable by running `chmod u+x denugetize.sh`

To change the references back to PackageReferences:

```bash
cd Source
./nugetize.sh
```

Make sure the script is executable by running `chmod u+x nugetize.sh`

# Publishing Nuget Packages

To trigger a new build:  
- Go to project properties in VS 2019 / 2022 
- in the `Package` tab, increment either the MAJOR or MINOR `Package version`.  

The CI job will pick up the changes, pack, and push the Nuget package.
