# Lianer Backend 2.0

[English](README.md) | [Swedish](README.sv.md) | [Documentation](deployment.md) | [ADR](docs/adr/0001-choosing-azure-hosting.md) | [AI](README.md#ai-driven-feature-gemini-ai-integration) | <a href="https://lianer-frontend.icybush-5ce7e353.italynorth.azurecontainerapps.io/" target="_blank" rel="noopener noreferrer">Frontend Live Demo</a>

A secure and distributed ASP.NET Core 9 microservices architecture built for cloud deployment. Features JWT authentication, Google OAuth2 integration, external API communication (Hunter.io & Gemini AI), and passwordless Azure Key Vault integration.

**Live Application:** <a href="https://lianer-frontend.icybush-5ce7e353.italynorth.azurecontainerapps.io/" target="_blank" rel="noopener noreferrer">Lianer Frontend App</a>

![Build & Test](https://github.com/exikoz/Lianer-backend/actions/workflows/pr.yml/badge.svg)
![Deploy to Azure](https://github.com/exikoz/Lianer-backend/actions/workflows/deploy.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![Azure](https://img.shields.io/badge/Azure-Container%20Apps-blue?logo=microsoftazure&logoColor=white)
![Azure Key Vault](https://img.shields.io/badge/Azure-Key%20Vault-purple?logo=microsoftazure&logoColor=white)
![Azure Monitor](https://img.shields.io/badge/Azure-Monitor%20%2F%20App%20Insights-orange?logo=microsoftazure&logoColor=white)
![AI](https://img.shields.io/badge/AI-Google%20Gemini-red?logo=googlegemini&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-black?logo=githubactions&logoColor=white)

---

## Table of Contents

- [Deployment and System Status](#deployment-and-system-status)
- [Architecture](#architecture)
- [Features](#features)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)
- [Security](#security)
- [Testing](#testing)
- [CI/CD Pipeline](#cicd-pipeline)
- [Advanced Design & Resiliency Patterns](#advanced-design--resiliency-patterns)
- [Surgical Test Flow (End-to-End)](#surgical-test-flow-end-to-end)
- [Observability & Monitoring](#observability--monitoring)
- [AI-Driven Feature (Gemini AI Integration)](#ai-driven-feature-gemini-ai-integration)
- [Team](#team)
- [Contact](#contact)
- [Technical Verification (Logs)](#technical-verification-logs)

---

## Deployment and System Status

The system is fully containerized and deployed in a unified cloud environment. Below is a comprehensive overview of how the different components of the system have been built, secured, quality-assured, and put into production. Click on each section to expand and read more:

<details>
<summary><b>Containerization & Azure Hosting (Fullstack)</b></summary>

To guarantee a reproducible and consistent production environment, we have containerized our entire application stack. 

#### Backend Microservices (Lianer.Core.API & Lianer.Features.API)
The containers are managed via multi-stage Dockerfiles. To achieve the highest safety standards (defense in depth), we made the following design choices:
- **Chiseled and Rootless Images:** We use the minimal `mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled` as the runtime image. This contains no operating system shell (no `sh` or `bash`), no package manager, and no extra tools. This dramatically minimizes the container's attack surface, as an attacker cannot run custom commands or scripts.
- **Non-Root User (USER app):** The applications run under the built-in non-privileged user `app` (UID 1654). Since non-root users are not allowed to listen on ports below 1024, the apps are configured to listen on port **8080**. This prevents an attacker from obtaining administrator privileges (root access) on the host machine in the event of a container vulnerability.

#### Frontend Container (Nginx)
The frontend (Vanilla JS SPA) is containerized in an unprivileged (rootless) Nginx image (`nginxinc/nginx-unprivileged:alpine`) listening on port 8080. 
- **Architecture Decision (ADR):** Originally, we planned to use Azure Static Web Apps (SWA). However, Azure for Students accounts only support 5 specific regions, and SWA is not available in any of the approved regions in our subscription (Policy Error: `RequestDisallowedByAzure`). The solution was to redirect to a containerized frontend in Azure Container Apps (ACA), which had the added benefit of bringing the entire application stack together into the same unified ACA environment. See [ADR 0001: Choosing Azure Hosting](docs/adr/0001-choosing-azure-hosting.md).

*   **For complete design details, see:** [1. Containerisering av backend-mikrotjänster (Task 1)](deployment.md#1-containerisering-av-backend-mikrotjänster-task-1) and [2. Containerisering & Molnvärdskap för Frontend (ADR)](deployment.md#2-containerisering--molnvärdskap-för-frontend-adr).

<details>
<summary><b>View Screenshots</b></summary>

![Azure Container Apps Running Status](docs/images/containerapp-running-status.png)
*Azure Container Apps running status.*

![Container Apps Environment Variables](docs/images/containerapp-environment-variables.png)
*ACA environment variables configuration.*
</details>
</details>

<details>
<summary><b>Secure Configuration & Key Vault</b></summary>

We have eliminated the risk of leaked production keys and connection strings by completely purging source code and configuration files of secrets.

#### Azure Key Vault Architecture
- **Centralized Storage:** All secrets (JWT keys, Google OAuth secrets, and external API keys) are stored centrally in Azure Key Vault.
- **Infrastructure as Code (Bicep):** The entire infrastructure for Key Vault is set up with Bicep-code in `infra/keyvault.bicep` and is deployed automatically via CI/CD.

#### Passwordless Authentication (Managed Identity & RBAC)
- **Passwordless Access:** The services in Azure Container Apps use an embedded identity (**Managed Identity**) to communicate with Key Vault. 
- **Least Privilege (RBAC):** Instead of the legacy access policies, we use role-based access control (RBAC) via Bicep-code (`infra/roleAssignments.bicep`). The services are assigned the role *Key Vault Secrets User*, giving them *only* read permissions to secrets at runtime.
- **Safe Local Fallback:** In `Program.cs`, there is try-catch logic invoking `DefaultAzureCredential()`. If the app cannot connect to Azure during startup (e.g., during offline local development), it falls back automatically to local User Secrets, enabling the development team to collaborate smoothly regardless of Azure permissions.

*   **For complete security details, see:** [4. Säkerhet & Key Vault (Epic 4)](deployment.md#4-säkerhet--key-vault-epic-4).

<details>
<summary><b>View Screenshots</b></summary>

![Key Vault RBAC Role Assignments](docs/images/keyvault-identity-rbac.png)
*Key Vault RBAC role assignments.*

![Key Vault Active Secrets](docs/images/api-documentation/kv-secrets-active-overview.png)
*Azure Key Vault secrets list.*
</details>
</details>

<details>
<summary><b>CI/CD (Build, Test, Release)</b></summary>

To guarantee system stability and traceability, all releases are driven by GitHub Actions pipelines:

#### Quality Gates (pr.yml)
Every Pull Request to `main` or `dev` is validated automatically:
- Runs `npm ci`, linting, and Jest tests for the frontend.
- Runs `dotnet restore`, `build`, and tests for the backend.
- Executes a test build (`docker build`) of all Dockerfiles to ensure the container infrastructure is intact before code is allowed to merge.

#### Continuous Deployment (deploy.yml)
When code is merged into `main`, it is deployed automatically:
- Logs in securely to Azure and Azure Container Registry (ACR) securely without storing any passwords (using OIDC Federated Credentials).
- Builds the production images and tags them with the unique **GitHub Commit SHA** (instead of just `latest`). This provides absolute traceability from the running code in production directly to the specific code line in Git.
- Updates the Container Apps environment (`az containerapp update`) in the Italy North region.

*   **For complete pipeline details, see:** [3. CI/CD Pipeline (Epic 3)](deployment.md#3-cicd-pipeline-epic-3).
</details>

<details>
<summary><b>Monitoring & Diagnostics</b></summary>

We have established a complete monitoring and logging solution in our production environment to quickly isolate and troubleshoot issues.

#### Monitoring with Application Insights
- **Automatic Tracking:** Logs incoming HTTP requests, response times, status codes, unhandled exceptions (via our custom `ExceptionMiddleware`), and outgoing dependency calls via `HttpClient`.
- **Distributed Tracing (Application Map):** Azure automatically maps the communication chain (Frontend ➔ Features API ➔ Core API ➔ Hunter.io). If any link in the chain is slow or failing, it shows up immediately on the map.

#### Operations Runbook & KQL
In the event of service disruptions, we troubleshoot the system using the following runbook steps:
1. **Real-time Logs (Log Stream):** View container logs in real-time directly in Azure Portal or via CLI:
   ```bash
   az containerapp logs show --name lianer-core-api --resource-group rg-lianer-prod --follow
   ```
2. **Real-time Telemetry (Live Metrics):** Follow CPU, memory, request rates, and log streams live in Application Insights.
3. **Log Analysis via KQL (Kusto Query Language):** Run queries in Log Analytics to identify patterns. Example queries included in the documentation:
   * Average response time per endpoint (Performance analysis)
   * Traffic trend (Requests per hour over the last 24 hours)
   * Errors and failed external requests (Dependency analysis)

*   **For complete runbook steps and screenshots, see:** [5. Övervakning & Felsökbarhet (Epic 5)](deployment.md#5-övervakning--felsökbarhet-epic-5).

<details>
<summary><b>View Screenshots</b></summary>

![Application Insights Program Map](docs/images/application-insights-map.png)
*Distributed Tracing program map in Application Insights.*

![Application Insights Live Metrics](docs/images/application-insights-live-metrics.png)
*Live telemetry stream in Application Insights.*

![Average Duration KQL Query](docs/images/log-analytics-average-duration.png)
*Average response time per endpoint query results.*

![Request Trend KQL Query](docs/images/log-analytics-request-trend.png)
*Traffic trend query results.*

![Dependencies KQL Query](docs/images/log-analytics-dependencies.png)
*Failed external requests dependency query.*
</details>
</details>

---

## Architecture

### System Architecture Map

The following map illustrates the cloud infrastructure and the request flow of the Lianer fullstack application deployed in Azure:

```mermaid
graph TD
    classDef client fill:#e1f5fe,stroke:#0288d1,stroke-width:2px;
    classDef aca fill:#e8f5e9,stroke:#388e3c,stroke-width:2px;
    classDef azure fill:#ede7f6,stroke:#5e35b1,stroke-width:2px;
    classDef ext fill:#fff3e0,stroke:#f57c00,stroke-width:2px;

    Browser["User Browser"]:::client
    Frontend["Lianer Frontend (Nginx Container)"]:::aca

    subgraph Azure ["Azure Italy North - Production Environment"]
        direction TB
        subgraph ACA_Env ["Azure Container Apps Environment"]
            Frontend
            CoreAPI["Lianer.Core.API (.NET 9)"]:::aca
            FeaturesAPI["Lianer.Features.API (.NET 9)"]:::aca
        end

        KeyVault["Azure Key Vault"]:::azure
        AppInsights["Application Insights & Log Analytics"]:::azure
    end

    subgraph ExternalServices ["External Services"]
        Google["Google OAuth 2.0"]:::ext
        Hunter["Hunter.io API"]:::ext
        Gemini["Gemini AI API"]:::ext
    end

    Browser -->|HTTPS| Frontend
    Browser -->|HTTPS / JWT Auth| CoreAPI
    Browser -->|HTTPS / JWT Auth| FeaturesAPI

    FeaturesAPI -->|HTTP / Polly / CoreApiClient| CoreAPI
    FeaturesAPI -->|HTTPS| Hunter
    FeaturesAPI -->|HTTPS| Gemini
    CoreAPI -->|HTTPS| Google

    CoreAPI -.->|Managed Identity / RBAC| KeyVault
    FeaturesAPI -.->|Managed Identity / RBAC| KeyVault

    CoreAPI -.->|Telemetry / AppInsights SDK| AppInsights
    FeaturesAPI -.->|Telemetry / AppInsights SDK| AppInsights
    Frontend -.->|Log Streaming| AppInsights
```

### Request and Resilience Flow (Flowchart)

Below is the sequence flowchart demonstrating how requests flow through the system, protected by Polly resilience mechanisms and enriched with Gemini AI capabilities:

```mermaid
sequenceDiagram
    autonumber
    actor User as User Browser
    participant FE as Lianer Frontend
    participant Features as Lianer.Features.API
    participant Core as Lianer.Core.API
    participant Hunter as Hunter.io API
    participant AI as Gemini AI API

    User->>FE: Access App / Request Actions
    FE-->>User: Load SPA Assets (Nginx)
    
    User->>Features: POST /api/v1/leads/import/{domain} (JWT Token)
    activate Features
    
    Features->>Core: GET /api/v1/users/{userId} (Verify Identity)
    Note over Features,Core: CoreApiClient under Polly protection
    alt Core API is Healthy
        Core-->>Features: 200 OK (User Summary)
    else Core API is Slow / Failing
        Note over Features,Core: Polly Retries with Exponential Backoff
        Note over Features,Core: Circuit Breaker Opens if Failure Ratio > 50%
        Features-->>User: 503 Service Unavailable / Fallback
    end

    Features->>Hunter: GET /v2/domain-search (Enrichment)
    Hunter-->>Features: Return Domain Leads & Contacts

    Features->>AI: Process Tasks / Generate Recommendations
    AI-->>Features: Return AI Agent Insights (Gemini)

    Features-->>User: Return Enriched Leads (JSON)
    deactivate Features
```

<details>
<summary><b>Services Details & Inter-service Communication</b></summary>

### Services Overview

The system is split into three main components:
1. **Lianer Frontend:** A Vanilla JavaScript single-page application packaged inside a secure, rootless Nginx container (`nginxinc/nginx-unprivileged:alpine`) running on port 8080.
2. **Lianer.Core.API:** Core service managing user administration, session management (JWT/Google SSO), and localized database operations.
3. **Lianer.Features.API:** Business features service executing lead enrichment via Hunter.io, bulk lead imports, and tasks management.

#### Lianer.Core.API Endpoints:

**User & Session Management:**
- `POST /api/v1/users` - Register new user
- `GET /api/v1/users` - List all users (cached)
- `GET /api/v1/users/{id}` - Get specific user (cached)
- `PUT /api/v1/users/{id}` - Update user profile (requires authentication)
- `DELETE /api/v1/users/{id}` - Delete user (own account only, requires authentication)
- `POST /api/v1/sessions` - Traditional login (email/password)
- `POST /api/v1/sessions/google` - Google OAuth2 login
- `GET /api/v1/sessions/google/url` - Get Google authorization URL

**Contacts Management (requires authentication):**
- `GET /api/v1/contacts/{id}` - Get contact by ID
- `POST /api/v1/contacts` - Create new contact
- `PUT /api/v1/contacts/{id}` - Update existing contact
- `DELETE /api/v1/contacts/{id}` - Delete contact

**Activities Management (requires authentication):**
- `GET /api/v1/activities` - List all activities paginated
- `GET /api/v1/activities/{id}` - Get specific activity by ID
- `GET /api/v1/activities/user/{id}` - List activities for a specific user
- `POST /api/v1/activities` - Create new activity
- `PUT /api/v1/activities` - Update existing activity
- `DELETE /api/v1/activities/{id}` - Delete activity

**Activity Notes Management (requires authentication):**
- `GET /api/v1/activities/{activityId}/notes` - List notes for an activity paginated
- `GET /api/v1/activities/{activityId}/notes/{noteId}` - Get specific note
- `POST /api/v1/activities/{activityId}/notes` - Create note for an activity
- `PUT /api/v1/activities/{activityId}/notes/{noteId}` - Update note
- `DELETE /api/v1/activities/{activityId}/notes/{noteId}` - Delete note

#### Lianer.Features.API Endpoints:
- `GET /api/v1/leads` - List all leads (enriched with usernames)
- `GET /api/v1/leads/{id}/details` - Get lead details
- `GET /api/v1/leads/enrich/{domain}` - Enrich domain via Hunter.io
- `POST /api/v1/leads/import/{domain}` - Bulk import contacts from domain (requires authentication)
- `PATCH /api/v1/leads/{leadId}/assign` - Assign lead to user (requires authentication)
- `POST /api/v1/leads/prepare-test` - Prepare test data (requires authentication)

### Inter-service Communication

**CoreApiClient (Features API → Core API):**
```csharp
public class CoreApiClient
{
    // Fetch user information to enrich leads
    public async Task<CoreUserSummaryDto?> GetUserSummaryAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"/api/v1/users/{userId}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CoreUserSummaryDto>();
    }
}
```

**Resilience Configuration:**
```csharp
builder.Services.AddHttpClient<CoreApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5297/");
})
.AddResilienceHandler("core-api-pipeline", pipeline =>
{
    pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        Delay = TimeSpan.FromSeconds(2)
    });

    pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(60)
    });
});
```

<details>
<summary><b>View Screenshot</b></summary>

![Microservices Communication](docs/images/api-documentation/k-126-microservices-terminal-handshake.png)
*Terminal output showing successful communication between Core API and Features API.*
</details>
</details>

---

## Features

<details>
<summary><b>Technical Features List</b></summary>

### RESTful API Design
- Plural nouns for all URLs (`/api/v1/users`, `/api/v1/leads`)
- Correct HTTP methods (GET, POST, PUT, DELETE, PATCH)
- Standardized status codes (200, 201, 204, 400, 401, 403, 404, 500, 503)
- DTOs for separation between database models and API contracts
- URL versioning (`/api/v1/...`) via `Asp.Versioning.Http`

### Security
- JWT Bearer Authentication with HMAC-SHA256
- BCrypt hashing for passwords
- Google OAuth2 SSO with automatic user registration
- Strict CORS policy (no `AllowAnyOrigin`)
- Data Annotations for input validation (`[Required]`, `[EmailAddress]`, `[StringLength]`)
- Custom Action Filter for centralized model validation
- Ownership control (users can only delete their own accounts)

### Performance (Caching)
- In-Memory Cache (MemoryCache) for resource-intensive GET endpoints
- Cache invalidation (eviction) on data changes (POST, PUT, DELETE, PATCH)
- Exponential backoff with jitter for resilient requests
- Rate Limiting (Fixed Window: 100 requests/minute)
- Pagination on list endpoints (`?page=1&pageSize=20`)
- Advanced filtering via query parameters

### External API Integration
- **Hunter.io API**: Integrates via a Typed Client with Polly resilience to search domains and enrich lead details in Features API.
- **Google OAuth 2.0 API**: Used in Core API to authorize users via Google Single Sign-On (SSO).
- **Google Gemini AI API**: Integrates with Features API to run AI categorization, lead/task insights, and recommendations.
- Secure API key and client secret management using Azure Key Vault (production) and User Secrets (development).
- Integrated error handling with `EnsureSuccessStatusCode()` and custom exception middleware.
- Built-in API resiliency using the Polly Standard Resilience Handler (retries, backoff with jitter, circuit breaker).

### Testing
- Unit tests with xUnit and Moq
- Integration tests with `WebApplicationFactory`
- Correct DI architecture with `ValidateOnBuild` and `ValidateScopes`
- Avoids Service Locator anti-pattern

### API Documentation
- Scalar for modern API documentation
- XML comments on all endpoints
- OAuth2 security scheme in OpenAPI

<details>
<summary><b>View Screenshot</b></summary>

![API Documentation](docs/images/api-documentation/leads-import-scalar-test.png)
*Scalar API documentation for bulk lead import.*
</details>
</details>

---

## Getting Started

<details>
<summary><b>Setup, Installation & Running Guide</b></summary>

### Prerequisites

- .NET 9.0 SDK or later
- Git
- A text editor (Visual Studio, VS Code, Rider)
- Google OAuth2 credentials (for SSO functionality)
- Hunter.io API key (for lead enrichment)

### Installation

#### 1. Clone the repository

```bash
git clone https://github.com/exikoz/Lianer-backend.git
cd Lianer-backend
```

#### 2. Configure User Secrets

**Core API:**
```bash
cd Lianer.Core.API

# JWT Settings (MANDATORY)
dotnet user-secrets set "JwtSettings:SecretKey" "MySecretKeyMustBeAtLeastThirtyTwoChars123!!"
dotnet user-secrets set "JwtSettings:Issuer" "LianerIssuer"
dotnet user-secrets set "JwtSettings:Audience" "LianerAudience"
dotnet user-secrets set "JwtSettings:ExpirationMinutes" "60"

# Google OAuth (MANDATORY for Google SSO)
dotnet user-secrets set "Google:Auth:ClientId" "YOUR_CLIENT_ID.apps.googleusercontent.com"
dotnet user-secrets set "Google:Auth:ClientSecret" "YOUR_CLIENT_SECRET"
dotnet user-secrets set "Google:Auth:RedirectUri" "http://localhost:3000/auth/callback"

# Gemini API Key (MANDATORY for AI Agent Chat)
dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY"

# Verify
dotnet user-secrets list
```

**Features API:**
```bash
cd ../Lianer.Features.API

# Hunter.io API Key (MANDATORY for lead enrichment)
dotnet user-secrets set "Hunter:ApiKey" "YOUR_HUNTER_API_KEY"

# Verify
dotnet user-secrets list
```

**Important:** User Secrets are stored outside the project folder and are never checked into Git.

#### 3. Build the project

```bash
cd ..
dotnet build
```

#### 4. Run the services

**Option 1: Manually in separate terminals**

```bash
# Terminal 1: Core API
cd Lianer.Core.API
dotnet run --launch-profile https
# Listening on: https://localhost:5297

# Terminal 2: Features API
cd Lianer.Features.API
dotnet run --launch-profile https
# Listening on: https://localhost:5298
```

**Option 2: Visual Studio Multiple Startup Projects**

1. Right-click on the Solution in Solution Explorer
2. Select "Configure Startup Projects"
3. Choose "Multiple startup projects"
4. Set both `Lianer.Core.API` and `Lianer.Features.API` to "Start"
5. Press F5

### Ports

| Service | HTTP | HTTPS |
|---------|------|-------|
| Core API | 5297 | 7115 |
| Features API | 5266 | 7089 |
</details>

---

## API Documentation

<details>
<summary><b>Scalar API Details & Verification</b></summary>

Both services feature interactive API documentation via Scalar (Development mode only). Since the frontend is not yet implemented, Scalar is used to test and demonstrate all endpoints.

### Core API
**URL:** `https://localhost:5297/scalar/v1`

**Features:**
- Test all endpoints directly in the browser (replaces frontend during development)
- OAuth2 Authorization Code Flow for Google SSO
- Automatic JWT Bearer Token handling
- XML comments for all endpoints
- Complete API specification for future frontend integration

<details>
<summary><b>View Screenshots</b></summary>

![Google SSO Testing](docs/images/api-documentation/google-sso-endpoint-test1.png)
*Google OAuth2 endpoint testing in Scalar.*

![Core API User Creation](docs/images/api-documentation/core-api-user-created-success.png)
*Successful registration of a new user in Core API.*
</details>

### Features API
**URL:** `https://localhost:5298/scalar/v1`

**Features:**
- Test lead enrichment and bulk import (replaces frontend during development)
- Display enriched leads with usernames from Core API
- Hunter.io domain search
- Complete API specification for future frontend integration

<details>
<summary><b>View Screenshots</b></summary>

![Enriched Leads](docs/images/api-documentation/k-126-final-enriched-leads-list.png)
*Enriched lead list with usernames from Core API.*

![Lead Import Success](docs/images/api-documentation/leads-import-sogeti-success.png)
*Successful import and enrichment of leads from Hunter.io in Features API.*

![Hunter.io Contact Search](docs/images/api-documentation/features-hunter-frontend.png)
*Hunter.io search by domain (e.g. stripe.com) under the Contacts/Leads view in the interface, retrieving and displaying found contacts.*
</details>
</details>

---

## Security

<details>
<summary><b>Authentication, SSO, CORS & Key Vault Details</b></summary>

### Authentication

#### JWT Bearer Authentication

**Flow:**
1. User registers via `POST /api/v1/users`
2. User logs in via `POST /api/v1/sessions`
3. Backend returns a JWT token with a 60-minute lifespan
4. Client (Scalar/Postman/future frontend) sends the token in the `Authorization: Bearer {token}` header
5. Backend validates the token on protected endpoints

<details>
<summary><b>View Screenshots</b></summary>

![Scalar Bearer Auth](docs/images/api-documentation/auth-bearer_token_Core.png)
*Configuring BearerAuth (JWT) directly in Scalar for Core API.*

![Password Validation Test](docs/images/api-documentation/core-api-password-validation-test.png)
*Verification of password validation and error handling (401 Unauthorized).*
</details>

**Current Testing:** Use Scalar API documentation to test the authentication flow.

**Token Content:**
```json
{
  "sub": "user-guid",
  "name": "John Doe",
  "email": "john@example.com",
  "userId": "user-guid",
  "exp": 1714838400
}
```

**Protected Endpoints:**
- `PUT /api/v1/users/{id}` - Requires authentication
- `DELETE /api/v1/users/{id}` - Requires authentication + ownership
- `POST /api/v1/leads/import/{domain}` - Requires authentication (Features API)
- `PATCH /api/v1/leads/{leadId}/assign` - Requires authentication (Features API)

#### Google OAuth2 SSO

**Flow (Ready for frontend integration):**
1. Client fetches the authorization URL via `GET /api/v1/sessions/google/url`
2. Client redirects the user to Google
3. Google redirects back with an authorization code
4. Client exchanges the code for an access token (via Google)
5. Client sends the access token to `POST /api/v1/sessions/google`
6. Backend validates the token with Google
7. Backend creates or retrieves the user
8. Backend returns a JWT token

**Current Testing:** Use Scalar API documentation to test the Google SSO flow.

<details>
<summary><b>View Screenshot</b></summary>

![Google SSO Flow](docs/images/api-documentation/SSO1.png)
*Google OAuth2 authorization flow.*
</details>

**Auto-registration:**
- A Google user's first login automatically creates an account
- `Provider = "Google"` and `ExternalProviderId = {google-user-id}`
- No password is stored (`PasswordHash = null`)

### Input Validation

**Data Annotations on DTOs:**
```csharp
public class RegisterRequestDto
{
    [Required(ErrorMessage = "FullName is required")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; }
}
```

**Custom Action Filter:**
```csharp
public class ValidateModelFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}
```

### CORS Policy

**Strict Configuration:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:5173", "http://localhost:3000"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
```

**Important:** Does NOT use `AllowAnyOrigin()` - only specified origins are allowed.

### Rate Limiting

**Fixed Window Limiter:**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 10;
    });
});

app.MapControllers().RequireRateLimiting("fixed");
```

**Result:** Maximum 100 requests per minute per client, thereafter `429 Too Many Requests`.

### Azure Key Vault & Secrets Management

To guarantee high security in production, all credentials and API keys are completely removed from source code and configurations:
- **Azure Key Vault Integration:** In production, secrets are fetched dynamically on application startup from the centralized Azure Key Vault.
- **Managed Identities & RBAC:** Access to Key Vault secrets is secured using Azure RBAC. The container apps are assigned a User-Assigned Managed Identity, which is granted the *Key Vault Secrets User* role. No passwords or client secrets are stored in the code.
- **Local Fallback:** If the app cannot connect to Azure at startup (e.g., during offline local development), it falls back gracefully to local User Secrets.

#### Required Production Secrets (Azure Key Vault)

The following secrets must be added to your Azure Key Vault. Nested JSON configuration sections are separated using a double dash (`--`):

| Configuration Key | Key Vault Secret Name | Purpose |
|-------------------|-----------------------|---------|
| `Gemini:ApiKey` | `Gemini--ApiKey` | API Key for Google Gemini AI integration (Core API) |
| `Hunter:ApiKey` | `Hunter--ApiKey` | API Key for Hunter.io lead enrichment (Features API) |
| `Google:Auth:ClientId` | `Google--Auth--ClientId` | Google OAuth2 Client ID (Core API) |
| `Google:Auth:ClientSecret` | `Google--Auth--ClientSecret` | Google OAuth2 Client Secret (Core API) |
| `Google:Auth:RedirectUri` | `Google--Auth--RedirectUri` | Redirect URI for Google SSO (Core API) |
| `JwtSettings:SecretKey` | `JwtSettings--SecretKey` | Signature Key for JWT generation |
| `JwtSettings:Issuer` | `JwtSettings--Issuer` | Token Issuer domain/identifier |
| `JwtSettings:Audience` | `JwtSettings--Audience` | Token Audience identifier |

<details>
<summary><b>View Screenshot</b></summary>

![Azure Key Vault](docs/images/api-documentation/kv-secrets-active-overview.png)
*Azure Key Vault secrets overview.*
</details>

**Development:**
- User Secrets for local development
- Fallback mechanism only for tests
</details>

---

## Testing

<details>
<summary><b>Testing Suite (Unit, Integration & DI Validation)</b></summary>

### Unit Tests

**Example: AuthService test with Moq**
```csharp
[Fact]
public async Task RegisterAsync_ShouldHashPassword_AndCreateUser()
{
    // Arrange
    var mockContext = new Mock<AppDbContext>();
    var mockLogger = new Mock<ILogger<AuthService>>();
    var mockTokenService = new Mock<ITokenService>();
    
    var service = new AuthService(mockContext.Object, mockLogger.Object, mockTokenService.Object);
    
    var request = new RegisterRequestDto
    {
        FullName = "John Doe",
        Email = "john@example.com",
        Password = "SecurePassword123!"
    };

    // Act
    var result = await service.RegisterAsync(request);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("john@example.com", result.Email);
    mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
}
```

### Integration Tests

**Example: WebApplicationFactory test**
```csharp
public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUser_ShouldReturn201Created()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

### Running Tests

```bash
# All tests
dotnet test

# Only unit tests
dotnet test --filter Category=Unit

# Only integration tests
dotnet test --filter Category=Integration

# With code coverage
dotnet test /p:CollectCoverage=true
```

### Dependency Injection Validation

**Prevents common DI issues:**
```csharp
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;      // Prevents Captive Dependencies
    options.ValidateOnBuild = true;     // Prevents missing registrations
});
```
</details>

---

## CI/CD Pipeline

<details>
<summary><b>CI/CD Pipeline Architecture & Deployment</b></summary>

We have automated the building, testing, and deployment of the entire fullstack system using GitHub Actions:

### 1. Pull Request Validation (`pr.yml`)
- Triggers on any PR against `main` or `dev`.
- Restores dependencies, runs unit and integration tests, and executes a dry-run Docker build on all Dockerfiles (Nginx Frontend, Core API, Features API) to verify that the containerization builds without errors.

### 2. Continuous Deployment (`deploy.yml`)
- Triggers on merges to `main`.
- Logs into Azure Container Registry (ACR) using passwordless OpenID Connect (OIDC) Federated Credentials.
- Builds production-ready Docker images, tags them with the unique **GitHub Commit SHA** (ensuring absolute spårbarhet/traceability), and pushes them to ACR.
- Automatically instructs Azure Container Apps to update and deploy the new revisions in Italy North.
</details>

---

## Advanced Design & Resiliency Patterns

<details>
<summary><b>Advanced Backend Features (Exception Handling, Polly Policies & Caching)</b></summary>

### 1. Custom Exception Middleware

**Robust error handling with RFC 7807 ProblemDetails:**

```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, "Bad Request"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found"),
            NotFoundException => (HttpStatusCode.NotFound, "Not Found"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            InvalidOperationException => (HttpStatusCode.Conflict, "Conflict"),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsJsonAsync(new { Title = title, Status = (int)statusCode });
    }
}
```

### 2. Resilience Patterns with Polly

**Retry Policy for Google API:**
```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            _logger.LogWarning("Retrying Google API call. Attempt: {Attempt}", retryCount);
        });

var result = await retryPolicy.ExecuteAsync(async () =>
{
    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<GoogleUserInfoDto>();
});
```

**Circuit Breaker for Core API:**
```csharp
pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
{
    FailureRatio = 0.5,                          // Open at 50% failure
    SamplingDuration = TimeSpan.FromSeconds(30), // Measure over 30 seconds
    MinimumThroughput = 2,                       // Minimum 2 requests
    BreakDuration = TimeSpan.FromSeconds(60),    // Open for 60 seconds
    OnOpened = args =>
    {
        Console.WriteLine("[Circuit Breaker] Opened. Core API is likely down.");
        return default;
    }
});
```

### 4. Caching on GET Endpoints (In-Memory Cache)

**Implemented in both Core and Features APIs:**

```csharp
// Example from LeadsController
if (cache.TryGetValue(LeadsCacheKey, out object? cachedLeads))
{
    return Ok(cachedLeads);
}

// ... fetch and enrich data ...

cache.Set(LeadsCacheKey, enrichedLeads, new MemoryCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
});
```

**Automatic Invalidation:**
The cache is cleared immediately upon calls to endpoints that change underlying data (e.g., `BulkImport` or `AssignLead`), ensuring the user always sees consistent information.

### 3. Multiple External API Integrations

**Hunter.io (API Key):**
```csharp
builder.Services.AddHttpClient<HunterClient>(client =>
{
    client.BaseAddress = new Uri("https://api.hunter.io/");
})
.AddStandardResilienceHandler();
```

**Google OAuth2 (Bearer Token):**
```csharp
builder.Services.AddHttpClient("GoogleAuth", client =>
{
    client.BaseAddress = new Uri("https://www.googleapis.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```
</details>

---

## Surgical Test Flow (End-to-End)

<details>
<summary><b>Surgical Test Flow Phases</b></summary>

Follow these steps to verify the entire system's functionality (Security, Integration & Caching).

### Phase 1: Core API (Port 5297)
*Open Scalar: http://localhost:5297/scalar/v1*

1.  **Create User**
    *   **Endpoint**: `POST /api/v1/users`
    *   **Body**:
        ```json
        {
          "fullName": "Test Testsson",
          "email": "test@test.se",
          "password": "Password123!"
        }
        ```
    *   **Goal**: Copy the `userId` from the response.

2.  **Login**
    *   **Endpoint**: `POST /api/v1/sessions`
    *   **Body**: Same credentials as above.
    *   **Goal**: Copy the entire `accessToken` field.

3.  **Authorize**
    *   Click **Authorize** in Scalar (**BearerAuth is now selected by default**).
    *   Paste your token into the **Token** field.

---

### Phase 2: Features API (Port 5266)
*Switch tab to Scalar: http://localhost:5266/scalar/v1*

4.  **Authorize**
    *   Click **Authorize** and paste the **same** token you received from Core API.

5.  **Import Leads (Protected)**
    *   **Endpoint**: `POST /api/v1/leads/import/microsoft.com`
    *   **Goal**: Verify you receive `200 OK` and leads are imported from Hunter.io.

6.  **Fetch Lead ID**
    *   **Endpoint**: `GET /api/v1/leads` (Open)
    *   **Goal**: Copy an `id` from the list.

7.  **Assign Lead (Protected)**
    *   **Endpoint**: `PATCH /api/v1/leads/{leadId}/assign`
    *   **Body**:
        ```json
        {
          "userId": "YOUR_USER_ID_FROM_STEP_1"
        }
        ```
    *   **Goal**: Verify the lead is now linked to your user.

---

### Phase 3: Verification

8.  **Integration & Enrichment**
    *   **Endpoint**: `GET /api/v1/leads/{leadId}/details`
    *   **Result**: You should now see `"assignedToName": "Test Testsson"`. Features API has fetched the name live from Core API.

9.  **Caching**
    *   Call `GET /api/v1/leads` multiple times.
    *   **Result**: Check the Features API terminal. You should see the log: `info: Returning leads from cache.`

10. **Invalidation**
    *   Change your name in Core API (`PUT /api/v1/users/{userId}`).
    *   Go back to Features API and fetch leads again.
    *   **Result**: The cache should have been cleared automatically, and the new name should appear immediately.
</details>

---

## Observability & Monitoring

<details>
<summary><b>Observability & Monitoring Details</b></summary>

We have integrated full observability into the production environment:
- **Application Insights & Log Analytics:** Both API backends automatically stream incoming HTTP requests, latencies, exceptions, and dependency calls to Azure Log Analytics.
- **Operations Runbook:** A complete runbook and KQL (Kusto Query Language) template library are documented in [deployment.md](deployment.md) to enable rapid troubleshooting via Live Metrics, Log Streams, and Transaction Search.
- **Screenshots & Telemetry Evidence:** The [docs/images/](file:///c:/Users/D/Lianer-backend/docs/images) folder contains screenshots proving our telemetry works end-to-end, including:
  - `application-insights-map.png` (Distributed Tracing program map)
  - `application-insights-live-metrics.png` (Live telemetry stream)
  - KQL query results for endpoint latencies, requests trend, and external dependencies.
</details>

---

## AI-Driven Feature (Gemini AI Integration)

<details>
<summary><b>AI-Driven Feature Details (Gemini AI)</b></summary>

The Lianer application integrates a secure, backend-driven AI feature utilizing the Gemini AI API:
- **Gemini Agent Integration:** A server-side AI agent that helps users manage, create, update, and delete CRM contacts using natural language directly from a chat interface.
- **Secure Key Management:** The API key for Gemini is stored securely in Azure Key Vault and injected on startup.
- **UX Resiliency:** Includes fallback mechanisms to ensure a smooth user experience even if the external AI service rate-limits or fails.

<details>
<summary><b>View Chat & Security Sandbox Screenshot</b></summary>

![Gemini AI Agent Chat & Security Prompt Test](docs/images/api-documentation/gemini-agent-chat.png)
*AI chat interface demonstrating prompt injection protection (graceful fallback) and contact creation proposal.*
</details>
</details>

---

## Team

**API Architects - .NET Team Malmö**

- [Joco Borghol](https://github.com/JocoBorghol) - Fullstack Developer
- [Alexander Jansson](https://github.com/alexanderjson) - Fullstack Developer
- [Hussein Hasnawy](https://github.com/exikoz) - Fullstack Developer

---

## Contact

For questions or feedback, contact the team via GitHub Issues or create a Pull Request.

**Repository:** [https://github.com/exikoz/Lianer-backend](https://github.com/exikoz/Lianer-backend)

---

## Technical Verification (Logs)

<details>
<summary><b>System Logs & Verification</b></summary>

Here are actual logs from the system verifying that critical functions are active:

### 1. Caching & Invalidering (Features API)
Logs showing the system detecting missing cache data, fetching it, and returning it from memory on subsequent calls.
```text
info: GET /api/v1/leads called (Cache Check)
info: Cache miss. Fetching fresh data and enriching.
info: Saved 10 entities to in-memory store.
...
info: GET /api/v1/leads called (Cache Check)
info: Returning leads from cache.
```

### 2. Resilience with Polly (CoreApiClient)
Verification of Polly monitoring the inter-service call and confirming success within policy.
```text
info: Polly[3]
      Execution attempt. Source: 'CoreApiClient-core-api-pipeline//Retry', 
      Operation Key: '', Result: '200', Attempt: '0', Execution Time: 19.3348ms
```

### 3. Security & Error Handling (Core API)
Logs demonstrating a correctly handled security incident (invalid password) followed by successful login and JWT generation.
```text
warn: Login failed: Invalid password for email test@test.se
fail: Unhandled exception caught by middleware. Status: 401, Path: /api/v1/sessions
...
info: User authenticated successfully: 17c98dcb-9e66-4d3b-9eea-2508d7270839
info: JWT token generated for user: 17c98dcb-9e66-4d3b-9eea-2508d7270839
```
</details>
