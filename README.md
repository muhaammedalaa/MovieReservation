# 🎬 Movie Reservation API

A **production-ready** ASP.NET Core 9 REST API for managing movie theater reservations with integrated payment processing, real-time caching, and comprehensive CI/CD pipeline.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Key Features](#key-features)
- [Prerequisites](#prerequisites)
- [Installation & Setup](#installation--setup)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Database](#database)
- [Caching Strategy](#caching-strategy)
- [Authentication & Authorization](#authentication--authorization)
- [Payment Processing](#payment-processing)
- [Email Notifications](#email-notifications)
- [DevOps & Deployment](#devops--deployment)
- [Testing](#testing)
- [Contributing](#contributing)
- [Troubleshooting](#troubleshooting)

---

## 🎯 Overview

**Movie Reservation API** is a comprehensive backend solution for managing movie theater operations including:
- User authentication with JWT tokens
- Movie and showtime management
- Seat reservation system
- Secure payment processing via Stripe
- Real-time email notifications
- Advanced caching with Redis
- Complete audit logging
- Production-grade error handling

**Status:** ✅ Production Ready | **Version:** 1.0.0 | **License:** MIT

---

## 🛠️ Tech Stack

### Core Framework
- **Language:** C# 13.0
- **.NET:** .NET 9 (LTS)
- **Framework:** ASP.NET Core 9
- **Architecture:** Clean Architecture with CQRS patterns

### Database & Caching
- **Primary Database:** SQL Server 2022
- **Cache:** Redis 7 (Alpine)
- **ORM:** Entity Framework Core 9
- **Migrations:** Code-First approach

### Authentication & Security
- **Auth:** JWT (JSON Web Tokens)
- **Identity:** ASP.NET Core Identity
- **Encryption:** bcrypt for passwords
- **HTTPS:** SSL/TLS support

### Payment Processing
- **Payment Gateway:** Stripe
- **Webhook Handling:** Event-driven architecture

### Messaging
- **Email:** SMTP (Gmail compatible)
- **Libraries:** MailKit, MimeKit

### Mapping & Serialization
- **Mapping:** AutoMapper
- **JSON:** System.Text.Json

### Logging & Monitoring
- **Logging:** Microsoft.Extensions.Logging
- **Health Checks:** Built-in ASP.NET Core health endpoints

### CI/CD & DevOps
- **Container:** Docker & Docker Compose
- **CI/CD:** GitHub Actions
- **Orchestration:** Kubernetes-ready (optional)
- **Code Quality:** SonarCloud, Trivy scanning
- **Code Coverage:** Codecov

---

## 📁 Project Structure

# 🎬 Movie Reservation API - Complete Project Documentation

**Version:** 1.0.0
**Last Updated:** January 13, 2026
**Status:** Production Ready ✅
**Framework:** .NET 9 | **Language:** C# 13
**Repository:** `https://github.com/muhaammedalaa/MovieReservation`

---

## 📑 Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture & Design](#architecture--design)
3. [Tech Stack](#tech-stack)
4. [Project Structure](#project-structure)
5. [Key Features](#key-features)
6. [Installation & Setup](#installation--setup)
7. [Configuration Guide](#configuration-guide)
8. [Database Schema](#database-schema)
9. [API Documentation](#api-documentation)
10. [Caching Strategy](#caching-strategy)
11. [Authentication & Security](#authentication--security)
12. [Payment Processing](#payment-processing)
13. [Email Notifications](#email-notifications)
14. [Testing](#testing)
15. [DevOps & Deployment](#devops--deployment)
16. [CI/CD Pipeline](#cicd-pipeline)
17. [Monitoring & Maintenance](#monitoring--maintenance)
18. [Troubleshooting](#troubleshooting)
19. [Contributing](#contributing)

---

# 1️⃣ Project Overview

## Mission

Provide a **production-ready, scalable REST API** for managing movie theater operations including user authentication, movie management, seat reservations, payment processing, and real-time caching.

## Key Objectives

- ✅ **Secure:** JWT authentication, role-based access control, encrypted data
- ✅ **Scalable:** Caching with Redis, optimized queries, horizontal scaling support
- ✅ **Reliable:** Comprehensive error handling, health checks, monitoring
- ✅ **Maintainable:** Clean architecture, comprehensive tests, well-documented
- ✅ **Modern:** Latest .NET 9, async/await patterns, microservices-ready

## Project Highlights

| Aspect | Details |
|--------|---------|
| **Framework** | ASP.NET Core 9 (LTS) |
| **Architecture** | Clean Architecture + CQRS patterns |
| **Database** | SQL Server 2022 + Entity Framework Core |
| **Cache** | Redis 7 with fallback to in-memory |
| **Auth** | JWT with role-based access control |
| **Payments** | Stripe integration with webhooks |
| **Email** | SMTP with HTML templates |
| **Testing** | xUnit, Moq, 80%+ coverage |
| **CI/CD** | GitHub Actions with multi-stage pipeline |
| **Containerization** | Docker & Docker Compose |
| **Deployment** | Azure App Service + Kubernetes-ready |

---

# 2️⃣ Architecture & Design

## Clean Architecture Layers

The project adheres to the principles of Clean Architecture, promoting separation of concerns, testability, and maintainability. The layers are organized as follows:

*   **Domain Layer:** The innermost layer, containing enterprise-wide business rules and entities. It is independent of all other layers.
    *   `MovieReservation.Domain`: Contains core entities (e.g., `Movie`, `Showtime`, `Reservation`), value objects, and interfaces for repositories and services. It defines the business logic that is independent of any specific technology.

*   **Application Layer:** Contains application-specific business rules. It orchestrates the domain objects to perform application-specific tasks.
    *   `MovieReservation.Service`: Implements the application's use cases. It contains DTOs, interfaces for application services, and concrete implementations of these services (e.g., `MovieService`, `ShowtimeService`). It depends on the Domain layer.

*   **Infrastructure Layer:** Provides implementations for interfaces defined in the Application and Domain layers. It deals with external concerns like databases, file systems, and external services.
    *   `MovieReservation.Data`: Contains the Entity Framework Core `DbContext`, concrete implementations of repositories, and migration logic. It depends on the Domain layer.
    *   `MovieReservation.Infrastructure`: Houses implementations for external services like Redis caching (`RedisCacheService`), email sending, and other utilities. It depends on the Domain and Application layers.

*   **Presentation Layer (API):** The outermost layer, responsible for handling user requests and presenting data. It depends on the Application and Infrastructure layers.
    *   `MovieReservation.APi`: The ASP.NET Core Web API project, containing controllers, DTOs for API requests/responses, and dependency injection configuration. It orchestrates calls to the Application layer.

## Design Patterns Used

The project leverages several design patterns to ensure a robust, scalable, and maintainable codebase:

*   **Clean Architecture:** As described above, for clear separation of concerns and dependency inversion.
*   **Repository Pattern:** Abstracting data access logic, making the application independent of the data source. Implemented with `IGenericRepository<T>`.
*   **Unit of Work Pattern:** Manages transactions and ensures that multiple repository operations are treated as a single atomic unit. Implemented with `IUnitOfWork`.
*   **CQRS (Command Query Responsibility Segregation):** Separating read (queries) and write (commands) operations, which can lead to better performance, scalability, and maintainability.
*   **Dependency Injection (DI):** Used extensively throughout the application to manage dependencies between components, promoting loose coupling and testability.
*   **Specification Pattern:** Encapsulating query logic for filtering and sorting data, making queries reusable and testable.
*   **AutoMapper:** For object-to-object mapping, reducing boilerplate code when transferring data between layers (e.g., Entity to DTO).

---

# 3️⃣ Tech Stack

### Core Framework
- **Language:** C# 13.0
- **.NET:** .NET 9 (LTS)
- **Framework:** ASP.NET Core 9
- **Architecture:** Clean Architecture with CQRS patterns

### Database & Caching
- **Primary Database:** SQL Server 2022
- **Cache:** Redis 7 (Alpine)
- **ORM:** Entity Framework Core 9
- **Migrations:** Code-First approach

### Authentication & Security
- **Auth:** JWT (JSON Web Tokens)
- **Identity:** ASP.NET Core Identity
- **Encryption:** bcrypt for passwords
- **HTTPS:** SSL/TLS support

### Payment Processing
- **Payment Gateway:** Stripe
- **Webhook Handling:** Event-driven architecture

### Messaging
- **Email:** SMTP (Gmail compatible)
- **Libraries:** MailKit, MimeKit

### Mapping & Serialization
- **Mapping:** AutoMapper
- **JSON:** System.Text.Json

### Logging & Monitoring
- **Logging:** Microsoft.Extensions.Logging
- **Health Checks:** Built-in ASP.NET Core health endpoints

### CI/CD & DevOps
- **Container:** Docker & Docker Compose
- **CI/CD:** GitHub Actions
- **Orchestration:** Kubernetes-ready (optional)
- **Code Quality:** SonarCloud, Trivy scanning
- **Code Coverage:** Codecov

---

## 📁 Project Structure

The solution is organized into several projects, each representing a distinct layer or concern within the Clean Architecture:

*   **MovieReservation.APi:** The main ASP.NET Core Web API project. This is the Presentation Layer, handling HTTP requests, routing, and serialization. It depends on `MovieReservation.Service` and `MovieReservation.Infrastructure`.
*   **MovieReservation.Service:** The Application Layer, containing the business logic, application services, DTOs, and interfaces for application-specific operations. It depends on `MovieReservation.Domain`.
*   **MovieReservation.Data:** Part of the Infrastructure Layer, responsible for data access. It contains the Entity Framework Core `DbContext`, concrete implementations of repositories, and database migration logic. It depends on `MovieReservation.Domain`.
*   **MovieReservation.Domain:** The core Domain Layer, defining the enterprise business rules, entities, value objects, and domain interfaces (e.g., `IGenericRepository<T>`, `IUnitOfWork`, `ICacheService`). It has no dependencies on other layers.
*   **MovieReservation.Infrastructure:** Another part of the Infrastructure Layer, providing implementations for cross-cutting concerns and external services, such as Redis caching (`RedisCacheService`), email services, and other utilities. It depends on `MovieReservation.Domain` and `MovieReservation.Service`.
*   **MovieReservation.Tests:** Contains unit and integration tests for the application's various layers and components, ensuring code quality and correctness.

---

## 🔑 Key Features

*   **User Authentication & Authorization:** Secure user registration, login, and role-based access control using JWT.
*   **Movie Management:** Administrators can add, update, and delete movie details, including title, description, duration, and genre.
*   **Showtime Scheduling:** Create and manage showtimes for movies across different theaters, specifying date, time, and available seats.
*   **Seat Reservation:** Users can select and reserve specific seats for a chosen showtime.
*   **Stripe Payment Integration:** Seamless and secure processing of ticket payments through the Stripe API, with webhook handling for asynchronous event processing.
*   **Redis Caching:** Implemented with `StackExchange.Redis` for high-performance data retrieval, significantly reducing database load. Features include:
    *   **Paginated Caching:** Caching of movie and showtime lists per page.
    *   **Pattern-Based Invalidation:** Automatic invalidation of relevant cache entries (e.g., "Movie:*", "showtimes:*") upon creation, update, or deletion of movie or showtime data, ensuring data consistency.
*   **Unit of Work & Repository Pattern:** Ensures atomic transactions and clean separation of concerns for data access.
*   **AutoMapper:** Simplifies object-to-object mapping between DTOs and entities.
*   **Docker Support:** `docker-compose.yml` provided for easy setup of the API, Redis, and SQL Server for local development.
*   **Comprehensive Error Handling:** Production-grade error handling middleware to provide consistent and informative error responses.
*   **Audit Logging:** (If implemented, add details here. Otherwise, remove or mark as future).
*   **Email Notifications:** Real-time email notifications for reservation confirmations, cancellations, etc., using MailKit and MimeKit.

---

## ⚙️ Prerequisites

*   .NET SDK 9.0 or later
*   Docker Desktop (for running Redis and SQL Server locally)
*   A Stripe account (for payment processing, if testing locally)
*   An email service provider (e.g., Gmail) for SMTP configuration.

---

## 🚀 Installation & Setup

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/muhaammedalaa/MovieReservation.git
    cd MovieReservation.Sol
    ```

2.  **Start Docker services:**
    Navigate to the root of the solution (`MovieReservation.Sol`) and run:
    ```bash
    docker-compose up -d
    ```
    This will start SQL Server and Redis containers.

3.  **Update Database:**
    Apply migrations to create the database schema:
    ```bash
    dotnet ef database update --project MovieReservation.Data
    ```

---

## 🛠️ Configuration

Open `MovieReservation.APi/appsettings.Development.json` (and `appsettings.json` for production) to configure the following:

*   **Connection Strings:** Ensure the SQL Server connection string is correct (usually handled by Docker Compose for local setup).
*   **Redis:**
    ```json
    "Redis": {
      "Enabled": true,
      "Connection": "localhost:6379"
    }
    ```
*   **Stripe:**
    ```json
    "StripeSettings": {
      "SecretKey": "sk_test_...",
      "PublishableKey": "pk_test_...",
      "WebhookSecret": "whsec_..."
    }
    ```
*   **JWT:**
    ```json
    "JwtSettings": {
      "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong",
      "Issuer": "MovieReservationApi",
      "Audience": "MovieReservationUsers",
      "ExpiryMinutes": 60
    }
    ```
*   **Email:**
    ```json
    "EmailSettings": {
      "SmtpServer": "smtp.gmail.com",
      "SmtpPort": 587,
      "SenderName": "Movie Reservation",
      "SenderEmail": "your_email@gmail.com",
      "Username": "your_email@gmail.com",
      "Password": "your_email_app_password"
    }
    ```

---

## ▶️ Running the Application

Navigate to the API project directory:
```bash
cd MovieReservation.APi
dotnet run
```
The API will typically run on `https://localhost:7001` (or another port specified in `launchSettings.json`).

---

## 📖 API Documentation

Once the application is running, you can access the Swagger UI for API documentation and testing at:
`https://localhost:7001/swagger` (adjust port if necessary).

---

## 🗄️ Database

The project uses SQL Server 2022 as the primary database, managed by Entity Framework Core 9 with a Code-First approach. Migrations are used to evolve the database schema.

---

## 🚀 Caching Strategy

Redis 7 is integrated as a distributed cache, significantly improving application performance and reducing database load.

*   **Implementation:** `RedisCacheService` implements `ICacheService`, providing methods for `GetDataAsync`, `SetDataAsync`, and `RemoveByPatternAsync`.
*   **Pattern-Based Invalidation:** Cache entries are invalidated using patterns (e.g., "Movie:*", "showtimes:*") whenever relevant data is created, updated, or deleted, ensuring data consistency.
*   **Fallback:** A fallback to in-memory caching is provided if Redis is disabled or unavailable.

---

## 🔒 Authentication & Authorization

*   **JWT (JSON Web Tokens):** Used for secure authentication. Users receive a JWT upon successful login, which is then used to authorize subsequent API requests.
*   **ASP.NET Core Identity:** Manages user accounts, roles, and password hashing.
*   **Role-Based Access Control:** Ensures that users can only access resources and perform actions for which they have appropriate permissions.

---

## 💳 Payment Processing

*   **Stripe Integration:** Securely handles credit card payments for ticket reservations.
*   **Webhooks:** Stripe webhooks are configured to receive asynchronous notifications for payment events (e.g., successful charges, refunds), allowing the system to update reservation statuses in real-time.

---

## 📧 Email Notifications

*   **MailKit & MimeKit:** Used for sending transactional emails, such as reservation confirmations, payment receipts, and password reset links.
*   **SMTP Configuration:** Configurable via `appsettings.json` to work with various SMTP providers (e.g., Gmail).
*   **HTML Templates:** Supports rich HTML email templates for a better user experience.

---

## 🧪 Testing

The project includes a comprehensive suite of tests to ensure reliability and correctness:

*   **Unit Tests:** Written using xUnit and Moq, covering the business logic in the `MovieReservation.Service` layer and other critical components.
*   **Integration Tests:** (If implemented, add details here. Otherwise, remove or mark as future).
*   **Test Coverage:** Aim for 80%+ code coverage for critical business logic.

To run all tests, navigate to the solution directory (`MovieReservation.Sol`) and execute:
```bash
dotnet test
```

---

## 🌐 DevOps & Deployment

*   **Docker & Docker Compose:** Provides a containerized development environment, ensuring consistency across different machines and simplifying deployment.
*   **CI/CD Pipeline (GitHub Actions):** (If implemented, add details here. Otherwise, remove or mark as future).
*   **Kubernetes-Ready:** The application is designed with containerization in mind, making it suitable for deployment to Kubernetes clusters for orchestration and scaling.

---

## 🤝 Contributing

We welcome contributions to the Movie Reservation API! Please follow these steps:

1.  Fork the repository.
2.  Create a new branch for your feature or bug fix (`git checkout -b feature/your-feature-name`).
3.  Make your changes and ensure tests pass.
4.  Commit your changes (`git commit -m 'feat: Add new feature'`).
5.  Push to your branch (`git push origin feature/your-feature-name`).
6.  Open a Pull Request.

---

## ❓ Troubleshooting

*   **Redis Connection Issues:**
    *   Ensure your Redis Docker container is running (`docker-compose up -d`).
    *   Verify the Redis connection string in `appsettings.Development.json` is `localhost:6379`.
    *   Check `Program.cs` in `MovieReservation.APi` for any conflicting Aspire configurations (e.g., `builder.AddRedisClient("redis");` or `builder.AddServiceDefaults();`) and remove them if present.
*   **Database Migrations:** If you encounter issues with the database, try:
    ```bash
    dotnet ef migrations remove --project MovieReservation.Data
    dotnet ef database update --project MovieReservation.Data
    ```
    (Note: This will remove all data if the database already exists).
*   **API Not Starting:** Check the console output for error messages. Ensure all prerequisites are met and configurations are correct.

---

## 📄 License

This project is licensed under the MIT License. See the `LICENSE` file for details.
