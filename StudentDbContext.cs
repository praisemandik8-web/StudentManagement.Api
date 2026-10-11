
using Microsoft.EntityFrameworkCore;

namespace StudentManagement.Api;

public class StudentDbContext : DbContext
{
    public StudentDbContext(DbContextOptions<StudentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
}
