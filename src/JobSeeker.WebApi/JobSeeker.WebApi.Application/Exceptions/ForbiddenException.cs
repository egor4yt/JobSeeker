namespace JobSeeker.WebApi.Application.Exceptions;

public class ForbiddenException(string message) : Exception(message);