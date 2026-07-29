using CWM.Domain.Entities;
using CWM.Domain.Exceptions;
using Xunit;

namespace CWM.Tests.UnitTests.Domain;

public class StudentAndTeacherTests
{
    [Fact]
    public void Student_constructor_throws_when_external_id_is_empty()
    {
        Assert.Throws<MathTestDomainException>(() => new Student(""));
    }

    [Fact]
    public void Student_AddExam_appends_to_Exams()
    {
        var student = new Student("12345");
        var exam = new Exam("1");

        student.AddExam(exam);

        Assert.Single(student.Exams);
        Assert.Same(exam, student.Exams.First());
    }

    [Fact]
    public void Teacher_constructor_throws_when_external_id_is_empty()
    {
        Assert.Throws<MathTestDomainException>(() => new Teacher(""));
    }

    [Fact]
    public void Teacher_AddStudent_appends_to_Students()
    {
        var teacher = new Teacher("11111");
        var student = new Student("12345");

        teacher.AddStudent(student);

        Assert.Single(teacher.Students);
        Assert.Same(student, teacher.Students.First());
    }
}
