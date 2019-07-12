

# ImplicitPackageReference
Allows execution of a custom MSBuild targets file, that will allow declaring transitive NuGet dependencies as direct NuGet dependencies.

# Problem 
The proper way to create a NuGet package requires the NuGet author to declare all the direct dependencies that their published assembly is built against; however, it is common for an assembly author to pull (via PackageReference) higher-level packages than what they directly use for their library to align versions across multiple sub-dependencies. The higher-level package shouldn't be declared as a NuGet dependency, but one or more of its sub-dependencies should be since they are actually used by the assembly code being published.

## Example of the Problem
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>NeatLibrary</AssemblyName>
	<GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  </PropertyGroup>

	<ItemGroup>
		<PackageReference Include="Microsoft.AspNetCore" Version="2.2.0" />
	</ItemGroup>
</Project>
```
This code will indirectly pull in the *Microsoft.Extensions.Configuration* NuGet package that is used by the code for NeatLibrary.dll.
 
In this example, the NeatLibrary code only uses the  *Microsoft.Extensions.Configuration.IConfiguration*  class -- nothing else. This class comes from the  *Microsoft.Extensions.Configuration.Abstractions* NuGet package.

The resulting *NeatLibrary* NuGet package would declare a dependency on *Microsoft.AspNetcore* >= 2.2.0. This forces the NeatLibrary  consumers to pull the entire *Microsoft.AspNetCore* dependency tree. While this works, it is not correct. It should only declare a dependency on *Microsoft.Extensions.Configuration.Abstractions* package.

## Solving without ImplicitPackageReference
The work-around for this is to declare the direct dependencies with PackageReferences:

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<ItemGroup>
		<PackageReference Include="Microsoft.AspNetCore" Version="2.2.0" PrivateAssets="All" /> <!-- Set PrivateAssets or completely remove this PackageReference -->
		<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="2.2.0" />
	</ItemGroup>
</Project>
```

This works, but introduces another problem. If the author wants to upgrade to *Microsoft.AspNetCore* 2.3.0 then NuGet package manager will require the author to manually upgrade all transitive dependencies, like *Microsoft.Extensions.Configuration.Abstractions*, first. With a large set of direct dependencies this becomes a chore.

As NuGet authors we want the ability to pull in a higher-level package like (*Microsoft.AspNetCore*) and still declare dependencies on transitive dependencies without having to know the exact version of those transitive dependencies or having to update those in a certain order to upgrade my dependencies.

## Solution
ImplicitPackageReference allows declaring direct NuGet dependencies on transitive dependencies. The NuGet author does not need to know the exact version of the sub-dependency when pulling it in transitively.

### Usage

1. To use ImplicitPackageReference, then pull in the _Microsoft.Build.ImplicitPackageReference_ NuGet package
2. Specify ImplicitPackageReference for each sub-dependency that needs to be declared as a direct dependency of the NuGet package that is being published
3. Set PrivateAssets="All" on all PackageReference tags that are not direct dependencies of the NuGet package that is being published

### Example Usage
```xml
  <Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
      <AssemblyName>NeatLibrary</AssemblyName>
	  <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    </PropertyGroup>

	<ItemGroup>
		<PackageReference Include="Microsoft.Build.ImplicitPackageReference" /> <!-- Add Support for ImplicitPackageReferences -->

		<PackageReference Include="Microsoft.AspNetCore" Version="2.2.0" PrivateAssets="All" /> <!-- Sets PrivateAsssets to exclude as a declared dependency in the publishing NuGet package -->
		<ImplicitPackageReference Include="Microsoft.Extensions.Configuration.Abstractions" /> <!-- Direct Dependency the Neat.cs uses -->
	</ItemGroup>
</Project>
```

The result will be a NuGet package called NeatLibrary.nupkg that will declare a dependency on just *Microsoft.Extensions.Configuration.Abstractions* >= 2.2.0. If *Microsoft.AspNetCore* package is every upgraded by the NuGet author then the Microsoft.Extensions.Configuration.Abstractions version will automatically be upgraded too.

## Supported ImplicitPackageReference Attributes

***Include*** --> This attribute specifies the name of the sub-dependency that needs to be declared as a direct dependency in the publishing NuGet.

***PrivateAssets*** --> This indicates which components of the NuGet package are needed by NuGet consumers. For more details see: https://docs.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files

# Contributing
This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.