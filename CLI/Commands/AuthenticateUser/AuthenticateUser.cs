using System.CommandLine;
using System.CommandLine.Invocation;

namespace CLI.Commands;

public class AuthenticateUser : BaseCommand 
{
    public AuthenticateUser(string name, string? description = null) : base(name, description)
    {
    }
}
