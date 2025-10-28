The FAST.FBasic Library Toolkit is your gateway to extending the FAST.FBasic programming language with your own libraries composed of new statements and functions. If you want to augment FAST.FBasic's capabilities and tailor the language environment to your specific needs, this Toolkit provides a streamlined, modular framework to author, package, and distribute your customized libraries.

With the FAST.FBasic Library Toolkit, you can implement and integrate your own language extensions—be it new text processing commands, data manipulation routines, decision logic components, or any specialized functionality—directly into the FAST.FBasic ecosystem. This allows you to elevate FAST.FBasic well beyond its core features, adapting it seamlessly to your projects and workflows.

| NuGet Package | Version | Download | Activity | 
| --- | --- | --- | --- |
| FAST.FBasic.LibraryToolkit | [![NuGet](https://img.shields.io/nuget/v/FAST.FBasicLibraryToolkit.svg)](https://www.nuget.org/packages/FAST.FBasicLibraryToolkit) | [![Nuget](https://img.shields.io/nuget/dt/FAST.FBasicLibraryToolkit.svg)](https://www.nuget.org/packages/FAST.FBasicLibraryToolkit) | [![Last Commit](https://img.shields.io/github/last-commit/aafent/FAST.FBasic/main?label=Laster%20Project%20Commit&path=FAST.FBasic.LibraryToolkit)](https://github.com/aafent/FAST.FBasic/commits/main) |

Here are some typical examples of how to download and install the NuGet package **FAST.FBasicLibraryToolkit** in different development environments:

***

### Installing via .NET CLI

Use the `dotnet add package` command in your project directory to add the package:

```bash
dotnet add package FAST.FBasicLibraryToolkit --version 1.0.0
```

***

### Using Package Manager Console in Visual Studio

Run the following command in the Package Manager Console:

```powershell
Install-Package FAST.FBasicLibraryToolkit -Version 1.0.0
```

***

### Reference in Project File (for projects supporting PackageReference)

Add the following XML node to your `.csproj` file:

```xml
<PackageReference Include="FAST.FBasicLibraryToolkit" Version="1.0.0" />
```

***

### Using Paket Dependency Manager

Add the package to your dependencies list with:

```bash
paket add FAST.FBasicLibraryToolkit --version 1.0.0
```

***

### F# Interactive or Polyglot Notebooks

Reference the package in your interactive session or notebook script:

```fsharp
#r "nuget: FAST.FBasicLibraryToolkit, 1.0.0"
```

***

### .NET 10 Preview 4 and Newer (File-based C# Apps)

Add the package reference at the top of your `.cs` file:

```csharp
#r "nuget: FAST.FBasicLibraryToolkit, 1.0.0"
```

***

### Using with Cake Build Scripts

Add as an addin or tool:

```csharp
#addin nuget:?package=FAST.FBasicLibraryToolkit&version=1.0.0
#tool nuget:?package=FAST.FBasicLibraryToolkit&version=1.0.0
```

***

These methods provide flexible ways to incorporate the **FAST.FBasicLibraryToolkit** into your FAST.FBasic environment or .NET projects for extending the language with customized libraries.

The package supports a wide range of .NET target frameworks including net8.0, net9.0, and computed targets like Android, iOS, macOS, Windows, and more. It has no external dependencies, making integration straightforward.

***

Feel free to use whichever method best suits your development setup to start leveraging the FAST.FBasic Library Toolkit.

[See the package at nuget.org](https://www.nuget.org/packages/FAST.FBasicLibraryToolkit/)

[Read more about the Libray ToolKit](https://github.com/aafent/FAST.FBasic/wiki/Extending-the-language)
