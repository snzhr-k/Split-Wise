namespace FairSplit.Api.Services.Interfaces;

using FairSplit.Api.Services.Models;

public interface IAuthService
{
    AuthTokenResult CreateDevToken(Guid memberId, string displayName);
}