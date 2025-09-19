# Typesetting MIS - Management Information System
## SaaS Platform for Typesetting Companies

A comprehensive, multi-tenant SaaS platform designed specifically for typesetting companies to manage their operations, from quote generation to order fulfillment and equipment management.

## 🚀 Project Overview

The Typesetting MIS is a scalable, cloud-native platform that serves 100-200 typesetting companies with up to 50 users each, handling 10,000+ quotes and orders per company annually. The platform provides complete customization for equipment, services, pricing policies, and user roles.

## 📋 Key Features

### Core Functionality
- **Multi-tenant Architecture** - Complete data isolation per company
- **Equipment Management** - Customizable equipment catalogs and capabilities
- **Service & Product Management** - Flexible catalog management with pricing
- **Quote Generation** - Dynamic pricing with approval workflows
- **Order Management** - End-to-end order processing and tracking
- **Inventory Management** - Materials, supplies, and resource tracking
- **Financial Management** - Invoicing, payments, and reporting
- **Analytics & Reporting** - Business intelligence and insights
- **Role-based Access Control** - Granular permissions per user role

### Technical Highlights
- **Scalable Architecture** - Modular .NET services with Kubernetes orchestration
- **Multi-tenant Security** - Database-per-tenant with complete isolation
- **Modern Tech Stack** - React 18, .NET 9, TypeScript, PostgreSQL
- **Cloud-native** - AWS infrastructure with auto-scaling
- **Compliance Ready** - SOC 2, GDPR, and security best practices

## 🏗️ Architecture

### System Architecture
```
┌─────────────────────────────────────────────────────────────────┐
│                        Client Layer                             │
├─────────────────────────────────────────────────────────────────┤
│  Web App (React)  │  Mobile App  │  Admin Dashboard  │  APIs   │
└─────────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────────┐
│                      API Gateway Layer                         │
├─────────────────────────────────────────────────────────────────┤
│  Load Balancer  │  API Gateway  │  Rate Limiting  │  Auth     │
└─────────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────────┐
│                    .NET Services Layer                        │
├─────────────────────────────────────────────────────────────────┤
│ Auth Service │ Quote Service │ Order Service │ Inventory Service │
│ User Service │ Equipment Svc │ Pricing Svc   │ Reporting Svc    │
└─────────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────────┐
│                      Data Layer                                │
├─────────────────────────────────────────────────────────────────┤
│ PostgreSQL (Primary) │ Redis (Cache) │ S3 (Files) │ ElasticSearch │
└─────────────────────────────────────────────────────────────────┘
```

### Technology Stack

#### Backend
- **Runtime**: .NET 9
- **Framework**: ASP.NET Core
- **Database**: PostgreSQL 14+ with connection pooling
- **Cache**: Redis 6+ for session and data caching
- **Message Queue**: Redis Bull for job processing
- **ORM**: EF Core

#### Frontend
- **Framework**: React 18 with TypeScript
- **State Management**: Redux Toolkit with RTK Query
- **UI Library**: Material-UI (MUI) v5
- **Routing**: React Router v6
- **Forms**: React Hook Form with Yup validation
- **Charts**: Recharts for data visualization
- **Build Tool**: Vite for fast development and building

#### Infrastructure
- **Containerization**: Podman (local dev) / Docker (CI/CD) with Kubernetes
- **Cloud Provider**: AWS (EC2, RDS, S3, CloudFront)
- **CDN**: CloudFront for static asset delivery
- **CI/CD**: GitHub Actions with automated testing and deployment

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- Podman (with podman-compose)
- Node.js 18+ (for frontend development)
- AWS CLI (for deployment)

> **Note**: This project uses Podman for local development to avoid Docker's commercial licensing restrictions. CI/CD pipelines typically use Docker for building and pushing images to registries. Both tools are compatible and use the same container images.

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/typesetting-mis.git
   cd typesetting-mis
   ```

2. **Install dependencies**
   ```bash
   # Install frontend dependencies
   cd frontend && npm install
   
   # Install backend dependencies (restore NuGet packages)
   cd ../backend && dotnet restore
   ```

3. **Set up environment variables**
   ```bash
   # Copy environment files
   cp .env.example .env
   cp backend/appsettings.Development.json.example backend/appsettings.Development.json
   ```

4. **Start the development environment**
   ```bash
   # Start shared services (PostgreSQL, Redis)
   podman-compose -f podman-compose.yml up -d postgres redis
   
   # Start backend services
   cd backend\TypesettingMIS.API; dotnet run
   
   # Start frontend (in separate terminal)
   cd frontend; npm run dev
   ```

5. **Access the application**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger

### Podman Commands Reference

```bash
# Start services
podman-compose -f podman-compose.yml up -d

# Stop services
podman-compose -f podman-compose.yml down

# View logs
podman-compose -f podman-compose.yml logs -f

# Rebuild and restart services
podman-compose -f podman-compose.yml up -d --build

# Remove all containers and volumes
podman-compose -f podman-compose.yml down -v

# Check running containers
podman ps

# Access container shell
podman exec -it typesetting-mis-postgres psql -U admin -d typesetting_mis_shared
```

### Podman Troubleshooting

If you encounter issues with Podman, here are some common solutions:

```bash
# Enable Podman socket (if needed)
systemctl --user enable --now podman.socket

