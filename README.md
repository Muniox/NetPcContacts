# NetPcContacts - Technical Specification

## Project Overview

NetPcContacts is a full-stack contact management application built with .NET 8.0 Web API (backend) and Angular 20 (frontend). The application follows Clean Architecture principles with CQRS pattern and provides comprehensive contact management functionality with authentication and authorization.

## Features

### 1. User Authentication
- Login functionality using ASP.NET Core Identity
- Bearer token authentication (1 minute expiration)
- Identity API endpoints: `/api/identity/register`, `/api/identity/login`

### 2. Contact List Browsing (Public Access)
- Available for unauthenticated users
- Displays basic contact information
- Pagination, sorting, and searching capabilities
- Details view for individual contacts

### 3. Contact Management (Requires Authentication)
Authenticated users can:
- **View** contact details
- **Create** new contacts
- **Edit** existing contacts
- **Delete** contacts

#### Contact Properties:
- **First Name** (required, 1-100 characters)
- **Last Name** (required, 1-100 characters)
- **Email** (required, unique, max 255 characters, valid email format)
- **Password** (required, 8-100 characters, complexity requirements)
- **Phone Number** (required, 9-20 characters, specific format)
- **Birth Date** (required, must be in the past, not older than 150 years)
- **Category** (required, from dictionary: Business/Private/Other)
- **Subcategory** (conditional):
  - Required for "Business" category (from dictionary: Boss, Client, Supplier, etc.)
  - Null for "Private" category
  - Custom text field for "Other" category (max 100 characters)

## Architecture

The project follows Clean Architecture with four main layers:

### 1. Domain Layer (`NetPcContacts.Domain`)
**Purpose**: Contains core business entities, repository interfaces, and domain logic

**Entities**:
- `Contact` - Main entity representing a contact with all properties
- `Category` - Dictionary entity for contact categories
- `Subcategory` - Dictionary entity for subcategories
- `User` - Identity entity for authentication

**Interfaces** (`IRepositories/`):
- `IContactsRepository` - Contact data access operations
- `ICategoryRepository` - Category data access operations
- `ISubcategoryRepository` - Subcategory data access operations

**Exceptions** (`Exceptions/`):
- `NotFoundException` - Thrown when entity is not found
- `DuplicateEmailException` - Thrown when email already exists

**Dependencies**: None (except ASP.NET Core Identity for User entity)

### 2. Application Layer (`NetPcContacts.Application`)
**Purpose**: Implements business logic using CQRS pattern with MediatR

**Commands** (`Contacts/Commands/`):

1. **CreateContact**
   - Handler: `CreateContactCommandHandler`
   - Validator: `CreateContactCommandValidator`
   - Business logic:
     - Validates all contact fields
     - Checks email uniqueness
     - Verifies category and subcategory existence
     - Hashes password using `IPasswordHasher<Contact>`
   - Returns: `int` (new contact ID)

2. **UpdateContact**
   - Handler: `UpdateContactCommandHandler`
   - Validator: `UpdateContactCommandValidator`
   - Uses: `UpdateContactDto` (without Id field)
   - Business logic:
     - Validates contact existence
     - Checks email uniqueness if changed
     - Optional password update
     - Verifies category and subcategory relationships
   - Returns: `Task` (void)

3. **DeleteContact**
   - Handler: `DeleteContactCommandHandler`
   - Business logic:
     - Validates contact existence
     - Performs soft or hard delete
   - Returns: `Task` (void)

**Queries** (`Contacts/Queries/`):

1. **GetAllContacts**
   - Handler: `GetAllContactsQueryHandler`
   - Validator: `GetAllContactsQueryValidator`
   - Features:
     - Pagination (page size: 5, 10, 15, 30)
     - Sorting by: FirstName, LastName, Category
     - Search by: Name, Surname, Email
   - Returns: `PagedResult<BasicContactDto>`

2. **GetContactById**
   - Handler: `GetContactByIdQueryHandler`
   - Business logic:
     - Validates contact existence
     - Returns full contact details
   - Returns: `ContactDto`

