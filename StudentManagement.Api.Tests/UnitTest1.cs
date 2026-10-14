
using System.ComponentModel.DataAnnotations;
using StudentManagement.Api;

namespace StudentManagement.Api.Tests;

public class UnitTest1
{
    [Fact]
    public void Student_WithValidNameAndCourse_ShouldPassValidation()
    {
        // Arrange
        var student = new Student
        {
            Name = "Ada",
            Course = "Software Engineering"
        };

        // Act
        var results = ValidateStudent(student);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Student_WithOneCharacterName_ShouldFailValidation()
    {
        // Arrange
        var student = new Student
        {
            Name = "A",
            Course = "Software Engineering"
        };

        // Act
        var results = ValidateStudent(student);

        // Assert
        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(Student.Name)));
    }

    [Fact]
    public void Student_WithNameLongerThan101Characters_ShouldFailValidation()
    {
        // Arrange
        var student = new Student
        {
            Name = new string('A', 102),
            Course = "Software Engineering"
        };

        // Act
        var results = ValidateStudent(student);

        // Assert
        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(Student.Name)));
    }

    private static List<ValidationResult> ValidateStudent(Student student)
    {
        var context = new ValidationContext(student);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            student,
            context,
            results,
            validateAllProperties: true);

        return results;
    }
}
