namespace JobSeeker.WebApi.Application.Exceptions;

public class ConflictException : Exception
{
    public ConflictException()
    {
    }

    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string name, object key) : base($"Entity \"{name}\" ({key}) was exists.")
    {
    }
}