**DTOs** (`DTOs/`):
- `ContactDto` - Full contact information
- `BasicContactDto` - Basic contact information for list view
- `UpdateContactDto` - Update DTO without Id field
- `PagedResult<T>` - Generic paged result wrapper

**Validation**: FluentValidation for all commands and queries

### 3. Infrastructure Layer (`NetPcContacts.Infrastructure`)
**Purpose**: Implements data access and external concerns

**Database Context**:
- `NetPcContactsDbContext` - Inherits `IdentityDbContext<User>`
- Configures:
  - Entity relationships
  - Field constraints (max length, required, unique)
  - Cascade delete behavior (SetNull)
  - Indexes (unique email index)

**Repositories** (`Repositories/`):
- `ContactsRepository` - Implements `IContactsRepository`
  - Methods: GetAll, GetById, Create, Update, Delete, EmailExists
- `CategoryRepository` - Implements `ICategoryRepository`
  - Methods: GetAll, GetById, Exists
- `SubcategoryRepository` - Implements `ISubcategoryRepository`
  - Methods: GetByCategoryId, Exists, ExistsForCategory

**Database**:
- SQLite (`NetPcContacts.sqlite`)
- Connection string: `Data Source=NetPcContacts.sqlite`

**Seeders** (`Seeders/`):
- `ApplicationSeeder` - Seeds initial data (categories, subcategories, sample contacts)

**Migrations** (`Migrations/`):
- EF Core migrations for database schema

### 4. Presentation Layer (`NetPcContacts.Api`)
**Purpose**: Exposes REST API endpoints and hosts Angular UI

**Controllers**:
- `ContactController` - Contact management endpoints
  - GET `/api/contact` - Get all contacts (public, rate-limited: 100/min)
  - GET `/api/contact/{id}` - Get contact by ID (public)
  - POST `/api/contact` - Create contact (protected, rate-limited: 30/min)
  - PATCH `/api/contact/{id}` - Update contact (protected, rate-limited: 30/min)
  - DELETE `/api/contact/{id}` - Delete contact (protected, rate-limited: 30/min)

**Middlewares**:
- `ErrorHandlingMiddleware` - Global exception handling
  - Catches all exceptions
  - Returns appropriate HTTP status codes
  - Logs errors

**Configuration**:
- Swagger/OpenAPI with XML documentation
- CORS (allows Angular UI origin)
- Rate limiting with multiple policies:
  - **Global**: 100 requests/minute (fixed window)
  - **auth**: 10 requests/minute (Identity endpoints)
  - **commands**: 30 requests/minute (POST/PUT/DELETE, sliding window)
  - **queries**: 100 tokens/minute (GET, token bucket)
- Bearer token authentication

### 5. Frontend (`NetPcContacts.Api/NetPcContacts.UI/`)
**Purpose**: Angular 20 standalone application with zoneless change detection

**Architecture**:
- **Standalone Components** (no NgModules)
- **Zoneless Change Detection** (signals-based)
- **Modern Control Flow** (`@if`, `@for`, `@switch`)
- **Functional Interceptors** (JWT, error handling)

**Components** (`src/app/components/`):
- `LoginComponent` - User authentication
- `ContactListComponent` - Contact list with pagination and search
- `ContactDetailsDialog` - View contact details (read-only)
- `ContactDialog` - Create/edit contact (dual mode)
- `ConfirmDialog` - Confirmation dialog for delete operations

**Services** (`src/app/services/`):
- `AuthService` - Authentication management
  - Methods: login, logout, refreshToken
  - Uses signals for reactive state
  - Syncs with localStorage and cookies
- `ContactService` - Contact CRUD operations
  - Methods: getContacts, getContactById, createContact, updateContact, deleteContact

**Interceptors** (`src/app/interceptors/`):
- `jwtInterceptor` - Attaches bearer token to requests
- `errorInterceptor` - Handles HTTP errors, attempts token refresh on 401

