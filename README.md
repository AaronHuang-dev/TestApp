# 🚀 ASP.NET Core Minimal API - Member Management & Unit Testing System

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)
![xUnit](https://img.shields.io/badge/Testing-xUnit-512BD4)

A lightweight Member Management System API built with **ASP.NET Core Minimal API**. This project demonstrates software engineering best practices, including **Separation of Concerns (SoC)** and **Pure Function Design**, decoupling business logic from HTTP endpoints to ensure high testability with **xUnit**.

---

## ✨ Key Features & Technical Highlights

- **Lightweight Architecture**: Built using ASP.NET Core Minimal API for high performance and low overhead.
- **Test-Driven & Highly Testable Design**:
  - Extracted authentication and data manipulation logic into a dedicated `AuthService` layer.
  - Implemented side-effect-free **pure functions**, enabling fast and isolated unit tests without relying on physical file system IO.
- **Full Member CRUD Operations**:
  - `POST /register`: Account registration with conflict checking and initialization.
  - `POST /login`: Secure authentication.
  - `PUT /update`: Password updating with old password verification and immutable array processing.
  - `DELETE /delete`: User account deletion with authorization check and list rebuilding.
- **OpenAPI / Swagger Integration**: Native OpenAPI support for seamless API documentation and interactive testing.
- **Developer-Friendly**: Includes a `.http` file for instant endpoint testing directly via Visual Studio or VS Code REST Client.

---

## 🛠️ Tech Stack

- **Framework**: .NET 10.0 / ASP.NET Core Minimal API
- **Testing Framework**: xUnit, Microsoft.NET.Test.Sdk
- **API Spec**: Microsoft.AspNetCore.OpenApi
- **Data Store**: CSV File Storage (chosen to simplify the persistence layer and highlight service decoupling & unit testing)

---

## 📁 Project Structure

```text
TestApp/
├── Properties/                # Application launch settings
│   └── launchSettings.json
├── TESTAPP.TESTS/             # Unit Test Project Folder (xUnit)
│   ├── AuthServiceTests.cs    # Unit tests for AuthService logic
│   └── TestApp.Tests.csproj   # Test project configuration
├── wwwroot/                   # Static files directory
├── AuthService.cs             # Core authentication & CSV processing (Pure functions)
├── Program.cs                 # Minimal API routes & middleware configuration
├── TestApp.csproj             # Web API project configuration
├── TestApp.http               # REST Client endpoint test script
├── appsettings.json           # Global configuration
├── appsettings.Development.json # Development settings
├── login_tutorial.html        # Frontend demonstration interface
└── users.csv                  # Lightweight CSV data store
