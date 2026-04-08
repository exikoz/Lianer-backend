namespace Lianer.Core.API.Exceptions.User;

public class EmailTakenException : Exception
{
    public EmailTakenException(string message = "Email is already taken")
        : base(message) { }
}