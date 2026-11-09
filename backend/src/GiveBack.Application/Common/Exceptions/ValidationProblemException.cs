namespace GiveBack.Application.Common.Exceptions;

public sealed class ValidationProblemException : Exception
{
    public ValidationProblemException(IDictionary<string, string[]> errors) : base("Validation failed.")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}
