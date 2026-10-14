using StudentManagement.Api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        // Force int/int32 (and long/int64) to be pure integer instead of the integer|string union
        if (schema.Format is "int32" or "int64" && schema.Type.HasValue)
        {
            schema.Type = Microsoft.OpenApi.JsonSchemaType.Integer;
        }
        return Task.CompletedTask;
    });
});

// Register the SQLite database
builder.Services.AddDbContext<StudentDbContext>(options =>
    options.UseSqlite("Data Source=students.db"));

var app = builder.Build();
app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/openapi/v1.json",
        "Student Management API v1");
});


static Dictionary<string, string[]> ValidateStudent(Student student)
{
    var context = new System.ComponentModel.DataAnnotations.ValidationContext(student);
    var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

    bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
        student,
        context,
        results,
        validateAllProperties: true);

    return results
        .SelectMany(result => result.MemberNames.Select(member => new
        {
            Member = member,
            Error = result.ErrorMessage ?? "Invalid value."
        }))
        .GroupBy(item => item.Member)
        .ToDictionary(
            group => group.Key,
            group => group.Select(item => item.Error).ToArray());
}


// Home route
app.MapGet("/", () => "Student Management API is running!");

// GET: Retrieve all students
app.MapGet("/api/students", async (StudentDbContext db) =>
{
    var students = await db.Students.ToListAsync();
    return Results.Ok(students);
});

// GET: Retrieve one student by ID
app.MapGet("/api/students/{id}", async (int id, StudentDbContext db) =>
{
    var student = await db.Students.FindAsync(id);

    if (student == null)
        return Results.NotFound("Student not found.");

    return Results.Ok(student);
});

// POST: Add a new student

app.MapPost("/api/students", async (Student student, StudentDbContext db) =>
{
    var errors = ValidateStudent(student);

    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    db.Students.Add(student);
    await db.SaveChangesAsync();

    return Results.Created($"/api/students/{student.Id}", student);
});


// PUT: Update an existing student

app.MapPut("/api/students/{id}", async (int id, Student updatedStudent, StudentDbContext db) =>
{
    var errors = ValidateStudent(updatedStudent);

    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    var student = await db.Students.FindAsync(id);

    if (student == null)
        return Results.NotFound("Student not found.");

    student.Name = updatedStudent.Name;
    student.Course = updatedStudent.Course;

    await db.SaveChangesAsync();

    return Results.Ok(student);
});


// DELETE: Remove a student
app.MapDelete("/api/students/{id}", async (int id, StudentDbContext db) =>
{
    var student = await db.Students.FindAsync(id);

    if (student == null)
        return Results.NotFound("Student not found.");

    db.Students.Remove(student);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
public partial class Program { }