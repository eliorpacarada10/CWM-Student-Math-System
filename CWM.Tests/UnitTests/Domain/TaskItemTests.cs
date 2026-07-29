using CWM.Domain.Entities;
using CWM.Domain.Exceptions;
using Xunit;

namespace CWM.Tests.UnitTests.Domain;

public class TaskItemTests
{
    [Fact]
    public void Constructor_throws_when_external_id_is_empty()
    {
        Assert.Throws<MathTestDomainException>(() => new TaskItem("", "2+2", 4));
    }

    [Fact]
    public void Constructor_throws_when_expression_is_empty()
    {
        Assert.Throws<MathTestDomainException>(() => new TaskItem("1", "   ", 4));
    }

    [Fact]
    public void Grade_marks_correct_when_computed_matches_claimed_exactly()
    {
        var task = new TaskItem("1", "2+2", 4m);

        task.Grade(4m);

        Assert.True(task.IsCorrect);
        Assert.Equal(4m, task.ComputedResult);
        Assert.Null(task.GradingError);
        Assert.NotNull(task.GradedAtUtc);
    }

    [Fact]
    public void Grade_marks_correct_within_tolerance_for_non_terminating_division()
    {
        // 1/3 does not terminate; a student rounding to 4 decimal places should still be
        // graded correct rather than failing on exact decimal equality.
        var task = new TaskItem("1", "1/3", 0.3333m);

        task.Grade(1m / 3m);

        Assert.True(task.IsCorrect);
    }

    [Fact]
    public void Grade_marks_incorrect_when_outside_tolerance()
    {
        var task = new TaskItem("1", "2+2", 5m);

        task.Grade(4m);

        Assert.False(task.IsCorrect);
    }

    [Fact]
    public void MarkGradingFailed_records_reason_without_a_computed_result()
    {
        var task = new TaskItem("1", "2+*", 4m);

        task.MarkGradingFailed("Unexpected token '*'.");

        Assert.False(task.IsCorrect);
        Assert.Null(task.ComputedResult);
        Assert.Equal("Unexpected token '*'.", task.GradingError);
        Assert.NotNull(task.GradedAtUtc);
    }

    [Fact]
    public void Grade_after_a_previous_failure_clears_the_grading_error()
    {
        var task = new TaskItem("1", "2+2", 4m);
        task.MarkGradingFailed("transient failure");

        task.Grade(4m);

        Assert.True(task.IsCorrect);
        Assert.Null(task.GradingError);
    }
}
