# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

NetPcContacts is a full-stack contact management application consisting of:
- **Backend**: .NET 8.0 Web API built with Clean Architecture and CQRS pattern, using SQLite for data persistence and ASP.NET Core Identity for authentication
- **Frontend**: Angular 20 standalone application with zoneless change detection, located in `NetPcContacts.Api/NetPcContacts.UI/`

## Architecture

The project follows Clean Architecture with four main layers:

### Domain Layer (`NetPcContacts.Domain`)
- Core business entities: `Contact`, `Category`, `Subcategory`, `User`
- Repository interfaces (`IRepositories/`)
- Domain exceptions and constants
- No external dependencies (except Identity for User entity)

### Application Layer (`NetPcContacts.Application`)
- CQRS implementation using MediatR
- Commands: `CreateContact`, `UpdateContact`, `DeleteContact`
- Queries: `GetAllContacts`, `GetContactById`
- FluentValidation validators for all commands and queries
- DTOs: `ContactDto`, `BasicContactDto`, `PagedResult<T>`
- Organized by feature folders (e.g., `Contacts/Commands`, `Contacts/Queries`)

### Infrastructure Layer (`NetPcContacts.Infrastructure`)
- EF Core DbContext: `NetPcContactsDbContext` (inherits `IdentityDbContext<User>`)
- Repository implementations (`Repositories/`)
- Database migrations (`Migrations/`)
- Data seeders (`Seeders/`)
- Uses SQLite with connection string in `appsettings.Development.json`
- Exposes internals to test project via `InternalsVisibleTo`

### Presentation Layer (`NetPcContacts.Api`)
- ASP.NET Core Web API controllers
- Swagger/OpenAPI with XML documentation
- Global exception handling using `IExceptionHandler` with ProblemDetails
- Rate limiting with multiple policies (auth, commands, queries)
- CORS configuration (allowing Angular UI origin)
- Bearer token authentication (1 minute expiration)
- Hosts Angular UI project (`NetPcContacts.UI/`)

## Dependency Injection Setup

Each layer registers its services via extension methods:
- **Application**: `ServiceCollectionExtensions.AddApplication()` - registers MediatR, FluentValidation
- **Infrastructure**: `ServiceCollectionExtensions.AddInfrastructure(configuration)` - registers DbContext, Identity, repositories, seeders
- **API**: `WebApplicationBuilderExtensions.AddPresentation()` - registers controllers, Swagger, authentication, rate limiting, CORS

## Common Commands

### Backend (.NET API)

#### Build & Run
```bash
# Build entire solution
dotnet build

# Run API project
dotnet run --project NetPcContacts.Api

# Watch mode (auto-reload)
dotnet watch --project NetPcContacts.Api
```

#### Database Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName --project NetPcContacts.Infrastructure --startup-project NetPcContacts.Api

# Update database
dotnet ef database update --project NetPcContacts.Infrastructure --startup-project NetPcContacts.Api

# Remove last migration (if not applied)
dotnet ef migrations remove --project NetPcContacts.Infrastructure --startup-project NetPcContacts.Api

# Drop database
dotnet ef database drop --project NetPcContacts.Infrastructure --startup-project NetPcContacts.Api
```

#### Testing
```bash
# Run all tests
dotnet test

# Run tests for specific project
dotnet test NetPcContacts.Api.Tests
dotnet test NetPcContacts.Application.Tests
dotnet test NetPcContacts.Infrastructure.Tests

# Run single test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend (Angular UI)

All Angular commands must be run from the `NetPcContacts.Api/NetPcContacts.UI/` directory.

#### Development
```bash
# Install dependencies (first time setup)
npm install

# Start development server (default: http://localhost:4200)
npm start
# or
ng serve

# Build for production
npm run build
# or
ng build

# Watch mode (rebuild on file changes)
npm run watch
```

#### Testing
```bash
# Run unit tests with Karma
npm test
# or
ng test
```

## Key Design Patterns & Conventions

### Backend (.NET)

#### CQRS with MediatR
- All business logic flows through MediatR handlers
- Commands return: `int` (for Create), `Task` (for Update/Delete)
- Queries return: `ContactDto`, `PagedResult<BasicContactDto>`
- Controllers are thin - they only mediate requests to MediatR

#### Validation
- FluentValidation used for all commands and queries
- Validators follow naming: `[CommandName]Validator`
- Auto-validation enabled in Application layer

#### Repository Pattern
- Repositories defined as interfaces in Domain layer
- Implementations in Infrastructure layer
- All repositories are scoped services

#### Rate Limiting Policies
- **Global**: 100 requests/minute (fixed window)
- **auth**: 10 requests/minute (for Identity endpoints)
- **commands**: 30 requests/minute (POST/PUT/DELETE - sliding window)
- **queries**: 100 tokens/minute (GET - token bucket)

#### Authentication & Authorization
- Public endpoints: GET `/api/contact` and GET `/api/contact/{id}`
- Protected endpoints: POST, PATCH, DELETE (require `[Authorize]` attribute)
- Identity API endpoints: `/api/identity/register`, `/api/identity/login`
- Bearer tokens expire after 1 minute (configured in Infrastructure)

#### Contact Entity Business Rules
- Email must be unique across all contacts
- Password is hashed using `IPasswordHasher<Contact>`
- Category is required (`CategoryId` foreign key)
- Subcategory is optional (`SubcategoryId` nullable):
  - Required for "Służbowy" (Business) category
  - Null for "Prywatny" (Private) category
  - Null for "Inny" (Other) category
