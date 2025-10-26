# Campaigns API

A production-ready RESTful API for managing marketing campaigns, built with .NET 8, Entity Framework Core, and Azure AD authentication.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:
```
┌─────────────────────────────────────────────────────┐
│                   API Layer                         │
│  (Controllers, Middleware, Swagger, Entry Point)    │
└─────────────────────────────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────┐
│               Application Layer                     │
│    (DTOs, Services, Validators, Interfaces)         │
└─────────────────────────────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────┐
│              Infrastructure Layer                   │
│      (EF Core, Repositories, Database Access)       │
└─────────────────────────────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────┐
│                 Domain Layer                        │
│        (Entities, Business Rules, Exceptions)       │
└─────────────────────────────────────────────────────┘
```

### Layer Responsibilities

- **Domain**: Core business entities and rules (Campaign, CampaignStatus, validation logic)
- **Application**: Use cases, DTOs, service interfaces, FluentValidation validators
- **Infrastructure**: Database access, EF Core DbContext, repository implementations
- **API**: HTTP endpoints, Swagger, authentication configuration, middleware

---

## 🚀 Features

### Core Functionality
- ✅ Full CRUD operations for campaigns
- ✅ Pagination with continuation tokens (load more pattern)
- ✅ Filtering by name (partial match) and status
- ✅ Sorting by multiple fields (name, budget, dates, created date)
- ✅ Soft delete (audit trail preservation)
- ✅ Partial updates (only update provided fields)

### Technical Features
- ✅ Azure AD authentication (multitenant + personal accounts)
- ✅ JWT Bearer token validation
- ✅ FluentValidation for input validation
- ✅ Entity Framework Core with SQL Server
- ✅ Database migrations
- ✅ Swagger/OpenAPI documentation with OAuth integration
- ✅ Structured error responses
- ✅ Comprehensive logging

---

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for SQL Server)
- [Azure Account](https://azure.microsoft.com/free/) (free tier) for authentication
- Code editor: [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

---

## 🛠️ Setup Instructions

### 1. Clone the Repository
```bash
git clone https://github.com/carvanino/infinion-campaignsApi
cd CampaignsApi
```

### 2. Start SQL Server (Docker)
```bash
# Pull and run SQL Server 2022
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong@Password123" \
  -p 1433:1433 \
  --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest

# Verify it's running
docker ps
```

### 3. Configure Database Connection
Create `appsettings.Development.json` from `appsettings.json`with your local configuration:
Update `appsettings.Development.json` with your SQL Server password:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CampaignsDb_Dev;User Id=sa;Password=YourStrong@Password123;Integrated Security=false;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true"
  }
}
```

### 4. Apply Database Migrations
```bash
# Navigate to API project
cd CampaignsApi.API

# Apply migrations to create database and tables
dotnet ef database update --project ../CampaignsApi.Infrastructure
```

### 5. Configure Azure AD Authentication

**Option A: Use Existing Configuration (Easiest)**

The app is pre-configured with a test Azure AD app registration:
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "common",
    "ClientId": "f50fb135-a878-46ae-a516-fd167f8183ec",
    "Audience": "f50fb135-a878-46ae-a516-fd167f8183ec"
  }
}
```

This configuration accepts any Microsoft work or personal account.

**Option B: Create Your Own Azure AD App (Recommended for Production)**

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** → **App registrations**
3. Click **New registration**:
   - **Name**: Campaigns API
   - **Supported account types**: Accounts in any organizational directory and personal Microsoft accounts
   - Click **Register**
4. Note your **Application (client) ID** and **Directory (tenant) ID**
5. Go to **Authentication** → **Add a platform** → **Single-page application**
6. Add redirect URI: `https://localhost:7137/swagger/oauth2-redirect.html`
7. Check **Access tokens** and **ID tokens**
8. Go to **Expose an API** → **Add a scope**:
   - Scope name: `access_as_user`
   - Consent: Admins and users
   - Save
