using System.CommandLine.Invocation;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Domain.Context;
using Domain.Dtos;
using Domain.Models;
using Newtonsoft.Json;

namespace CLI.Commands;

public class AuthenticateUserCommandHandler :ICommandHandler
{
    private readonly MethflixContext _context;

    public AuthenticateUserCommandHandler(MethflixContext context)
    {
        _context = context;
    }
    
    public int Invoke(InvocationContext context)
    {
        Console.WriteLine("Opening socket at ws://127.0.0.1:9987 waiting for Authentication request.");
        Console.WriteLine("For ease of use you can just authenticate the device with the Methflix mobile app by scanning the QR code.");
        Console.WriteLine("Before scanning please ensure that both devices are connected under the same local network.");
        
        var codeData = CodeGenerator.GenerateQrCodeAsciiArt("ws://your-csharp-websocket-endpoint");

        Console.WriteLine(codeData);
        
        var ipAddress = IPAddress.Parse("127.0.0.1");
        var port = 9987;

        using var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect(new IPEndPoint(ipAddress, port));

        while (true)
        {
            var buffer = new byte[1024];
            var bytesRead = clientSocket.Receive(buffer);
            var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Server response: " + response);
            var account = JsonConvert.DeserializeObject<RegisterAccountRequest>(response);
            var acc = _context.Accounts.Add(
                new Account
                {
                    Name = account.Account,
                    RoleId = 1,
                }
            ).Entity;
            _context.SaveChanges();

            _context.AccessKeys.Add(
                new AccessKey
                {
                    Key = account.PublicKey,
                    AccountId = acc.Id
                }
            );
            _context.SaveChanges();
            
            clientSocket.Close();
            break;
        }

        Console.WriteLine("Socket closed.");
        return -1;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        return -1;
    }
}
