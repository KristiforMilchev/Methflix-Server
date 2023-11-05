using System.CommandLine;
using System.CommandLine.Invocation;

namespace CLI.Commands;

public class BaseCommand : Command 
{
    protected BaseCommand(string name, string? description = null) : base(name, description)
    {
    }

  
}
