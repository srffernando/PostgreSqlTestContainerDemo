
# 🧪 PostgreSQL Integration Testing with Testcontainers (.NET)

This repository demonstrates how to build robust, isolated, and repeatable integration tests using:
- ✅ **.NET 9**
- ✅ **NUnit**
- ✅ **Entity Framework Core (EF Core)**
- ✅ **Testcontainers for .NET** (PostgreSQL)
- ✅ **Respawn** for database reset
- ✅ **Bogus** for fake test data generation

---

## 📦 Project Structure

```
📁 PostgreSqlTestContainerDemo/
├── .gitattributes
├── .gitignore
├── NuGet.config
├── PostgreSqlTestContainerDemo.csproj
├── PostgreSqlTestContainerDemo.sln
├── Program.cs
├── structure.txt
│
├── 📁 Data/
│   ├── DesignTimeDbContextFactory.cs
│   └── MyDbContext.cs
│
├── 📁 Helpers/
│   ├── PostgreTestContainer.cs
│   └── TestDataSeeder.cs
│
├── 📁 Migrations/
│   └── YourDbContextModelSnapshot.cs
│
├── 📁 Models/
│   └── SampleEntity.cs
│
├── 📁 Tests/
│   ├── NonResettingTests.cs
│   └── SampleIntegrationTests.cs
```

---

## ⚙️ Technologies & Packages Used

| Package | Version | Description |
|--------|---------|-------------|
| `Microsoft.NET.Test.Sdk` | 17.8.0 | Required for running unit tests |
| `NUnit` | 3.13.3 | Testing framework |
| `NUnit3TestAdapter` | 4.4.2 | Test adapter for running NUnit tests |
| `Microsoft.EntityFrameworkCore` | 8.0.0 | EF Core ORM |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.0 | Design-time tools |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.0 | PostgreSQL EF Core provider |
| `Testcontainers.PostgreSql` | 3.0.0 | Testcontainers library for PostgreSQL |
| `Respawn` | 6.2.1 | Reset database between tests |
| `Bogus` | 34.0.2 | Fake data generation for tests |

---

## 🚀 How to Run the Tests

### 🔧 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker](https://www.docker.com/products/docker-desktop) (must be installed and running)

### ▶️ Run Tests

```bash
dotnet test
```

> 💡 Docker will automatically spin up a PostgreSQL container for your tests.

---

## 🧪 Test Organization

| Category | Purpose |
|----------|---------|
| `CRUD` | Basic create, read, update, delete tests |
| `Lookup` | Entity queries by ID/Name |
| `Validation` | Nullable field validation |
| `Exception` | Tests for expected failures |
| `Parallel` | Safe parallel execution |
| `NoReset` | Tests that run without resetting the DB between runs |

Run by category:

```bash
dotnet test --filter Category=CRUD
```

---

## 🧰 Features Demonstrated

- Ephemeral PostgreSQL containers via Testcontainers
- Fake data generation via Bogus
- DB cleanup/reset using Respawn
- Parallel-safe test execution with `SemaphoreSlim`
- Exception assertions (`Throws`, `ThrowsAsync`)
- Stateful test scenarios without DB resets

---

## 📝 License

MIT License

---

## 👤 Author

Built by **[Ramesh Franklin Fernando]** – passionate about QA Automation, .NET, and containerized testing!
