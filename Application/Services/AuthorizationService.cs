using System.Text;
using Domain.Dtos;
using Infrastructure.Interfaces;
using NSec.Cryptography;

namespace Application.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly List<StoredMessage> _storedMessages = new();
    private readonly Timer _timer;
    public AuthorizationService()
    {
        _timer = new Timer(OnTokensExpire, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();

      
    }

    private void OnTokensExpire(object? state)
    {
        _storedMessages.RemoveAll(x => x.ExpiresAt < DateTime.UtcNow);
    }

    public string GetMessage(string host)
    {
        var uniqueMessage = Guid.NewGuid().ToString();
        var storedMessage = new StoredMessage
        {
            Message = uniqueMessage,
            Ip = host,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1)
        };
        _storedMessages.Add(storedMessage);
        return uniqueMessage;
    }

    public bool VerifyMessage(VerifyRequest request, string host)
    {
        var bytes = Convert.FromBase64String(request.PublicKey);
        var signatureBytes = Convert.FromBase64String(request.Signature);
        var algorithm = new Ed25519();
        var publicKey = PublicKey.Import(algorithm, bytes, KeyBlobFormat.RawPublicKey);
        var last = _storedMessages.LastOrDefault(x => x.Ip == host);
        if (last == null) return false;
        
        var messageBytes = Encoding.UTF8.GetBytes(last.Message);
        var isSignatureValid = algorithm.Verify(publicKey, messageBytes, signatureBytes);
        return isSignatureValid;
    }

    public string ApproveAccount(string account)
    {
        throw new NotImplementedException();
    }
}