# Check if podman-compose is installed
podman-compose --version

# If podman-compose is not available, install it:
# On Ubuntu/Debian: sudo apt install podman-compose
# On RHEL/CentOS: sudo dnf install podman-compose
# On macOS: brew install podman-compose

# Reset Podman (if containers are stuck)
podman system reset

# Check Podman logs
journalctl --user -u podman
```

### Database Setup

1. **Create databases**
   ```sql
   -- Shared database for system configuration
   CREATE DATABASE typesetting_mis_shared;
   
   -- Example tenant database
   CREATE DATABASE typesetting_mis_tenant_example;
   ```

2. **Run migrations**
   ```bash
   # Run database migrations
   cd backend; dotnet ef database update
   ```

3. **Start the application**
   ```bash
   # Data is automatically seeded on startup
   dotnet run
   ```

⚠️ **Default Admin Credentials (LOCAL DEVELOPMENT ONLY):**
Do not enable startup seeding or these credentials in staging/production. Gate seeding by environment or an explicit flag.
  - **Email**: `admin@testcompany.com`
   - **Password**: `Admin123!`
   - **Role**: Admin

## 🔧 Configuration

### Environment Variables

#### Shared Services
```env
# Database
DATABASE_URL=postgresql://user:password@localhost:5432/typesetting_mis_shared
REDIS_URL=redis://localhost:6379

# Authentication
JWT_SECRET=your-jwt-secret
JWT_EXPIRES_IN=24h
REFRESH_TOKEN_SECRET=your-refresh-token-secret

# AWS
AWS_REGION=us-east-1
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
S3_BUCKET=typesetting-mis-files

# Monitoring
SENTRY_DSN=your-sentry-dsn
DATADOG_API_KEY=your-datadog-key
```

#### Service-specific Variables
Each service has its own `.env` file with service-specific configurations.

### Multi-tenant Configuration

The platform supports multiple tenant configuration strategies:

1. **Database per Tenant** (Recommended)
   - Complete data isolation
   - Tenant-specific optimizations
   - Easier compliance and backup

2. **Shared Database with Tenant ID**
   - Lower operational overhead
   - Easier management
   - Row-level security

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test suites
dotnet test --filter Category=Unit          # Unit tests
dotnet test --filter Category=Integration   # Integration tests
dotnet test --filter Category=E2E          # End-to-end tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure

```
tests/
├── Unit/                 # Unit tests
├── Integration/          # Integration tests
├── E2E/                 # End-to-end tests
└── TestData/            # Test data and fixtures
```

## 🚀 Deployment

### Production Deployment

1. **Build container images**
   ```bash
   # Local development (using Podman)
   podman build -t typesetting-mis-backend ./backend
   podman build -t typesetting-mis-frontend ./frontend
   
   # CI/CD pipeline (using Docker)
   docker build -t typesetting-mis-backend ./backend
   docker build -t typesetting-mis-frontend ./frontend
   
   # Push to registry
   docker push typesetting-mis-backend
   docker push typesetting-mis-frontend
   ```

2. **Deploy to Kubernetes**
   ```bash
   # Apply Kubernetes manifests
   kubectl apply -f infrastructure/kubernetes/
   
   # Verify deployment
   kubectl get pods -n typesetting-mis
   ```
### Scaling

The platform is designed to scale horizontally:

- **Auto-scaling**: Kubernetes HPA based on CPU and memory usage
- **Database scaling**: Read replicas for read-heavy operations
- **Caching**: Multi-layer caching strategy
- **CDN**: CloudFront for global content delivery

## 🔒 Security

### Security Features

- **Authentication**: Multi-factor authentication (MFA)
- **Authorization**: Role-based access control (RBAC)
- **Encryption**: Data encrypted at rest and in transit
- **Network Security**: VPC, security groups, and WAF
- **Monitoring**: Security event monitoring and alerting
- **Compliance**: SOC 2, GDPR, and security best practices

### Security Testing

```bash
# Run security tests
dotnet test --filter Category=Security

# Run vulnerability scans
dotnet list package --vulnerable

# Run penetration tests
dotnet test --filter Category=Penetration
```
## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🗺️ Roadmap

### Phase 1: Foundation (Months 1-3)
- [x] Core platform setup
- [x] Multi-tenant architecture
- [x] Basic authentication and authorization
- [x] Database schema implementation
- [x] Basic UI framework

### Phase 2: Core Features (Months 4-6)
- [ ] Equipment management system
- [ ] Service/product catalog management
- [ ] Basic quote generation
- [ ] User role management
- [ ] Basic reporting

### Phase 3: Advanced Features (Months 7-9)
- [ ] Advanced pricing engine
- [ ] Order management workflow
- [ ] Inventory management
- [ ] Financial management
- [ ] Advanced reporting and analytics

### Phase 4: Integration & Optimization (Months 10-12)
- [ ] Third-party integrations
- [ ] Performance optimization
- [ ] Advanced security features
- [ ] Mobile responsiveness
- [ ] Production deployment

---

**Built with ❤️ for the typesetting industry**