9. Update `appsettings.json` with your Client ID

### 6. Run the Application
```bash
# Build the solution
dotnet build

# Run with HTTPS
dotnet run --launch-profile https
```

The API will start on:
- **HTTPS**: https://localhost:7137
- **HTTP**: http://localhost:5069

---

## 📖 API Documentation

### Access Swagger UI

Open your browser to: **https://localhost:7137/swagger**

### Authentication

1. Click the **"Authorize"** button (lock icon) in Swagger UI
2. The OAuth popup will appear with client_id pre-filled
3. [x] Check the box that says "**Access Campaigns API as user**"
3. Click **"Authorize"**
4. Sign in with any Microsoft account (work or personal)
5. Consent to permissions
6. You'll be redirected back to Swagger - the lock icon will be closed
7. All requests now include your JWT token automatically

### API Endpoints

#### Create Campaign
```http
POST /api/campaigns
Content-Type: application/json

{
  "name": "Summer Sale 2024",
  "description": "Campaign for summer product discounts",
  "startDate": "2024-12-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "budget": 50000
}
```

**Response: 201 Created**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Summer Sale 2024",
  "description": "Campaign for summer product discounts",
  "startDate": "2024-12-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "budget": 50000,
  "status": "Draft",
  "createdBy": "user@example.com",
  "createdAt": "2024-10-26T10:30:00Z",
  "updatedAt": "2024-10-26T10:30:00Z"
}
```

#### Get All Campaigns (Paginated)
```http
GET /api/campaigns?pageSize=20&nameFilter=summer&statusFilter=Active&sortBy=createdAt&sortDescending=true
```

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Summer Sale 2024",
      "status": "Active",
      ...
    }
  ],
  "totalCount": 100,
  "pageSize": 20,
  "continuationToken": "eyJMYXN0SWQiOiIzZmE4NWY2NC...",
  "hasMore": true
}
```

**Query Parameters:**
- `pageSize` (1-100): Number of items per page (default: 20)
- `continuationToken`: Token from previous response for next page
- `nameFilter`: Partial match on campaign name
- `statusFilter`: Filter by status (Draft, Active, Paused, Completed, Cancelled)
- `sortBy`: Field to sort by (name, budget, startDate, endDate, createdAt)
- `sortDescending`: Sort direction (default: false)

#### Get Campaign by ID
```http
GET /api/campaigns/{id}
```

**Response: 200 OK** (campaign object)

#### Update Campaign (Partial Update)
```http
PUT /api/campaigns/{id}
Content-Type: application/json

{
  "name": "Updated Summer Sale 2024",
  "budget": 75000
}
```

Only provided fields are updated. Omitted fields remain unchanged.

**Response: 200 OK** (updated campaign object)

#### Delete Campaign (Soft Delete)
```http
DELETE /api/campaigns/{id}
```

**Response: 204 No Content**

Campaign is marked as deleted (IsDeleted = true) but remains in database for audit purposes.

---

## 🗄️ Data Model

### Campaign Entity

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Id | Guid | Unique identifier | Primary key |
| Name | string | Campaign name | Required, max 200 chars |
| Description | string | Campaign description | Required, max 1000 chars |
| StartDate | DateTime | Campaign start date | Required, cannot be in past |
| EndDate | DateTime | Campaign end date | Required, must be after StartDate |
| Budget | double | Campaign budget | Required, > 0, ≤ 100,000,000 |
| Status | CampaignStatus | Current status | Enum: Draft, Active, Paused, Completed, Cancelled |
| CreatedBy | string | User who created campaign | Required, from JWT token |
| CreatedAt | DateTime | Creation timestamp | Auto-set (UTC) |
| UpdatedAt | DateTime | Last update timestamp | Auto-updated (UTC) |
| IsDeleted | bool | Soft delete flag | Default: false |

### Business Rules

