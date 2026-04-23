# 🚀 Lianer Backend

A robust ASP.NET Core 9 microservices backend with JWT authentication and Google OAuth2 integration.

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![.NET](https://img.shields.io/badge/.NET-9.0-blue)

---

## 🏗️ Architecture

**Microservices Structure:**
- **Lianer.Core.API** - Authentication, user management, core domain logic
- **Lianer.Features.API** - Business features and external integrations

## 🚀 Quick Start

### Setup
```bash
# Clone repository
git clone https://github.com/exikoz/Lianer-backend.git
cd Lianer-backend

# Configure secrets (replace with your values)
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_JWT_SECRET" --project Lianer.Core.API
dotnet user-secrets set "GoogleAuth:ClientId" "YOUR_GOOGLE_CLIENT_ID" --project Lianer.Core.API
dotnet user-secrets set "GoogleAuth:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET" --project Lianer.Core.API

# Build and run
dotnet build
cd Lianer.Core.API
dotnet run --launch-profile https
```

## 📚 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/users` | Register new user |
| `POST` | `/api/v1/sessions` | Login with credentials |
| `POST` | `/api/v1/sessions/google` | Google OAuth2 login |

---

## 🔐 Authentication

### JWT Authentication
Standard email/password authentication with secure JWT tokens and BCrypt password hashing.

### Google OAuth2 Integration
Seamless Google Sign-In with automatic user registration for first-time users. Features resilient external API integration with Polly retry patterns.

![Google SSO Testing](docs/images/api-documentation/google-sso-endpoint-test1.png)

*Google OAuth2 endpoint testing in Scalar API documentation*

**Features:**
- Auto-registration on first Google login
- Secure token validation with Google APIs

---

## 🛠️ Microservice Integration

The project implements a resilient **Service-to-Service** communication pattern using **Typed HttpClients** and **Standard Resilience Handlers** (Retry, Circuit Breaker).

**Key Features:**
- **Lead Enrichment**: Features API fetches real-time team member data from Core API.
- **Bulk Operations**: Automated test data preparation for cross-service verification.
- **External API**: Integration with **Hunter.io** for domain lead enrichment and bulk import.

![Enriched Leads List](docs/images/api-documentation/k-126-final-enriched-leads-list.png)
*Enriched lead list in Features API showing assigned team member names from Core API.*

---

## 📚 API Documentation

Both services feature interactive API documentation via **Scalar** and **OpenAPI**.

- **Core API**: `http://localhost:5297/scalar/v1`
- **Features API**: `http://localhost:5266/scalar/v1`

![Lead Import Test](docs/images/api-documentation/leads-import-scalar-test.png)
*Testing bulk lead import from Hunter.io in Scalar.*

---

## 👥 Team

**API Architects - .NET Team Malmö:**
- [Joco Borghol](https://github.com/JocoBorghol)
- [Alexander Jansson](https://github.com/alexanderjson)
- [Hussein Hasnawy](https://github.com/exikoz)
