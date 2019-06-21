
# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

Nuget requires packages to declare all their dependencies that are called from within the packages. If a User calls a dependency’s dependencies, they will need to declare that package as a direct dependency and will need to find and put the correct version of the package. This can cause a problem if the wrong version of the package is referenced, or if the version of the package is updated. This would cause the user to have the incorrect version of the dependency. ImplicitPackageReference was created to solve finding and packing the version of packages that are used in a project but are just dependencies of their dependencies. 

All one must do to use ImplicitPackageReference is import the Nuget Package. Then add the packages you wish to discover the versions for in the following format.

<ImplicitPackageReference Include="Microsoft.AspNetCore.Http.Abstractions" />

ImplicitPacageReference will make sure that package is packed into your Nuget package you are creating. This will only work if the package you are looking for is a dependency of one of your dependency. If the package is not found, the build will fail.