1. **End date must be after start date**
2. **Budget must be positive and ≤ $100M**
3. **Cannot activate campaign before start date**
4. **Soft delete preserves audit trail**
5. **Partial updates only modify provided fields**

---

## 🧪 Testing

### Manual Testing with Swagger

1. Run the application: `dotnet run --launch-profile https`
2. Open Swagger UI: https://localhost:7137/swagger
3. Authenticate using the Authorize button
4. Test all endpoints directly in the browser

### Using Postman (Alternative)

Import the Swagger/OpenAPI specification from:
```
https://localhost:7137/swagger/v1/swagger.json
```

Configure OAuth 2.0:
- Auth URL: `https://login.microsoftonline.com/common/oauth2/v2.0/authorize`
- Access Token URL: `https://login.microsoftonline.com/common/oauth2/v2.0/token`
- Client ID: `f50fb135-a878-46ae-a516-fd167f8183ec`
- Scope: `api://f50fb135-a878-46ae-a516-fd167f8183ec/access_as_user`

---

## 🏛️ Technical Decisions


### Design Patterns

- **Repository Pattern**: Abstracts data access, enables testing
- **Dependency Injection**: Loose coupling, testability
- **Factory Method**: Campaign.Create() enforces business rules
- **DTO Pattern**: API contracts separate from domain entities

### Performance Optimizations

- **Database indexes** on Status, CreatedBy, IsDeleted, composite indexes
- **Global query filter** automatically excludes soft-deleted records
- **ExecuteUpdateAsync** for efficient soft delete (no entity loading)
- **Continuation tokens** for scalable pagination
- **AsNoTracking** for read-only queries (can be added for optimization)

---

## 🔒 Security

- **Azure AD OAuth 2.0** with JWT Bearer tokens
- **HTTPS only** in production (enforced via HSTS)
- **Multitenant support** with personal account fallback
- **Token validation**: Signature, expiry, audience checks
- **No client secrets** (using PKCE for public clients)
- **CORS configured** for specific origins only

---

## 📦 Project Structure
```
CampaignsApi/
├── CampaignsApi.API/                    # Entry point, controllers, Swagger
│   ├── Controllers/
│   │   └── CampaignsController.cs
│   ├── Program.cs                       # DI configuration, middleware
│   └── appsettings.json
├── CampaignsApi.Application/            # Business logic, DTOs, validators
│   ├── DTOs/
│   │   ├── CreateCampaignDto.cs
│   │   ├── UpdateCampaignDto.cs
│   │   └── CampaignResponseDto.cs
│   ├── Services/
│   │   └── CampaignService.cs
│   ├── Validators/
│   │   ├── CreateCampaignValidator.cs
│   │   └── UpdateCampaignValidator.cs
│   └── Interfaces/
│       ├── ICampaignService.cs
│       └── ICampaignRepository.cs
├── CampaignsApi.Infrastructure/         # Database, repositories
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Migrations/
│   │   └── Repositories/
│   │       └── CampaignRepository.cs
├── CampaignsApi.Domain/                 # Core entities, business rules
│   ├── Entities/
│   │   └── Campaign.cs
│   ├── Enums/
│   │   └── CampaignStatus.cs
│   └── Exceptions/
│       └── DomainException.cs
└── README.md
```

---

## 📚 Additional Resources

- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Azure AD Authentication](https://learn.microsoft.com/en-us/azure/active-directory/develop/)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

## 👤 Author

**Akinola**  
Systems Engineer | Backend Developer  
Email: akinolatofunmi.tech@gmail.com
[LinkedIn](linkedin.com/in/oluwatofunmi-ac)

---

## 📝 License

This project is for assessment purposes.

---

## 🙏 Acknowledgments

Built as part of the Infinion Backend Assessment demonstrating:
- Production-ready .NET API development
- Clean Architecture principles
- Azure AD integration
- Modern pagination strategies
- Enterprise authentication patterns