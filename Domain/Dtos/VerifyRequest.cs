namespace Domain.Dtos;

public class VerifyRequest
{
    public string PublicKey { get; set; }
    public string Signature { get; set; }
}
