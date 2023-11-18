using Domain.Dtos;

namespace Infrastructure.Interfaces;

public interface IAuthorizationService
{
    public string GetMessage(string host);
    public bool VerifyMessage(VerifyRequest request, string host);
    public string ApproveAccount(string account);
}
