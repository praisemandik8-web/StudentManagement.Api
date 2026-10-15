
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Api;

public class CreateStudentDto
{
    [Required]
    [StringLength(101, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(101, MinimumLength = 2)]
    public string Course { get; set; } = string.Empty;
}
