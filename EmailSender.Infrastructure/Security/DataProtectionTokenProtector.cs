using EmailSender.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace EmailSender.Infrastructure.Security;

public sealed class DataProtectionTokenProtector(IDataProtectionProvider provider) : ITokenProtector
{
    private readonly IDataProtector _protector = provider.CreateProtector("EmailSender.OAuth.RefreshTokens.v1");

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string protectedText) => _protector.Unprotect(protectedText);
}
