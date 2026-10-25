using System.CommandLine;
using HelloWorld;
using GuidMaker;
using Rw.Core.Interfaces;
using Rw.Core.IO;
using Rw.Core.Processes;
using Uninstall;

var processRunner = new ProcessRunner();
var fileSystem = new PhysicalFileSystem();

var root = new RootCommand("Personal command toolkit");

foreach (var tool in new IRwTool[]
{
    new HelloWorldTool(),
    new UninstallTool(processRunner, fileSystem, MsiProductLocators.ForCurrentOs()),
    new GuidMakerTool(),
})
{
    root.Add(tool.CreateCommand());
}

return await root.Parse(args).InvokeAsync();
