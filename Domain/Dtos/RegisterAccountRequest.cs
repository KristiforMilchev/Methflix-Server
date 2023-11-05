namespace Domain.Dtos;

public class RegisterAccountRequest
{
    public string PublicKey { get; set; }
    public string Account { get; set; }
    public int Device { get; set; }
}
