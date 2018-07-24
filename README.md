C# TAF decoder
=================
[![Build status](https://ci.appveyor.com/api/projects/status/6bn4b15le3pj5wdt/branch/master?svg=true)](https://ci.appveyor.com/project/SafranCassiopee/csharp-taf-decoder/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/SafranCassiopee/csharp-taf-decoder/badge.svg)](https://coveralls.io/github/SafranCassiopee/csharp-taf-decoder)
[![Latest Stable Version NuGet](https://img.shields.io/nuget/v/csharp-taf-decoder.svg)](https://www.nuget.org/packages/csharp-taf-decoder/)


A .NET library to decode TAF (Terminal Aerodrome Forecast) strings, fully unit tested (100% code coverage)

This is largely based on [SafranCassiopee/php-taf-decoder](https://github.com/SafranCassiopee/php-taf-decoder)

They use csharp-taf-decoder in production:

- [Safran AGS ](https://www.safran-electronics-defense.com/aerospace/commercial-aircraft/information-system/analysis-ground-station-ags) (private)
- Your service here ? Submit a pull request or open an issue !

Introduction
------------

This piece of software is a library package that provides a parser to decode raw TAF messages.

TAF is a format made for weather information forecast. It is predominantly used by in aviation, during flight preparation. Raw TAF format is highly standardized through the International Civil Aviation Organization (ICAO).

*    [TAF definition on wikipedia](https://en.wikipedia.org/wiki/Terminal_aerodrome_forecast)
*    [TAF format specification](http://www.wmo.int/pages/prog/www/WMOCodes/WMO306_vI1/VolumeI.1.html)

Requirements
------------

This library package only requires .NET >= 4.5

It is currently tested automatically for .NET >= 4.5 using [nUnit 3.9.0](http://nunit.org/).

Although this is provided as a library project, a command line version (StartTafDecoder) is also included that can be used as both an example and a starting point.
StartTafDecoder requires [CommandLineParser](https://github.com/commandlineparser/commandline).

Usage:

```shell
StartTafDecoder.exe --TAF "TAF LEMD 080500Z 0806/0912 23010KT 9999 SCT025 TX12/0816Z TN04/0807Z"
```

If you want to integrate the library easily in your project, you should consider using the official nuget package available from https://www.nuget.org/.

```
nuget install csharp-taf-decoder
```

It is not mandatory though.

Setup
-----

- With nuget.exe *(recommended)*

From the Package Manager Console in Visual Studio

```shell
nuget install csharp-taf-decoder
```

Add a reference to the library, then add the following using directives:

```csharp
using csharp_taf_decoder;
using csharp_taf_decoder.entity;
```

- By hand

Download the latest release from [github](https://github.com/SafranCassiopee/csharp-taf-decoder/releases)

Extract it wherever you want in your project. The library itself is in the csharp-taf-decoder/ directory, the other directories are not mandatory for the library to work.

Add the csharp-taf-decoder project to your solution, then add a reference to it in your own project. Finally, add the same using directives than above.

Usage
-----

Instantiate the decoder and launch it on a TAF string.
The returned object is a DecodedTAF object from which you can retrieve all the weather properties that have been decoded.

All values who have a unit are based on the `Value` object which provides the ActualValue and ActualUnit properties

Please check the [DecodedTAF class](https://github.com/SafranCassiopee/csharp-taf-decoder/blob/master/csharp-taf-decoder/Entity/DecodedTaf.cs) for the structure of the resulting object

```csharp

  var d = TAFDecoder.ParseWithMode("TAF LEMD 080500Z 0806/0912 23010KT 9999 SCT025 TX12/0816Z TN04/0807Z");

 (TODO)
 
```

About Value objects
-------------------

In the example above, it is assumed that all requested parameters are available. 
In the real world, some fields are not mandatory thus it is important to check that the Value object (containing both the value and its unit) is not null before using it.
What you do in case it's null is totally up to you.

Here is an example:

```csharp

(TODO)

```

Value objects also contain their unit, that you can access with the `ActualUnit` property. When you access the `ActualValue` property, you'll get the value in this unit. 

If you want to get the value directly in another unit you can call `GetConvertedValue(unit)`. Supported values are speed, distance and pressure.

Here are all available units for conversion:

```csharp
// speed units:
// Value.Unit.MeterPerSecond
// Value.Unit.KilometerPerHour
// Value.Unit.Knot

// distance units:
// Value.Unit.Meter
// Value.Unit.Feet
// Value.Unit.StatuteMile

// pressure units:
// Value.Unit.HectoPascal
// Value.Unit.MercuryInch

// use on-the-fly conversion
var distance_in_sm = visibility.GetConvertedValue(Value.Unit.StatuteMile);
var speed_kph = speed.GetConvertedValue(Value.Unit.KilometerPerHour);
```

About parsing errors
--------------------

When an unexpected format is encountered for a part of the TAF, the parsing error is logged into the DecodedTaf object itself.

All parsing errors for one TAF can be accessed through the `DecodingExceptions` property.

By default parsing will continue when a bad format is encountered. 
But the parser also provides a "strict" mode where parsing stops as soon as an error occurs.
The mode can be set globally for a TafDecoder object, or just once as you can see in this example:

```csharp

var decoder = new TAFDecoder();

(TODO)

```

About parsing errors, again
---------------------------

(TODO)

Contribute
----------

If you find a valid TAF that is badly parsed by this library, please open a github issue with all possible details:

- the full TAF causing problem
- the parsing exception returned by the library
- how you expected the decoder to behave
- anything to support your proposal (links to official websites appreciated)

If you want to improve or enrich the test suite, fork the repository and submit your changes with a pull request.

If you have any other idea to improve the library, please use github issues or directly pull requests depending on what you're more comfortable with.

In order to contribute to the codebase, you must fork the repository on github, than clone it locally with:

```shell
git clone https://github.com/<username>/csharp-taf-decoder
```

Install all the dependencies using nuget :

```shell
nuget restore csharp-taf-decoder\
```

You're ready to launch the test suite with:

```shell
nunit-console.exe /xml:results.xml csharp-taf-decoder-tests\bin\debug\csharp-taf-decoder-tests.dll
```

This library is fully unit tested, and uses [nUnit]((http://nunit.org/)) to launch the tests.

Travis CI is used for continuous integration, which triggers tests for .NET 4.5 for each push to the repo.
