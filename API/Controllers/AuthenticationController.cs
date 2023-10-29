using System.Security.Cryptography;
using Domain.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("/API/VI/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly List<StoredMessage> storedMessages = new();

    [HttpGet("Init")]
    public IActionResult InitiateSignIn()
    {
        try
        {
            var uniqueMessage = Guid.NewGuid().ToString();
            var storedMessage = new StoredMessage
            {
                Message = uniqueMessage
            };
            storedMessages.Add(storedMessage);

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
            // Load the public key
            using var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(request.PublicKey);
            if (storedMessages.Count == 0)
                return BadRequest(new { VerificationStatus = "Message not found for verification." });

            var isSignatureValid = false;
            foreach (var x in storedMessages)
            {
                var messageData = Convert.FromBase64String(x.Message);
                var signature = Convert.FromBase64String(request.Signature);
                isSignatureValid = rsa.VerifyData(messageData, new SHA256CryptoServiceProvider(), signature);
                if (isSignatureValid) break; // Break out of the loop when a valid signature is found
            }

            if (isSignatureValid)
                return Ok(new { VerificationStatus = "Signature is valid." });
            return BadRequest(new { VerificationStatus = "Signature is not valid." });
        }
        catch (Exception ex)
        {
            return StatusCode(
                500, new { Message = "An error occurred while verifying the signature.", Error = ex.Message }
            );
        }
    }
}
