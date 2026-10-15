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




static Dictionary<string, string[]> ValidateModel(object model)
{
    var context = new System.ComponentModel.DataAnnotations.ValidationContext(model);
    var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

    bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
        model,
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
    var students = await db.Students
        .Select(student => new StudentDto
        {
            Id = student.Id,
            Name = student.Name,
            Course = student.Course
        })
        .ToListAsync();

    return Results.Ok(students);
});


// GET: Retrieve one student by ID

app.MapGet("/api/students/{id}", async (int id, StudentDbContext db) =>
{
    var student = await db.Students.FindAsync(id);

    if (student == null)
        return Results.NotFound("Student not found.");

    var studentDto = new StudentDto
    {
        Id = student.Id,
        Name = student.Name,
        Course = student.Course
    };

    return Results.Ok(studentDto);
});


// POST: Add a new student


app.MapPost("/api/students", async (CreateStudentDto studentDto, StudentDbContext db) =>
{
    var errors = ValidateModel(studentDto);

    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    var student = new Student
    {
        Name = studentDto.Name,
        Course = studentDto.Course
    };

    db.Students.Add(student);
    await db.SaveChangesAsync();

    var studentDtoResponse = new StudentDto
{
    Id = student.Id,
    Name = student.Name,
    Course = student.Course
};

    return Results.Created(
    $"/api/students/{student.Id}",
    studentDtoResponse
);
});



// PUT: Update an existing student


app.MapPut("/api/students/{id}", async (
    int id,
    UpdateStudentDto updatedStudentDto,
    StudentDbContext db) =>
{
    var errors = ValidateModel(updatedStudentDto);

    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    var student = await db.Students.FindAsync(id);

    if (student == null)
        return Results.NotFound("Student not found.");

    student.Name = updatedStudentDto.Name;
    student.Course = updatedStudentDto.Course;

    await db.SaveChangesAsync();

    var studentDtoResponse = new StudentDto
    {
    Id = student.Id,
    Name = student.Name,
    Course = student.Course
    };

    return Results.Ok(studentDtoResponse);
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