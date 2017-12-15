# Jubjubnest.Style.DotNet [![Build status](https://ci.appveyor.com/api/projects/status/6poirbr83iclbx44?svg=true)](https://ci.appveyor.com/project/Rantanen/jubjubnest-style-dotnet) [![codecov](https://codecov.io/gh/Rantanen/Jubjubnest.Style.DotNet/branch/master/graph/badge.svg)](https://codecov.io/gh/Rantanen/Jubjubnest.Style.DotNet)

.Net style checker for various style rules not covered by the built-in
analyzers.

## Downloads

The repository is automatically built on appveyor.

Following build artifacts are available:

### `master` branch

- [Visual Studio Extension package](https://ci.appveyor.com/api/projects/Rantanen/jubjubnest-style-dotnet/artifacts/dist/Jubjubnest.Style.DotNet.vsix?branch=master)
- [NuGet package](https://www.nuget.org/packages/Jubjubnest.Style.DotNet/)

## Installation

The Visual Studio extension can be installed locally on the development machine while the nuget package can be pulled in using project references.

Alternatively you can download the nuget package manually and extract the dll from it. The analyzer can then be enabled per project by adding the following item group in the project file:

```xml
  <ItemGroup>
    <Analyzer Include="path\to\Jubjubnest.Style.DotNet.dll" />
  </ItemGroup>
```

The extension can be installed both locally and per project, but care should be taken to use the same version of the extension in both cases. Using two different versions may result in the style warnings being displayed multiple times.

### Configuration

The style checks are configured in the `<project>.ruleset` file. The easiest way to configure the rules is to open the file in Visual Studio and use the visual editor to choose the rules. If the file doesn't exist, you can create it through the project properties or using a text editor with the following content:

```xml
<?xml version="1.0" encoding="utf-8">
<RuleSet Name="Style Rules" ToolsVersion="14.0" />
```
