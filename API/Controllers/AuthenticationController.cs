using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Dtos;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers;

[Route("/API/V1/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    public AuthenticationController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    [HttpGet("Init")]
    public IActionResult InitiateSignIn()
    {
        try
        {
            var host = Request.Host.Host;
            var uniqueMessage = _authorizationService.GetMessage(host);
            return Ok(uniqueMessage);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while initiating sign-in.", Error = ex.Message });
        }
    }

    [HttpPost("Verify")]
    public IActionResult VerifySignature([FromBody] VerifyRequest request)
    {
        try
        {
            var host = Request.Host.Host;
            var isSignatureValid = _authorizationService.VerifyMessage(request, host);
            if (!isSignatureValid)
                return BadRequest(new { VerificationStatus = "Signature is not valid." });
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TV") 
            };

            var bytes = Convert.FromBase64String(request.Signature);
            var key = new SymmetricSecurityKey(bytes);

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Methflix",
                audience: "IOT Device",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Adjust the expiration time as needed
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            return Ok(new { Token = tokenString });
        }
        catch (Exception ex)
        {
            return StatusCode(
                500, new
                {
                    Message = "An error occurred while verifying the signature or token has expired."
                }
            );
        }
    }
}
