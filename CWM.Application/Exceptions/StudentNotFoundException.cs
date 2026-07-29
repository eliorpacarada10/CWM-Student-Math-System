namespace CWM.Application.Exceptions;

public sealed class StudentNotFoundException : Exception
{
    public StudentNotFoundException(string studentExternalId)
        : base($"Student '{studentExternalId}' was not found.")
    {
    }
}