**Models** (`src/app/models/`):
- `Contact` - Full contact model
- `BasicContact` - Basic contact model for list view
- `LoginRequest` - Login credentials
- `LoginResponse` - Login response with token

## Used Libraries and Technologies

### Backend (.NET)

**Core Framework**:
- .NET 8.0 Web API
- ASP.NET Core 8.0

**Database**:
- Entity Framework Core 8.0
- SQLite (Microsoft.EntityFrameworkCore.Sqlite)

**Authentication**:
- ASP.NET Core Identity 8.0
- Bearer token authentication

**CQRS & Validation**:
- MediatR 12.5.0 - CQRS implementation
- FluentValidation.AspNetCore 11.3.1 - Input validation with ASP.NET Core integration

**API Documentation**:
- Swashbuckle.AspNetCore 6.6.2 - Swagger/OpenAPI

**Rate Limiting**:
- ASP.NET Core built-in rate limiting (System.Threading.RateLimiting)

**Testing**:
- xUnit 2.5.3 - Test framework
- xunit.runner.visualstudio 2.5.3 - Visual Studio test runner
- Moq 4.20.72 - Mocking library
- FluentAssertions 8.7.1 - Assertion library
- Microsoft.AspNetCore.Mvc.Testing 8.0.0 - Integration tests
- Microsoft.EntityFrameworkCore.InMemory 9.0.9 - In-memory database for testing
- Microsoft.NET.Test.Sdk 17.8.0 - Test SDK
- coverlet.collector 6.0.4 - Code coverage

### Frontend (Angular)

**Core Framework**:
- Angular 20.3.0 (core, common, compiler, forms, platform-browser, router)
- Angular CDK 20.2.8 - Component Dev Kit
- TypeScript 5.9.2
- RxJS 7.8.0 - Reactive programming
- tslib 2.3.0 - TypeScript runtime library

**UI Framework**:
- Angular Material 20.2.8 - Component library
- Tailwind CSS 3.4.18 - Utility-first CSS

**Build Tools**:
- Angular CLI 20.3.5
- Angular Build 20.3.5
- Webpack (via Angular CLI)
- PostCSS 8.5.6 - CSS processing
- Autoprefixer 10.4.21 - CSS vendor prefixing

**Testing**:
- Jasmine Core 5.9.0 - Test framework
- @types/jasmine 5.1.0 - TypeScript definitions for Jasmine
- Karma 6.4.0 - Test runner
- karma-chrome-launcher 3.2.0 - Chrome launcher for Karma
- karma-coverage 2.2.0 - Code coverage
- karma-jasmine 5.1.0 - Jasmine adapter for Karma
- karma-jasmine-html-reporter 2.1.0 - HTML reporter for Karma

## Compilation and Deployment

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+ (for Angular)
- npm or yarn

### Backend Compilation

#### Build Solution
```bash
# Navigate to solution root

# Restore NuGet packages
dotnet restore

# Build entire solution (Development)
dotnet build

# Build in Debug mode (default)
dotnet build --configuration Debug
```

#### Run Application
```bash
# Run API project (Development mode)
dotnet run --project NetPcContacts.Api

# Run with watch mode (auto-reload on file changes)
dotnet watch --project NetPcContacts.Api

# Explicitly specify Development environment
dotnet run --project NetPcContacts.Api --launch-profile "https"
```

**Note**: Always run in Development mode as production environment variables are not configured.

The API will be available at:
- HTTPS: `https://localhost:7076`
- HTTP: `http://localhost:5259`

#### Database Setup
```bash
# Create/update database
dotnet ef database update --project NetPcContacts.Infrastructure --startup-project NetPcContacts.Api

# Add new migration
dotnet ef migrations add MigrationName --project NetPcContacts.Infrastructure --startup-project NetPcContacts.Api

# Drop database (if needed)
dotnet ef database drop --project NetPcContacts.Infrastructure --startup-project NetPcContacts.Api
```

