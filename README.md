# About
rw is a command line tool dispatcher for my own personal tools, the tools are there to solve/streamline certain aspects I find laborious about using a PC. The whole tool is built in C# so the msi and deb installer packages that it creates are quite large ~100MB. It comes with a full instance of .NET10 runtime so nothing else needs to install. Running the uninstall command will remove all traces of it from the system and should leave no system fies.

## Using
Run each command using `rw` and then the command you wish to run. `rw -h` will bring up the full list of commands `rw uninstall -c` will fully uninstall the utility from your system.

# Notes
This is a mirrored repo, this is only the public interface so no CI runs in github itself. The real work is on a locally hosted gitlab instance which does feature a CI build and test. This will eventually publish a downloadable package.

## Running with no build
If you want to run the tool from visual studio just using the dotnet runtime, this is the command to be ran from the repo root
```
>> dotnet run --project src/Rw -- hello-world -n Rice
```

## Building
To build the project you will need to have the .NET10 SDK installed. Then building the project becomes is simple, it compiles into a self contained executable. 
Ensure that it targets the correct version windows/linux with the correct version tag. The version tag is how the installer handles overwriting existing versions of the tool. If you copy the below commands it will have the correct version numbers in it, (as long as you pull master).

```
>> dotnet publish src/Rw -c Release -r win-x64 --self-contained -p:RW_VERSION=0.2.0 -o dist/win-x64

OR

>> dotnet publish src/Rw -c Release -r linux-x64 --self-contained -p:RW_VERSION=0.2.0 -o dist/linux-x64
```

This produces an exe that can work for either OS, if you want an installer to handle the rest, then these are the platform specific commands.

### Windows
This requires Wix to create the MSI, this can only be ran from a windows machine, as Wix is not available for linux to the best of my knowledge.

`wix build installer/windows/Product.wxs -d Version=0.2.0 -d PayloadDir=dist/win-x64 -o dist/rw-0.2.0.msi`

### Linux
I have not tested this most recent build on linux, but these are the steps that got v0.1.0 to work on a linux mint machine.

`bash installer/linux/build-deb.sh 0.2.0 dist/linux-x64`