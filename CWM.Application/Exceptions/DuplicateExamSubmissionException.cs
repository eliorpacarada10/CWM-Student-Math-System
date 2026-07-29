namespace CWM.Application.Exceptions;

/// <summary>
/// Thrown when saving would violate the (StudentId, ExamExternalId) uniqueness rule --
/// i.e. an exam already exists for that student under this id. Resubmitting an
/// already-graded exam is treated as a conflict rather than silently overwritten or merged;
/// the assignment doesn't call for update/re-grade semantics, so this keeps behavior simple
/// and explicit instead of guessing what the teacher meant.
/// </summary>
public sealed class DuplicateExamSubmissionException : Exception
{
    public DuplicateExamSubmissionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
