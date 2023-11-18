namespace Domain.Dtos;

public class VerifyRequest
{
    public required string PublicKey { get; set; }
    public required string Signature { get; set; }
}
