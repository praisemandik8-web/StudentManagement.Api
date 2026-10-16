# Student Management System

A full-stack Student Management System designed to manage student records through a RESTful API and a modern web interface. The project is being developed to explore backend development, frontend integration, authentication, cloud deployment, and DevOps practices.

## Project Overview

The application currently provides an ASP.NET Core Web API for creating, retrieving, updating, and deleting student records. It uses Entity Framework Core with SQLite for database persistence.

The long-term goal is to develop this into a complete full-stack application with a React frontend, JWT-based authentication, automated testing, CI/CD, and cloud deployment on Microsoft Azure.

## Technology Stack

### Backend
- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- RESTful API design

### Frontend — Planned
- React
- JavaScript
- React Router
- API integration

### Security — Planned
- JWT authentication
- Secure password hashing
- Authentication and authorization

### DevOps and Cloud — Planned
- Git and GitHub
- Automated testing and CI/CD with GitHub Actions
- Microsoft Azure hosting and deployment
- Production configuration and application monitoring

## Current Features

- Create student records
- Retrieve all students
- Retrieve an individual student by ID
- Update existing student records
- Delete student records
- Validate student names and courses
- Handle requests for nonexistent students
- Persist records using SQLite
- Manage database schema changes using Entity Framework Core migrations

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/students` | Retrieve all students |
| GET | `/api/students/{id}` | Retrieve a student by ID |
| POST | `/api/students` | Create a student |
| PUT | `/api/students/{id}` | Update a student |
| DELETE | `/api/students/{id}` | Delete a student |

## Getting Started

### Prerequisites

- .NET SDK compatible with the project
- Git
- A terminal or code editor such as Visual Studio Code

### Installation

1. Clone the repository:

   ```bash
   git clone <https://github.com/praisemandik8-web/StudentManagement.Api>
   ```

2. Navigate to the project directory:

   ```bash
   cd StudentManagement.Api
   ```

3. Restore the project dependencies:

   ```bash
   dotnet restore
   ```

4. Create or update the local SQLite database:

   ```bash
   dotnet ef database update
   ```

   If the EF Core command-line tool is not installed, install it with:

   ```bash
   dotnet tool install --global dotnet-ef
   ```

5. Run the API:

   ```bash
   dotnet run --urls http://localhost:5004
   ```

6. Test the API:

   ```bash
   curl http://localhost:5004/api/students
   ```

The API should be available at `http://localhost:5004`.

## Example Request

Create a student using the POST endpoint:

```bash
curl -X POST http://localhost:5004/api/students \
  -H "Content-Type: application/json" \
  -d '{"name":"Ada","course":"Software Engineering"}'
```

## Development Roadmap

- [x] Implement student CRUD endpoints
- [x] Integrate SQLite with Entity Framework Core
- [x] Add input validation and error handling
- [x] Configure database migrations
- [x] Add Swagger/OpenAPI documentation
- [x] Introduce DTOs for create, update, and response models
- [x] Implement JWT authentication and authorization
- [ ] Develop the React frontend
- [ ] Integrate the frontend with the backend
- [x] Add automated tests
- [ ] Configure CI/CD using GitHub Actions
- [ ] Deploy the application to Microsoft Azure
- [ ] Configure a production database and application monitoring

## Project Status

**In development.** The backend CRUD functionality, database persistence, validation, and basic error handling have been implemented and tested. Frontend integration, authentication, automated deployment, and cloud hosting are planned next steps.

## Learning Objectives

This project is an opportunity to develop practical skills in full-stack application development, REST API design, database management, secure authentication, automated testing, CI/CD, and cloud deployment.



## Troubleshooting and Debugging

During development, I encountered and resolved several issues while testing the Student Management API.

### 1. Swagger UI Validation Error for Route Parameters

**Problem**

When testing the `PUT` and `DELETE` endpoints through Swagger UI, I encountered the following validation error:

```text
Please correct the following validation errors and try again.
For 'id': Required field is not provided.
```

The error appeared after clicking **Execute**, even though the endpoint required a student ID in the URL.

**Debugging process**

- Confirmed that the `id` field was visible in Swagger UI after clicking **Try it out**.
- Investigated the request configuration and how Swagger UI handled the required route parameter.
- Adjusted the API configuration and retested the affected endpoints.

**Resolution**

The issue was resolved after making changes to the application's JSON configuration and OpenAPI setup.

The following configuration was added to `Program.cs`:

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling =
        System.Text.Json.Serialization.JsonNumberHandling.Strict;
});

builder.Services.AddOpenApi();
```

The OpenAPI document was exposed using:

```csharp
app.MapOpenApi();
```

Swagger UI was configured to use the generated OpenAPI document:

```csharp
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/openapi/v1.json",
        "Student Management API v1");
});
```

**Lesson learned**

This experience reinforced the importance of inspecting API requests, checking route parameters, understanding JSON serialization, and testing endpoints after making configuration changes.

> Note: The exact root cause of the Swagger validation error has not been independently confirmed. The configuration above records the changes made during troubleshooting rather than establishing that JSON number handling was definitively responsible for the fix.
