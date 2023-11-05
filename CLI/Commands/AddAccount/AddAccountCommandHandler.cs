using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Domain.Context;
using Domain.Models;

namespace CLI.Commands.AddAccount;

public class AddAccountCommandHandler : ICommandHandler
{
    private readonly MethflixContext _context;

    public AddAccountCommandHandler(MethflixContext context)
    {
        _context = context;
    }
    
    public int Invoke(InvocationContext context)
    {
        var result = context.BindingContext.ParseResult;
        var key = new Option<string>("key");
        var name = new Option<string>("name");
        if (!result.HasOption(key) ||
            !result.HasOption(name)) return 1; // Check if an option is present

        var keyValue = result.GetValueForOption(key);
        var nameValue = result.GetValueForOption(key);

        var exits = _context.Accounts.FirstOrDefault(x => x.Name == nameValue);
        if (exits != null)
        {
            _context.AccessKeys.Add(
                new AccessKey
                {
                    AccountId = exits.Id,
                    Key = keyValue
                }
            );
            _context.SaveChanges();
            return 1;
        }

        var acc = _context.Accounts.Add(
            new Account
            {
                RoleId = 1,
                Name = nameValue
            }
        ).Entity;
        _context.SaveChanges();
        _context.AccessKeys.Add(
            new AccessKey
            {
                Key = keyValue,
                AccountId = acc.Id
            }
        );
        _context.SaveChanges();
        return 1;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        return -1;
    }
}