#### Run Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test NetPcContacts.Api.Tests
dotnet test NetPcContacts.Application.Tests
dotnet test NetPcContacts.Infrastructure.Tests

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

#### Publish Application

**Note**: Production deployment requires additional configuration (environment variables, connection strings, etc.) which are not included in this repository.

For development deployment:
```bash
# Publish for development deployment
dotnet publish NetPcContacts.Api --configuration Debug --output ./publish

# Or publish as self-contained (includes runtime)
dotnet publish NetPcContacts.Api --configuration Debug --self-contained true --runtime win-x64 --output ./publish
```

### Frontend Compilation

#### Setup
```bash
# Navigate to Angular project
cd NetPcContacts.Api/NetPcContacts.UI

# Install dependencies (first time only)
npm install
```

#### Development
```bash
# Start development server (http://localhost:4200)
npm start
# or
ng serve

# Start with specific port
ng serve --port 4200

# Watch mode (rebuild on changes)
npm run watch
```

#### Build
```bash
# Build for development
npm run build
# or
ng build

# Build with development configuration (default)
ng build --configuration development

# Output directory: dist/net-pc-contacts.ui/browser/
```

**Note**: Production build configuration requires additional environment settings not included in this repository.

#### Test
```bash
# Run unit tests
npm test
# or
ng test

# Run tests once (for CI)
ng test --watch=false --browsers=ChromeHeadless

# Run with code coverage
ng test --code-coverage
```

### Full Application Deployment

**⚠️ Important**: Production deployment is not configured. The application is designed to run in Development mode only.

For development environment deployment:

1. **Build Backend**:
   ```bash
   dotnet publish NetPcContacts.Api --configuration Debug --output ./publish
   ```

2. **Build Frontend**:
   ```bash
   cd NetPcContacts.Api/NetPcContacts.UI
   npm run build
   ```

3. **Deploy to Development Server**:
   - Copy published backend files to server
   - Copy `dist/net-pc-contacts.ui/browser/` to server's wwwroot or static file directory
   - Configure web server (IIS, Nginx, Apache) to serve:
     - API endpoints: `https://yourdomain.com/api/`
     - Static files: `https://yourdomain.com/`
   - Set up SSL certificate
   - Ensure `appsettings.Development.json` is present with proper configuration
   - Set environment variable: `ASPNETCORE_ENVIRONMENT=Development`

**For Production Deployment** (requires additional setup):
- Create `appsettings.Production.json` with production settings
- Configure production database connection string
- Set up production authentication keys
- Configure production CORS origins
- Set environment variable: `ASPNETCORE_ENVIRONMENT=Production`

## Validation Rules

