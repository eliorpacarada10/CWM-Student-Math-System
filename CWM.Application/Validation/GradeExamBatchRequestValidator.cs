using CWM.Application.Contracts;
using FluentValidation;

namespace CWM.Application.Validation;

/// <summary>
/// Input validation on the Contract itself -- "is this a sensible request" (non-empty ids,
/// at least one student/exam/task). This is deliberately separate from the parser's
/// structural/XSD validation (is this well-formed XML) and from Domain's invariants (is this
/// domain-legal). Runs identically no matter which IExamDocumentParser produced the request.
/// </summary>
public sealed class GradeExamBatchRequestValidator : AbstractValidator<GradeExamBatchRequest>
{
    public GradeExamBatchRequestValidator()
    {
        RuleFor(x => x.TeacherExternalId).NotEmpty();
        RuleFor(x => x.Students)
            .NotEmpty()
            .WithMessage("An exam batch must contain at least one student.");

        RuleForEach(x => x.Students).ChildRules(student =>
        {
            student.RuleFor(s => s.StudentExternalId).NotEmpty();
            student.RuleFor(s => s.Exams).NotEmpty().WithMessage("Each student must have at least one exam.");

            student.RuleForEach(s => s.Exams).ChildRules(exam =>
            {
                exam.RuleFor(e => e.ExamExternalId).NotEmpty();
                exam.RuleFor(e => e.Tasks).NotEmpty().WithMessage("Each exam must have at least one task.");

                exam.RuleForEach(e => e.Tasks).ChildRules(task =>
                {
                    task.RuleFor(t => t.TaskExternalId).NotEmpty();
                    task.RuleFor(t => t.Expression).NotEmpty();
                });
            });
        });
    }
}
