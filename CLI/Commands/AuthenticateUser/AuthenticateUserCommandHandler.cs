using System.CommandLine.Invocation;

namespace CLI.Commands;

public class AuthenticateUserCommandHandler :ICommandHandler
{
    public int Invoke(InvocationContext context)
    {
        return -1;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        return -1;
    }
}
