# Immotionar ImmotionRoom v0.4.2.1 (BETA)

The [`ImmotionRoom`](https://github.com/gianni-rg/immotionroom) repository is where development is done and there are many ways you can participate in the project, for example:

* [Submit bugs and feature requests](https://github.com/gianni-rg/ImmotionRoom/issues) and help me verify as they are checked in
* Review [source code changes](https://github.com/gianni-rg/ImmotionRoom/pulls)
* Review the [documentation](https://github.com/gianni-rg/ImmotionRoom/docs) and make pull requests for anything from typos to new content

Currently the development is stopped and the source code reflects the status of the last stable release of ImmotionRoom as of April 2017. I am maintaining the project in my limited spare time, so requests of any kind will be handled as best-effort.

*Following documentation only tells you how to build ImmotionRoom*

## Build Instructions

* We use PowerShell to build all the projects. Make sure you set the variables in the **build.ps1** script

* **Visual Studio 2017 is REQUIRED to build the project**. 

You can set a custom path to the msbuild included with Visual Studio as the **$msbuild_vs** variable in the **build.ps1** script.

* Make sure you install the latest version of nuget from <http://blog.nuget.org/20150902/nuget-3.2RC.html> (3.2+) and set its path in the **$nuget_exe** variable in the **build.ps1** script. In the Tools folder there is the Nuget command line tool we use to build (3.2.0.10516) and the build script is already configured to use this version. 
* Make sure you install Unity 3D 5.x (currently we're using **v5.5.0f3**) and set its path in the **$unity_exe** variable in the **build_unity.ps1**

* Make sure you install the latest version of Sandcastle in order to generate the Intellisense XML documentation (currently for Unity3dLittleBoots projects only). You can download it from <https://github.com/EWSoftware/SHFB/releases>. A copy of the current binary to install can be found in the \Tools folder.
 
* Finally, run "Windows PowerShell" 
* The build script (**build.ps1**) is a PowerShell script and is set to stop on any failure. The last failure you see is the one that broke the build.
* Run the following in your PowerShell to allow build script to run. Answer 'Y' to the question on allowing unsigned scripts to run.
    'Set-ExecutionPolicy -Scope CurrentUser unrestricted'
* Run ".\build.ps1" with one or more of the following options:

  * **-buildConfiguration**: choose which configuration to build (Debug or Release). Default: Release
  * **-buildPlatform**: choose which platform to build (NET45). Default: NET45
  * **-noclean**: disable bin/obj and Binaries_* folders clean before build
  * **-trackingService**: builds and exports TrackingService application only
  * **-dataSourceService**: builds and exports DataSourceService application only
  * **-managerApp**: builds and exports ManagerApp application only
  * **-unity3dtools**: builds and exports Unity3D Tools only
  * **-serviceConfigTool**: builds and exports Service Config Tool only
  * **-csclient**: builds and exports C# client only
  * **-servers**: builds and exports ImmotionRoom Servers (IRidge) only
  * **-all**: builds and exports all ImmotionRoom applications
  * **-testbuild**: builds all ImmotionRoom applications in Debug+Release mode to verify everything is in place
  * **-xmlDoc**: enables XML documentation generation for the Unity 3d Tools (requires SandCastle)
  * **-help**: prints the instructions

* Successful completion of the **build.ps1** script will build all the applications in the specified build configuration and place the binaries in the "Binaries_[Debug|Release]" folder in the root of the project.

*If you don't know which options to use, just launch the script with **'-all'** option.*
It will build and export all ImmotionRoom applications in Release configuration for NET45.

Usually, the standard build command is:

        .\build.ps1 -all -xmlDoc

*If you just want to test if all the projects build, launch the script with **'-testbuild'** option.*
It will cleanup the build folders, build all ImmotionRoom applications in Debug/Release configuration and then cleanup again the build folders.

## Contributing

If you are interested in fixing issues and contributing directly to the code base, please do it. I am maintaining the project in my limited spare time, so please be patient, as requests of any kind will be handled as best-effort.

## License

Immotionar ImmotionRoom is made available under a Dual License business model and different licenses are available for two distinct purposes, open source and commercial development.

If you wish to use the open source license of ImmotionRoom, you must contribute all your source code to the open source community and you must give them the right to share it with everyone too.

If you derive a commercial advantage by having a closed source solution, you must purchase an appropriate commercial licenses from Immotionar / Beps Engineering. By purchasing commercial license, you are no longer obligated to publish your source code.

Please send an email to legal@immotionar.com and segreteria@bepseng.it for
further information.

For more details, see [ImmotionRoom License](LICENSE.txt).