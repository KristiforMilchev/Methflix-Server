using System.CommandLine;
using System.CommandLine.Invocation;

namespace CLI.Commands;

public class Users : BaseCommand
{
    public Users(string name, string? description = null) : base(name, description)
    {
    }
    
    

}
