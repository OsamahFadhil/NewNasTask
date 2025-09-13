# Insight Invoicing - Residential Complex Installment Management System

A complete .NET Core 8 solution implementing Clean Architecture and Domain-Driven Design (DDD) principles for managing residential complex contracts, installments, and payment tracking with full RBAC (Role-Based Access Control).

## 🏗️ Architecture Overview

The solution follows Clean Architecture principles with the following layers:

- **Domain Layer** (`Insight.Invoicing.Domain`): Core business logic, entities, value objects, domain events, and repository interfaces
- **Application Layer** (`Insight.Invoicing.Application`): CQRS implementation with MediatR, DTOs, specifications, and pipeline behaviors
- **Infrastructure Layer** (`Insight.Invoicing.Infrastructure`): Data persistence with EF Core, external services (MinIO, email), and background services
- **API Layer** (`Insight.Invoicing.API`): ASP.NET Core Web API with controllers, authentication, and middleware
- **Shared Kernel** (`Insight.Invoicing.Shared`): Common utilities and base classes

## 🌟 Key Features

### Core Business Features

- **Contract Management**: Complete lifecycle from draft to approval/rejection
- **Installment Calculation**: Automated calculation with grace periods and penalty handling
- **Payment Tracking**: Upload and validate payment receipts with file storage
- **User Management**: Tenant and Administrator roles with permission-based authorization

### Technical Features

- **Clean Architecture**: Separation of concerns with dependency inversion
- **Domain-Driven Design**: Rich domain models with business rules enforcement
- **CQRS Pattern**: Command and Query segregation using MediatR
- **Event-Driven Architecture**: Domain events for cross-cutting concerns
- **Background Services**: Automated overdue checking and penalty calculation
- **File Storage**: MinIO integration for payment receipt storage
- **Comprehensive Logging**: Structured logging with Serilog
- **Global Error Handling**: Structured error responses with ProblemDetails
- **API Documentation**: Swagger/OpenAPI with JWT authentication

## 🛠️ Technology Stack

- **.NET Core 8.0**
- **Entity Framework Core 8.0** (SQL Server)
- **ASP.NET Core Identity** with JWT authentication
- **MediatR** for CQRS implementation
- **FluentValidation** for input validation
- **MinIO** for file storage
- **Quartz.NET** for background jobs
- **Serilog** for structured logging
- **Swagger/OpenAPI** for API documentation

## 🚀 Getting Started

### Prerequisites

1. **.NET 8.0 SDK** or later
2. **SQL Server** (LocalDB for development)
3. **MinIO Server** (for file storage)
4. **Visual Studio 2022** or **VS Code** (recommended)

### Setup Instructions

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd insight-invoicing
   ```

2. **Configure Database Connection**

   - Update the connection string in `appsettings.json` if needed
   - The default uses SQL Server LocalDB

3. **Setup MinIO (for file storage)**

   ```bash
   # Using Docker
   docker run -p 9000:9000 -p 9001:9001 \
     -e "MINIO_ROOT_USER=minioadmin" \
     -e "MINIO_ROOT_PASSWORD=minioadmin" \
     quay.io/minio/minio server /data --console-address ":9001"
   ```

4. **Restore NuGet packages**

   ```bash
   dotnet restore
   ```

5. **Run the application**

   ```bash
   cd src/Insight.Invoicing.API
   dotnet run
   ```

6. **Access the API**
   - API: `https://localhost:5001` or `http://localhost:5000`
   - Swagger UI: `https://localhost:5001` (served at root)

### Default Login Credentials

The system creates a default administrator account on startup:

- **Email**: `admin@insight-invoicing.com`
- **Password**: `Admin@123`

## 📊 Domain Model

### Core Aggregates

#### Contract Aggregate

- **Contract** (Root): Lease agreement with tenant
- **Installment**: Payment schedule for the contract

#### PaymentReceipt Aggregate

- **PaymentReceipt** (Root): Uploaded payment proof with validation workflow

#### User Aggregate

- **User** (Root): System users (Tenants and Administrators)

### Key Business Rules

1. **Contract Lifecycle**: Draft → Submitted → Approved/Rejected → Closed
2. **Installment Calculation**: Automatic generation with banker's rounding
3. **Grace Periods**: 5-day grace period before penalties apply
4. **Penalty Calculation**: 2% per month on outstanding amounts
5. **Payment Validation**: Administrator approval required for payment receipts

## 🔐 Security & Authorization

### Authentication

- JWT-based authentication with refresh token support
- Password requirements: 8+ characters, uppercase, lowercase, digit
- Account lockout after 5 failed attempts

### Authorization

