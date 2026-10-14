
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StudentManagement.Api;

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

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        _connection.Dispose();
    }
}