- CustomSubcategory is used only when Category = "Inny"

#### Update Contact Pattern
- **Important**: Update endpoints use a separate DTO (`UpdateContactDto`) that excludes the `Id` field
- The `Id` is passed via route parameter (`/api/contact/{id}`) and manually set in the controller
- This prevents JSON deserialization from setting `Id = 0`, which would fail validation
- Password is optional on update - if null/empty, the existing password is retained
- Pattern: Controller receives DTO → maps to Command → sets Id from route → sends to MediatR

#### Error Handling
- Global exception handler using `IExceptionHandler` pattern (`GlobalExceptionHandler`)
- Returns standardized ProblemDetails responses with `requestId` in extensions
- Domain exceptions in `NetPcContacts.Domain/Exceptions/`
- Exception to status code mapping:
  - `ValidationException` → 400 Bad Request
  - `NotFoundException` → 404 Not Found
  - `DuplicateEmailException` → 409 Conflict
  - All other exceptions → 500 Internal Server Error
- Comprehensive logging with user context (userId, IP, endpoint, timestamp)

#### XML Documentation
- API project generates XML documentation file for Swagger
- NoWarn 1591 suppresses missing XML comment warnings
- All controllers and actions should have `<summary>` and `<remarks>` tags

### Frontend (Angular)

#### Modern Angular Architecture
- **Standalone Components**: All components use standalone: true (no NgModules)
- **Zoneless Change Detection**: Uses signals and `provideZonelessChangeDetection()`
- **Modern Control Flow**: Uses `@if`, `@for`, `@switch` instead of `*ngIf`, `*ngFor`, `*ngSwitch`
- **Functional Interceptors**: HTTP interceptors are functional (`jwtInterceptor`, `errorInterceptor`)
- **Signals**: State management uses signals (e.g., `signal()` for reactive state)

#### UI Framework & Styling
- **Angular Material**: Component library (`@angular/material` v20.2.8)
- **Tailwind CSS**: Utility-first CSS framework (v3.4.18)
- **Custom Theme**: Material theming configured in `src/custom-theme.scss`
- **PostCSS & Autoprefixer**: For CSS processing

#### Project Structure
- `src/app/components/`: UI components (`login`, `contact-list`, `contact-details`)
- `src/app/services/`: Services (e.g., `auth-service.ts`)
- `src/app/models/`: TypeScript models (`Contact`, `BasicContact`, `LoginRequest`, `LoginResponse`)
- `src/app/interceptors/`: HTTP interceptors for JWT and error handling
- `src/environments/`: Environment configuration (API URL: `https://localhost:7076`)

#### HTTP Interceptors
- **JWT Interceptor**: Automatically attaches bearer token to API requests
- **Error Interceptor**: Handles HTTP errors globally, attempts token refresh on 401

#### Signal Effects Pattern
- Effects in services (e.g., `AuthService`) use `effect()` to sync state changes
- Side effects (localStorage, cookies) should be wrapped in `untracked()` to prevent reactive loops
- Example: Token changes trigger effects that update storage without causing additional reactive updates

## Testing Stack

### Backend (.NET)
- **xUnit**: Test framework
- **Moq**: Mocking library
- **FluentAssertions**: Assertion library
- **Microsoft.AspNetCore.Mvc.Testing**: Integration tests
- **coverlet.collector**: Code coverage

### Frontend (Angular)
- **Jasmine**: Test framework (v5.9.0)
- **Karma**: Test runner (v6.4.0)
- **karma-coverage**: Code coverage reporting

## Database Configuration
- **Database**: SQLite (`NetPcContacts.sqlite` in API project root)
- **Connection String**: `Data Source=NetPcContacts.sqlite`
- **EF Core Configuration**:
  - Email uniqueness via index
  - MaxLength constraints matching FluentValidation rules
  - CASCADE delete behavior set to `SetNull` for all relationships
- **Seeding**: Database is seeded on application startup via `IApplicationSeeder`

## Important Implementation Notes

### Adding New CQRS Commands/Queries
When adding new commands or queries, follow this pattern:
1. Create command/query class in `Application/[Feature]/Commands|Queries/[Name]/`
2. Create validator class `[Name]Validator` in same folder
3. Create handler class `[Name]Handler` implementing `IRequestHandler<TRequest, TResponse>`
4. Add DTO if returning data to API (keep DTOs separate from commands/queries)
5. If updating entities via route parameter, use a separate DTO without the Id field (see `UpdateContactDto` pattern)

### Angular Dialog Components
- Dialogs can operate in multiple modes (add/edit) using the same component
- Use `MAT_DIALOG_DATA` to pass mode-specific data (e.g., `contactId` for edit mode)
- Set up mode detection in `ngOnInit()` using signals (e.g., `isEditMode = signal(false)`)
- Load existing data in edit mode and adjust form validators accordingly
- Return boolean from `dialogRef.close(result)` to indicate success/cancellation

### Contact Management Features
- **Create**: Requires all fields including password
- **Update**: Password optional, uses `UpdateContactDto` (no Id field)
- **Delete**: Requires confirmation dialog before deletion
- **View Details**: Read-only dialog showing full contact information
- All mutations (create/update/delete) require authentication and show success/error snackbar messages
- to memorize