namespace EmailSender.Application.Interfaces;

public interface ITokenProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedText);
}
