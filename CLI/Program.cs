﻿using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using CLI.Commands;
using CLI.Commands.AddAccount;
using Domain.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QRCoder;

var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()) // Set the path to the JSON file
    .AddJsonFile("settings.json", true, true);

IConfiguration configuration = builder.Build();
var options = new DbContextOptionsBuilder<MethflixContext>().UseNpgsql(configuration.GetConnectionString("PostgreSQL")).
    Options;

Console.WriteLine("Important: Please do not expose this tool online. It is intended for initial setup purposes only!");

args = new[]
{
    "users"
};

foreach (var s in args) Console.WriteLine(s);
var rootCommand = new RootCommand
{
    new AuthenticateUser(
        "authenticate",
        "Allows a user to login the system, takes in a the following parameters, --user [name], -key [public address], --device [phone,pc,tv]"
    )
    {
        Handler = new AuthenticateUserCommandHandler(new MethflixContext())
    },
    new RemoveUser("remove", "Removes a user from accessing the api, takes in a public address")
    {
        Handler = new RemoveUserCommandHandler()
    },
    new Users(
        "users",
        "Gets a list of authorized users with the following properties, name, public access address, device name"
    )
    {
        Handler = new UsersCommandHandler()
    },
    new AddAcoountCommand("add-account","Adds a new account, if the name exist it will only append the new access key to the authorized account keys.")
    {
        Handler = new AddAccountCommandHandler(new MethflixContext())
    }
};

var parser = new CommandLineBuilder(rootCommand).UseDefaults().Build();

var exitCode = await parser.InvokeAsync(args);

Environment.Exit(exitCode);