### Password Complexity Requirements
- Minimum 8 characters
- Maximum 100 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one digit (0-9)
- At least one special character (!@#$%^&* etc.)

### Field Length Constraints

| Field | Minimum | Maximum | Notes |
|-------|---------|---------|-------|
| Name | 1 | 100 | Required |
| Surname | 1 | 100 | Required |
| Email | - | 255 | Required, unique, valid format |
| Password | 8 | 100 | Required on create, optional on update |
| Phone Number | 9 | 20 | Required, specific format |
| Custom Subcategory | - | 100 | Optional |
| Category Name | - | 50 | Dictionary value |
| Subcategory Name | - | 100 | Dictionary value |

### Business Rules
- Email must be unique across all contacts
- Password is hashed using PBKDF2 via ASP.NET Core Identity
- Category is required
- Subcategory is required for "Business" category
- Subcategory must be null for "Private" category
- Custom subcategory is used only when category is "Other"
- Birth date must be in the past, not older than 150 years

### Pagination & Sorting
- **Page sizes**: 5, 10, 15, 30
- **Sort by**: FirstName, LastName, Category
- **Sort direction**: Ascending, Descending
- **Search**: Filters by Name, Surname, Email

## Security Features

1. **Authentication**: ASP.NET Core Identity with bearer tokens
2. **Authorization**: Protected endpoints require authentication
3. **Password hashing**: PBKDF2 algorithm via Identity
4. **Rate limiting**: Multiple policies to prevent abuse
5. **CORS**: Configured for specific origins
6. **Validation**: Input validation on all endpoints
7. **Error handling**: Custom middleware prevents information leakage
8. **HTTPS**: Enforced in production

## API Endpoints

### Contact Management

| Method | Endpoint | Auth | Rate Limit | Description |
|--------|----------|------|------------|-------------|
| GET | `/api/contact` | No | 100/min | Get paginated contacts |
| GET | `/api/contact/{id}` | No | 100/min | Get contact by ID |
| POST | `/api/contact` | Yes | 30/min | Create new contact |
| PATCH | `/api/contact/{id}` | Yes | 30/min | Update contact |
| DELETE | `/api/contact/{id}` | Yes | 30/min | Delete contact |

### Identity

| Method | Endpoint | Auth | Rate Limit | Description |
|--------|----------|------|------------|-------------|
| POST | `/api/identity/register` | No | 10/min | Register new user |
| POST | `/api/identity/login` | No | 10/min | Login user |

## Project Structure

```
NetPcContacts/
├── NetPcContacts.Domain/             # Domain entities and interfaces
│   ├── Entities/                     # Contact, Category, Subcategory, User
│   ├── IRepositories/                # Repository interfaces
│   ├── Exceptions/                   # Domain exceptions
│   └── Constants/                    # Domain constants
├── NetPcContacts.Application/        # Application logic (CQRS)
│   ├── Contacts/
│   │   ├── Commands/                 # Create, Update, Delete
│   │   ├── Queries/                  # GetAll, GetById
│   │   └── Validators/               # FluentValidation validators
│   └── DTOs/                         # Data transfer objects
├── NetPcContacts.Infrastructure/     # Data access and persistence
│   ├── Persistence/                  # DbContext
│   ├── Repositories/                 # Repository implementations
│   ├── Migrations/                   # EF Core migrations
│   └── Seeders/                      # Data seeders
├── NetPcContacts.Api/                # API presentation layer
│   ├── Controllers/                  # API controllers
│   ├── Middlewares/                  # Custom middlewares
│   ├── Extensions/                   # Service registration extensions
│   └── NetPcContacts.UI/             # Angular frontend
│       ├── src/
│       │   ├── app/
│       │   │   ├── components/       # Angular components
│       │   │   ├── services/         # Angular services
│       │   │   ├── interceptors/     # HTTP interceptors
│       │   │   └── models/           # TypeScript models
│       │   ├── environments/         # Environment configs
│       │   └── styles.scss           # Global styles
│       └── package.json              # npm dependencies
└── Tests/
    ├── NetPcContacts.Api.Tests/      # API integration tests
    ├── NetPcContacts.Application.Tests/ # Application unit tests
    └── NetPcContacts.Infrastructure.Tests/ # Infrastructure unit tests
```

## Configuration

### Backend Configuration (`appsettings.json`)
- Database connection string
- JWT token settings (expiration, issuer, audience)
- Rate limiting policies
- CORS origins
- Logging configuration

### Frontend Configuration (`src/environments/`)
- `environment.ts` - Development settings
- `environment.prod.ts` - Production settings
- API base URL: `https://localhost:7076`

## Additional Information

### Code Quality
- XML documentation for all public APIs
- Comprehensive unit and integration tests
- Code coverage reporting
- FluentAssertions for readable test assertions

### Performance
- Rate limiting to prevent abuse
- Pagination for large datasets
- Efficient database queries with EF Core
- Angular zoneless change detection for better performance

### Maintainability
- Clean Architecture for separation of concerns
- CQRS pattern for clear command/query separation
- Dependency injection throughout
- Repository pattern for data access abstraction
- Feature-based folder structure

## Author & License

This project was created as a technical exercise demonstrating modern .NET and Angular best practices.

For more information, see the CLAUDE.md file in the project root.
