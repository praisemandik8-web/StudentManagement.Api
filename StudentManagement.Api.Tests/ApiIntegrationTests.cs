
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StudentManagement.Api;
using System.Net;
using System.Net.Http.Json;

namespace StudentManagement.Api.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // Create a separate in-memory database for this test.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the API's normal database configuration.
                services.RemoveAll<StudentDbContext>();
                services.RemoveAll<DbContextOptions<StudentDbContext>>();
                services.RemoveAll<
                    IDbContextOptionsConfiguration<StudentDbContext>>();

                // Register the in-memory database instead.
                services.AddDbContext<StudentDbContext>(options =>
                    options.UseSqlite(_connection));
            });
        });

        // Apply migrations to the isolated test database.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
        db.Database.Migrate();

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetStudents_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/api/students");

        Assert.Equal(
            System.Net.HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
public async Task CreateStudent_ReturnsCreatedStudent()
{
    var newStudent = new
    {
        Name = "Praise Mandik",
        Course = "Software Engineering"
    };

    var response = await _client.PostAsJsonAsync(
        "/api/students",
        newStudent
    );

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var createdStudent = await response.Content
    .ReadFromJsonAsync<Student>();

    Assert.NotNull(createdStudent);
    Assert.Equal("Praise Mandik", createdStudent.Name);
    Assert.Equal("Software Engineering", createdStudent.Course);
    var getResponse = await _client.GetAsync(
    $"/api/students/{createdStudent.Id}"
);

    Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

    var retrievedStudent = await getResponse.Content
    .ReadFromJsonAsync<Student>();

    Assert.NotNull(retrievedStudent);
    Assert.Equal(createdStudent.Id, retrievedStudent.Id);
    Assert.Equal("Praise Mandik", retrievedStudent.Name);
}


[Fact]
public async Task UpdateStudent_ReturnsUpdatedStudent()
{
    var newStudent = new
    {
        Name = "Praise Mandik",
        Course = "Software Engineering"
    };

    var createResponse = await _client.PostAsJsonAsync(
        "/api/students",
        newStudent
    );

    var createdStudent = await createResponse.Content
        .ReadFromJsonAsync<Student>();

    Assert.NotNull(createdStudent);

    var updatedStudent = new
    {
        Name = "Praise Pankyes",
        Course = "Computer Science"
    };

    var updateResponse = await _client.PutAsJsonAsync(
        $"/api/students/{createdStudent.Id}",
        updatedStudent
    );

    Assert.Equal(
        HttpStatusCode.OK,
        updateResponse.StatusCode
    );

    var result = await updateResponse.Content
        .ReadFromJsonAsync<Student>();

    Assert.NotNull(result);
    Assert.Equal(createdStudent.Id, result.Id);
    Assert.Equal("Praise Pankyes", result.Name);
    Assert.Equal("Computer Science", result.Course);
}


[Fact]
public async Task UpdateStudent_ReturnsNotFound_WhenStudentDoesNotExist()
{
    var updatedStudent = new
    {
        Name = "Praise Mandik",
        Course = "Software Engineering"
    };

    var response = await _client.PutAsJsonAsync(
        "/api/students/999999",
        updatedStudent
    );

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode
    );
}


[Fact]
public async Task DeleteStudent_RemovesStudent()
{
    var newStudent = new
    {
        Name = "Praise Mandik",
        Course = "Software Engineering"
    };

    var createResponse = await _client.PostAsJsonAsync(
        "/api/students",
        newStudent
    );

    var createdStudent = await createResponse.Content
        .ReadFromJsonAsync<Student>();

    Assert.NotNull(createdStudent);

    var deleteResponse = await _client.DeleteAsync(
        $"/api/students/{createdStudent.Id}"
    );

    Assert.Equal(
        HttpStatusCode.NoContent,
        deleteResponse.StatusCode
    );

    var getResponse = await _client.GetAsync(
        $"/api/students/{createdStudent.Id}"
    );

    Assert.Equal(
        HttpStatusCode.NotFound,
        getResponse.StatusCode
    );
}


[Fact]
public async Task DeleteStudent_ReturnsNotFound_WhenStudentDoesNotExist()
{
    var response = await _client.DeleteAsync(
        "/api/students/999999"
    );

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode
    );
}



[Fact]
public async Task CreateStudent_ReturnsValidationProblem_WhenNameIsTooShort()
{
    var invalidStudent = new
    {
        Name = "P",
        Course = "Software Engineering"
    };

    var response = await _client.PostAsJsonAsync(
        "/api/students",
        invalidStudent
    );

    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode
    );

    var studentsResponse = await _client.GetAsync("/api/students");

    var students = await studentsResponse.Content
    .ReadFromJsonAsync<List<Student>>();

    Assert.NotNull(students);
    Assert.DoesNotContain(students, student => student.Name == "P");
}


    
    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        _connection.Dispose();
    }
}
