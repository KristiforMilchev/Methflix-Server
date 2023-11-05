using System.CommandLine;
using System.CommandLine.Invocation;

namespace CLI.Commands;

public class RemoveUser : BaseCommand
{
    public RemoveUser(string name, string? description = null) : base(name, description)
    {
    }
}
