using System.CommandLine;
using HelloWorld;
using Rw.Core.Interfaces;

var root = new RootCommand("Personal command toolkit");

foreach (var tool in new IRwTool[]
{
    new HelloWorldTool(),
})
{
    root.Add(tool.CreateCommand());
}

return await root.Parse(args).InvokeAsync();