- Role-based access control (Tenant, Administrator)
- Permission-based authorization for fine-grained access control
- Protected endpoints with appropriate role restrictions

### Permissions

- **Contracts**: View, Create, Edit, Approve, Reject, Delete
- **Installments**: View, Edit
- **Payment Receipts**: View, Upload, Validate
- **Users**: View, Create, Edit, Delete
- **Reports**: View, Generate
- **Admin**: Dashboard, Settings

## 📁 Project Structure

```
src/
├── Insight.Invoicing.Domain/           # Domain layer
│   ├── Entities/                       # Domain entities and aggregates
│   ├── ValueObjects/                   # Value objects
│   ├── Events/                         # Domain events
│   ├── Enums/                          # Domain enumerations
│   ├── Exceptions/                     # Domain exceptions
│   ├── Repositories/                   # Repository interfaces
│   └── Services/                       # Domain service interfaces
├── Insight.Invoicing.Application/      # Application layer
│   ├── Commands/                       # CQRS commands
│   ├── Queries/                        # CQRS queries
│   ├── Handlers/                       # Command and query handlers
│   ├── DTOs/                           # Data transfer objects
│   ├── Specifications/                 # Specification pattern
│   ├── Behaviors/                      # MediatR pipeline behaviors
│   └── Validators/                     # FluentValidation validators
├── Insight.Invoicing.Infrastructure/   # Infrastructure layer
│   ├── Persistence/                    # EF Core configurations and repositories
│   ├── Services/                       # External service implementations
│   ├── EventHandlers/                  # Domain event handlers
│   └── BackgroundServices/             # Background tasks
├── Insight.Invoicing.API/              # Presentation layer
│   ├── Controllers/                    # API controllers
│   ├── Middleware/                     # Custom middleware
│   ├── Authorization/                  # Authorization policies
│   └── Program.cs                      # Application startup
└── Insight.Invoicing.Shared/           # Shared kernel
    └── Common/                         # Base classes and interfaces
```

## 🔄 API Endpoints

### Authentication

- `POST /api/auth/login` - User authentication
- `POST /api/auth/refresh` - Refresh access token

### Contracts (Tenants)

- `GET /api/contracts` - Get user's contracts
- `POST /api/contracts` - Create new contract
- `POST /api/contracts/{id}/submit` - Submit contract for approval

### Contracts (Administrators)

- `GET /api/contracts/admin/pending` - Get pending contracts
- `POST /api/contracts/admin/{id}/approve` - Approve contract
- `POST /api/contracts/admin/{id}/reject` - Reject contract

### Installments

- `GET /api/installments/contracts/{contractId}` - Get contract installments
- `GET /api/installments/overdue` - Get overdue installments (Admin)

### Payment Receipts

- `POST /api/paymentreceipts/upload` - Upload payment receipt (Tenant)
- `POST /api/paymentreceipts/{id}/validate` - Validate receipt (Admin)
- `GET /api/paymentreceipts/pending` - Get pending receipts (Admin)

### Reports (Administrators)

- `GET /api/admin/dashboard` - Dashboard statistics
- `GET /api/admin/reports/overdue` - Overdue installments report
- `GET /api/admin/reports/revenue` - Revenue report

## 🧪 Testing

The solution includes comprehensive test coverage:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📈 Performance Considerations

- **Database Indexing**: Strategic indexes on frequently queried fields
- **Pagination**: All list endpoints support pagination
- **Caching**: Redis integration ready for caching strategies
- **Background Processing**: Async processing for heavy operations
- **File Storage**: MinIO for scalable file storage

## 🚀 Deployment

### Docker Support

```dockerfile
# Example Dockerfile for API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Insight.Invoicing.API/Insight.Invoicing.API.csproj", "src/Insight.Invoicing.API/"]
RUN dotnet restore "src/Insight.Invoicing.API/Insight.Invoicing.API.csproj"
COPY . .
WORKDIR "/src/src/Insight.Invoicing.API"
RUN dotnet build "Insight.Invoicing.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Insight.Invoicing.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Insight.Invoicing.API.dll"]
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ConnectionStrings__DefaultConnection`: Database connection
- `JwtSettings__SecretKey`: JWT signing key
- `MinIO__Endpoint`: MinIO server endpoint

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions:

- Create an issue in the repository
- Email: support@insight-invoicing.com

## 🎯 Future Enhancements

- [ ] Mobile application support
- [ ] Advanced reporting with charts
- [ ] Email and SMS notifications
- [ ] Multi-tenant support
- [ ] Integration with payment gateways
- [ ] Document generation (contracts, invoices)
- [ ] Audit logging and compliance features

