using CWM.Domain.Entities;
using CWM.Domain.Exceptions;
using Xunit;

namespace CWM.Tests.UnitTests.Domain;

public class ExamTests
{
    [Fact]
    public void Constructor_throws_when_external_id_is_empty()
    {
        Assert.Throws<MathTestDomainException>(() => new Exam(""));
    }

    [Fact]
    public void AddTask_throws_on_duplicate_task_external_id()
    {
        var exam = new Exam("1");
        exam.AddTask(new TaskItem("1", "2+2", 4));

        Assert.Throws<MathTestDomainException>(() => exam.AddTask(new TaskItem("1", "3+3", 6)));
    }

    [Fact]
    public void TotalTasks_and_CorrectTasks_reflect_grading_results()
    {
        var exam = new Exam("1");

        var correct = new TaskItem("1", "2+2", 4);
        correct.Grade(4);

        var incorrect = new TaskItem("2", "2+2", 5);
        incorrect.Grade(4);

        var ungradable = new TaskItem("3", "2+*", 1);
        ungradable.MarkGradingFailed("bad expression");

        exam.AddTask(correct);
        exam.AddTask(incorrect);
        exam.AddTask(ungradable);

        Assert.Equal(3, exam.TotalTasks);
        Assert.Equal(1, exam.CorrectTasks);
    }
}
