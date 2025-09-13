# Insight Invoicing System

A comprehensive invoicing and contract management system built with .NET 9, designed for managing apartment rental contracts with installment-based payments.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with the following layers:

- **Domain Layer** (`Insight.Invoicing.Domain`) - Core business entities and rules
- **Application Layer** (`Insight.Invoicing.Application`) - Use cases, commands, queries, and DTOs
- **Infrastructure Layer** (`Insight.Invoicing.Infrastructure`) - Data access, external services, and implementations
- **API Layer** (`Insight.Invoicing.API`) - Web API controllers and middleware
- **Shared Layer** (`Insight.Invoicing.Shared`) - Common utilities and shared components

## 🚀 Features

### Core Functionality

- **Contract Management**: Create, submit, approve, reject, and manage rental contracts
- **Installment System**: Automated installment calculation and tracking
- **Payment Receipts**: Upload and manage payment receipts with file storage
- **User Management**: Multi-role system (Tenants, Administrators)
- **Real-time Notifications**: SignalR-based notifications for contract status changes

### Technical Features

- **JWT Authentication**: Secure token-based authentication
- **CQRS Pattern**: Command Query Responsibility Segregation with MediatR
- **Event-Driven Architecture**: Domain events and integration events
- **Background Jobs**: Hangfire integration for scheduled tasks
- **Message Bus**: MassTransit with RabbitMQ support
- **File Storage**: MinIO integration for document storage
- **Caching**: Redis support for performance optimization
- **API Documentation**: Swagger/OpenAPI integration

## 🛠️ Technology Stack

- **.NET 9** - Latest .NET framework
- **PostgreSQL** - Primary database
- **Entity Framework Core** - ORM
- **MediatR** - CQRS implementation
- **SignalR** - Real-time communication
- **JWT Bearer** - Authentication
- **Hangfire** - Background job processing
- **MassTransit** - Message bus
- **MinIO** - Object storage
- **Redis** - Caching and SignalR backplane
- **RabbitMQ** - Message broker
- **Docker** - Containerization

## 📋 Prerequisites

- .NET 9 SDK
- Docker and Docker Compose
- PostgreSQL (or use Docker)
- Visual Studio 2022 or VS Code

## 🚀 Quick Start

### Using Docker Compose (Recommended)

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd NewNasTask
   ```

2. **Start all services**

   ```bash
   docker-compose up -d
   ```

   This will start:

   - PostgreSQL database (port 5433)
   - MinIO object storage (ports 9000, 9002)
   - Redis cache (port 6379)
   - RabbitMQ message broker (ports 5672, 15672)
   - pgAdmin (port 5050)
   - API application (port 8080)

3. **Access the application**
   - API: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger
   - MinIO Console: http://localhost:9002
   - pgAdmin: http://localhost:5050
   - RabbitMQ Management: http://localhost:15672

### Manual Setup

1. **Install dependencies**

   ```bash
   dotnet restore
   ```

2. **Configure database**

   - Update connection strings in `appsettings.json`
   - Run migrations:

   ```bash
   dotnet ef database update --project src/Insight.Invoicing.Infrastructure --startup-project src/Insight.Invoicing.API
   ```

3. **Run the application**
   ```bash
   dotnet run --project src/Insight.Invoicing.API
   ```

## 🔧 Configuration

### Environment Variables

| Variable                               | Description                     | Default                                                                                        |
| -------------------------------------- | ------------------------------- | ---------------------------------------------------------------------------------------------- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string    | `Host=localhost;Port=5433;Database=InsightInvoicingDb;Username=postgres;Password=postgres123;` |
| `JwtSettings__SecretKey`               | JWT signing key                 | Required                                                                                       |
| `MinIO__Endpoint`                      | MinIO server endpoint           | `localhost:9000`                                                                               |
| `UseRedis`                             | Enable Redis caching            | `false`                                                                                        |
| `UseRabbitMQ`                          | Enable RabbitMQ message bus     | `false`                                                                                        |
| `UseHangfire`                          | Enable Hangfire background jobs | `false`                                                                                        |

### Service Configuration

The application supports optional services that can be enabled via configuration:

- **Redis**: Set `UseRedis: true` for caching and SignalR backplane
- **RabbitMQ**: Set `UseRabbitMQ: true` for message bus functionality
- **Hangfire**: Set `UseHangfire: true` for background job processing

## 📚 API Endpoints

### Authentication

- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/change-password` - Change password

### Contracts

- `GET /api/contracts/{id}` - Get contract by ID
- `GET /api/contracts` - Get user contracts (Tenant)
- `POST /api/contracts` - Create new contract (Tenant)
- `POST /api/contracts/{id}/submit` - Submit contract for approval (Tenant)
- `GET /api/contracts/admin/pending` - Get pending contracts (Admin)
- `POST /api/contracts/admin/{id}/approve` - Approve contract (Admin)
- `POST /api/contracts/admin/{id}/reject` - Reject contract (Admin)

### Payment Receipts

- `POST /api/payment-receipts/upload` - Upload payment receipt
- `GET /api/payment-receipts` - Get payment receipts

### Installments

- `GET /api/installments` - Get installments
- `POST /api/installments/{id}/mark-paid` - Mark installment as paid

## 🏢 Domain Model

### Core Entities

- **User**: System users (Tenants, Administrators)
- **Contract**: Rental contracts with installment details
- **Installment**: Individual payment installments
- **PaymentReceipt**: Payment documentation and receipts
- **Notification**: System notifications

### Value Objects

- **Money**: Currency and amount handling
- **Address**: User address information
- **Email**: Email validation and handling

## 🔄 Event-Driven Architecture

The system uses domain events and integration events for loose coupling:

### Domain Events

- `ContractSubmittedEvent`
- `ContractApprovedEvent`
- `ContractRejectedEvent`
- `PaymentReceiptUploadedEvent`

### Integration Events

- Contract status changes
- Payment notifications
- System alerts

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test src/Insight.Invoicing.Tests
```

## 📦 Deployment

### Docker Production

1. **Build the image**

   ```bash
   docker build -t insight-invoicing .
   ```

2. **Run with production settings**
   ```bash
   docker run -p 8080:8080 \
     -e ConnectionStrings__DefaultConnection="your-connection-string" \
     -e JwtSettings__SecretKey="your-secret-key" \
     insight-invoicing
   ```

### Environment-Specific Configuration

- **Development**: Uses mock services, in-memory message bus
- **Production**: Configure real services (SendGrid, Twilio, Stripe)

## 🔒 Security

- JWT-based authentication
- Role-based authorization
- Password hashing with ASP.NET Core Identity
- CORS configuration for frontend integration
- Input validation and sanitization

## 📊 Monitoring and Logging

- **Serilog** for structured logging
- **Hangfire Dashboard** for background job monitoring
- **Health checks** for service monitoring
- **Exception handling middleware** for error management

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:

- Create an issue in the repository
- Contact the development team
- Check the documentation in the `/docs` folder

## 🔄 Version History

- **v1.0.0** - Initial release with core contract management features
- **v1.1.0** - Added payment receipt management
- **v1.2.0** - Implemented real-time notifications
- **v1.3.0** - Added background job processing

---

**Built with ❤️ using .NET 9 and Clean Architecture principles**
