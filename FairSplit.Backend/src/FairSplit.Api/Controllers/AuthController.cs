using FairSplit.Api.Presentation.Models.Requests;
using FairSplit.Api.Presentation.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FairSplit.Api.Services.Interfaces;

namespace FairSplit.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("dev-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    public ActionResult<AuthTokenResponse> CreateDevToken([FromBody] DevTokenRequest request)
    {
        var token = authService.CreateDevToken(request.MemberId, request.DisplayName);

        return Ok(new AuthTokenResponse
        {
            AccessToken = token.AccessToken,
            TokenType = "Bearer",
            ExpiresAtUtc = token.ExpiresAtUtc,
            MemberId = token.MemberId,
            DisplayName = token.DisplayName
        });
    }
}