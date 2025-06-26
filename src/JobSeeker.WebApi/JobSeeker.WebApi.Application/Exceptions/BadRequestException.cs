namespace JobSeeker.WebApi.Application.Exceptions;

public class BadRequestException(string message) : Exception(message);