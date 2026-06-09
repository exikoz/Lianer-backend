# Lianer Backend 2.0

<p align="left">
  <a href="README.en.md" title="English"><img src="docs/svg/england.svg" alt="English" width="30" /></a>
  &nbsp;
  <a href="README.es.md" title="Español"><img src="docs/svg/spain.svg" alt="Español" width="30" /></a>
  &nbsp;
  <a href="README.sv.md" title="Svenska"><img src="docs/svg/sweden.svg" alt="Svenska" width="30" /></a>
  &nbsp;
  <a href="README.de.md" title="Deutsch"><img src="docs/svg/germany.svg" alt="Deutsch" width="30" /></a>
  &nbsp;
  <a href="README.ko.md" title="한국어"><img src="docs/svg/south-korea.svg" alt="한국어" width="30" /></a>
  &nbsp;
  <a href="README.ja.md" title="日本語"><img src="docs/svg/japan.svg" alt="日本語" width="30" /></a>
  &nbsp;
  <a href="README.zh.md" title="中文"><img src="docs/svg/china.svg" alt="中文" width="30" /></a>
</p>

[英語](README.en.md) | [日本語](README.ja.md) | [ドキュメント](deployment.md) | [ADR](docs/adr/0001-choosing-azure-hosting.md) | [AI](README.ja.md#ai-駆動機能-gemini-ai-統合) | [フロントエンドのライブデモ](https://lianer-frontend.icybush-5ce7e353.italynorth.azurecontainerapps.io)

クラウドデプロイ向けに構築された、安全で分散型の ASP.NET Core 9 マイクロサービスアーキテクチャです。JWT 認証、Google OAuth2 統合、Hunter.io と Gemini AI との外部 API 通信、パスワードレス Azure Key Vault 統合、CI/CD、監視、コンテナ化されたフルスタックデプロイを含みます。

**ライブアプリケーション:** [Lianer Frontend App](https://lianer-frontend.icybush-5ce7e353.italynorth.azurecontainerapps.io)

![Build & Test](https://github.com/exikoz/Lianer-backend/actions/workflows/pr.yml/badge.svg)
![Deploy to Azure](https://github.com/exikoz/Lianer-backend/actions/workflows/deploy.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![Azure](https://img.shields.io/badge/Azure-Container%20Apps-blue?logo=microsoftazure&logoColor=white)
![Azure Key Vault](https://img.shields.io/badge/Azure-Key%20Vault-purple?logo=microsoftazure&logoColor=white)
![Azure Monitor](https://img.shields.io/badge/Azure-Monitor%20%2F%20App%20Insights-orange?logo=microsoftazure&logoColor=white)
![AI](https://img.shields.io/badge/AI-Google%20Gemini-red?logo=googlegemini&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-black?logo=githubactions&logoColor=white)

---

## 目次

- [デプロイとシステム状態](#デプロイとシステム状態)
- [アーキテクチャ](#アーキテクチャ)
- [機能](#機能)
- [はじめに](#はじめに)
- [API ドキュメント](#api-ドキュメント)
- [セキュリティ](#セキュリティ)
- [テスト](#テスト)
- [CI/CD パイプライン](#cicd-パイプライン)
- [高度な設計とレジリエンスパターン](#高度な設計とレジリエンスパターン)
- [Team](#team)

---

## デプロイとシステム状態

システムは完全にコンテナ化され、統合された Azure クラウド環境にデプロイされています。このセクションでは、アプリケーションスタックがどのようにビルド、保護、品質保証、監視、本番リリースされるかをまとめます。

<details>
<summary><b>コンテナ化と Azure ホスティング</b></summary>

再現可能な本番環境を保証するため、アプリケーションスタック全体がコンテナ化されています。`Lianer.Core.API` と `Lianer.Features.API` は multi-stage Dockerfile と最小構成の .NET chiseled runtime image を使用します。サービスは非特権ユーザー `app` として **8080** ポートで動作し、攻撃対象領域を減らして defense in depth を強化します。フロントエンド SPA は非特権 Nginx コンテナとしてパッケージ化されています。Azure for Students のリージョン制限により、Azure Static Web Apps ではなく Azure Container Apps が選択されました。

See [deployment.md](deployment.md) and [ADR 0001: Choosing Azure Hosting](docs/adr/0001-choosing-azure-hosting.md) for full details.

<details>
<summary><b>Screenshots</b></summary>

![Azure Container Apps Running Status](docs/images/containerapp-running-status.png)
*Azure Container Apps running status.*

![Container Apps Environment Variables](docs/images/containerapp-environment-variables.png)
*ACA environment variables configuration.*
</details>
</details>

<details>
<summary><b>安全な設定と Key Vault</b></summary>

本番用シークレットはソースコードから削除され、Azure Key Vault に保存されます。Azure Container Apps は Managed Identity と Azure RBAC の `Key Vault Secrets User` ロールを使ってシークレットにアクセスします。ローカル開発では `DefaultAzureCredential()` ロジックにより User Secrets へ安全にフォールバックできます。

<details>
<summary><b>Screenshots</b></summary>

![Key Vault RBAC Role Assignments](docs/images/keyvault-identity-rbac.png)
*Key Vault RBAC role assignments.*

![Key Vault Active Secrets](docs/images/api-documentation/kv-secrets-active-overview.png)
*Azure Key Vault secrets list.*
</details>
</details>

<details>
<summary><b>CI/CD</b></summary>

GitHub Actions はすべての pull request を検証し、`main` への merge 後に自動デプロイします。デプロイパイプラインは OIDC Federated Credentials を使用し、本番 image をビルドし、GitHub commit SHA でタグ付けし、Azure Container Registry に push して Italy North の Azure Container Apps を更新します。
</details>

<details>
<summary><b>Observability と監視</b></summary>

Application Insights と Log Analytics は、HTTP request、応答時間、ステータスコード、例外、外部 dependency call、ログ、Application Map trace をサービスチェーン全体から収集します。

<details>
<summary><b>Screenshots</b></summary>

![Application Insights Program Map](docs/images/application-insights-map.png)
*Distributed tracing map in Application Insights.*

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

## アーキテクチャ

### System Architecture Map

次の図は、Azure にデプロイされた Lianer フルスタックアプリケーションのクラウドインフラと request flow を示します。

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

### Request and Resilience Flow

次の sequence diagram は、request が Polly のレジリエンス機構で保護され、Gemini AI 機能で拡張されながらシステムを通過する流れを示します。

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
<summary><b>サービス詳細とサービス間通信</b></summary>

システムは Vanilla JavaScript フロントエンド、ユーザー/セッション/連絡先/活動/ノートを担当する `Lianer.Core.API`、leads/import/task 処理/AI 支援機能を担当する `Lianer.Features.API` に分割されています。

### Core API エンドポイント

**User & Session Management**
- `POST /api/v1/users`
- `GET /api/v1/users`
- `GET /api/v1/users/{id}`
- `PUT /api/v1/users/{id}`
- `DELETE /api/v1/users/{id}`
- `POST /api/v1/sessions`
- `POST /api/v1/sessions/google`
- `GET /api/v1/sessions/google/url`

**Contacts Management**
- `GET /api/v1/contacts/{id}`
- `POST /api/v1/contacts`
- `PUT /api/v1/contacts/{id}`
- `DELETE /api/v1/contacts/{id}`

**Activities Management**
- `GET /api/v1/activities`
- `GET /api/v1/activities/{id}`
- `GET /api/v1/activities/user/{id}`
- `POST /api/v1/activities`
- `PUT /api/v1/activities`
- `DELETE /api/v1/activities/{id}`

**Activity Notes Management**
- `GET /api/v1/activities/{activityId}/notes`
- `GET /api/v1/activities/{activityId}/notes/{noteId}`
- `POST /api/v1/activities/{activityId}/notes`
- `PUT /api/v1/activities/{activityId}/notes/{noteId}`
- `DELETE /api/v1/activities/{activityId}/notes/{noteId}`

### Features API エンドポイント

- `GET /api/v1/leads`
- `GET /api/v1/leads/{id}/details`
- `GET /api/v1/leads/enrich/{domain}`
- `POST /api/v1/leads/import/{domain}`
- `PATCH /api/v1/leads/{leadId}/assign`
- `POST /api/v1/leads/prepare-test`

### Inter-service Communication

`Lianer.Features.API` は、Polly の retry と circuit-breaker policy で保護された `CoreApiClient` を通じて `Lianer.Core.API` と通信します。

<details>
<summary><b>Screenshot</b></summary>

![Microservices Communication](docs/images/api-documentation/k-126-microservices-terminal-handshake.png)
*Terminal output showing successful communication between Core API and Features API.*
</details>
</details>

---

## 機能

<details>
<summary><b>Technical Feature List</b></summary>

### RESTful API 設計

- `/api/v1/users` や `/api/v1/leads` のような複数形リソース URL
- GET, POST, PUT, PATCH, DELETE の正しい HTTP メソッド
- 標準化されたステータスコード
- データベースモデルと API contract を分離する DTO
- `Asp.Versioning.Http` による URL versioning

### セキュリティ

- HMAC-SHA256 による JWT Bearer Authentication
- BCrypt による password hashing
- 自動登録付き Google OAuth2 SSO
- `AllowAnyOrigin()` を使わない厳格な CORS
- Data Annotations と集中 validation
- 保護された操作の ownership check

### パフォーマンスとキャッシュ

- 高コスト GET endpoint 用 MemoryCache
- 書き込み時の cache invalidation
- jitter 付き exponential backoff
- fixed-window rate limiting
- pagination と filtering

### 外部統合

- domain search と lead enrichment 用 Hunter.io
- SSO 用 Google OAuth2
- categorization, insights, recommendations 用 Google Gemini
- Azure Key Vault と User Secrets
- Polly resilience policies

<details>
<summary><b>Screenshot</b></summary>

![API Documentation](docs/images/api-documentation/leads-import-scalar-test.png)
*Scalar API documentation for bulk lead import.*
</details>
</details>

---

## はじめに

<details>
<summary><b>Setup, Installation & Running Guide</b></summary>

### 前提条件

- .NET 9.0 SDK 以降
- Git
- Visual Studio, VS Code, または Rider
- Google OAuth2 credentials
- Hunter.io API key

### 1. リポジトリを clone

```bash
git clone https://github.com/exikoz/Lianer-backend.git
cd Lianer-backend
```

### 2. User Secrets を設定

**Core API:**

```bash
cd Lianer.Core.API

dotnet user-secrets set "JwtSettings:SecretKey" "MySecretKeyMustBeAtLeastThirtyTwoChars123!!"
dotnet user-secrets set "JwtSettings:Issuer" "LianerIssuer"
dotnet user-secrets set "JwtSettings:Audience" "LianerAudience"
dotnet user-secrets set "JwtSettings:ExpirationMinutes" "60"

dotnet user-secrets set "Google:Auth:ClientId" "YOUR_CLIENT_ID.apps.googleusercontent.com"
dotnet user-secrets set "Google:Auth:ClientSecret" "YOUR_CLIENT_SECRET"
dotnet user-secrets set "Google:Auth:RedirectUri" "http://localhost:3000/auth/callback"

dotnet user-secrets list
```

**Features API:**

```bash
cd ../Lianer.Features.API

dotnet user-secrets set "Hunter:ApiKey" "YOUR_HUNTER_API_KEY"

dotnet user-secrets list
```

### 3. プロジェクトをビルド

```bash
cd ..
dotnet build
```

### 4. サービスを実行

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

### ポート

| Service | HTTP | HTTPS |
|---------|------|-------|
| Core API | 5297 | 7115 |
| Features API | 5266 | 7089 |
</details>

---

## API ドキュメント

<details>
<summary><b>Scalar API Details & Verification</b></summary>

両方のサービスは development mode で Scalar によるインタラクティブ API ドキュメントを公開します。Core API では registration, login, Google SSO, JWT 保護 endpoint, contacts, activities, notes をテストできます。Features API では lead enrichment, bulk import, Core API から取得したユーザー名付き enriched leads をテストできます。

### Core API

**URL:** `https://localhost:5297/scalar/v1`

![Google SSO Testing](docs/images/api-documentation/google-sso-endpoint-test1.png)
*Google OAuth2 endpoint testing in Scalar.*

![Core API User Creation](docs/images/api-documentation/core-api-user-created-success.png)
*Successful registration of a new user in Core API.*

### Features API

**URL:** `https://localhost:5298/scalar/v1`

![Enriched Leads](docs/images/api-documentation/k-126-final-enriched-leads-list.png)
*Enriched lead list with usernames from Core API.*

![Lead Import Success](docs/images/api-documentation/leads-import-sogeti-success.png)
*Successful import and enrichment of leads from Hunter.io in Features API.*
</details>

---

## セキュリティ

<details>
<summary><b>Authentication, SSO, CORS & Key Vault</b></summary>

### JWT Bearer Authentication

ユーザーは登録してログインし、60 分有効な JWT を受け取り、クライアントは保護 endpoint 呼び出し時に `Authorization: Bearer {token}` として送信します。

```json
{
  "sub": "user-guid",
  "name": "John Doe",
  "email": "john@example.com",
  "userId": "user-guid",
  "exp": 1714838400
}
```

### Google OAuth2 SSO

Google OAuth2 は authorization URL を取得し、ユーザーを Google にリダイレクトし、Core API で返却 token を検証し、初回 Google ユーザーを自動登録して local JWT を返します。

### CORS Policy

アプリケーションは明示的な CORS allow-list を使用し、`AllowAnyOrigin()` は使用しません。

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

### Rate Limiting

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

### Azure Key Vault

本番シークレットは Managed Identity と Azure RBAC により Azure Key Vault から読み込まれます。ローカル開発では User Secrets が安全な fallback になります。

![Azure Key Vault](docs/images/api-documentation/kv-secrets-active-overview.png)
*Azure Key Vault secrets overview.*
</details>

---

## テスト

<details>
<summary><b>Testing Suite</b></summary>

バックエンドは unit test、integration test、Dependency Injection validation を使用して設定エラーを早期に検出します。

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

```csharp
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;      // Prevents Captive Dependencies
    options.ValidateOnBuild = true;     // Prevents missing registrations
});
```
</details>

---

## CI/CD パイプライン

<details>
<summary><b>CI/CD Pipeline Architecture & Deployment</b></summary>

Pull request は frontend tests、backend tests、dry-run Docker builds で検証されます。`main` への merge は image build、ACR push、commit-SHA tagging、Azure Container Apps revision update をトリガーします。
</details>

---

## 高度な設計とレジリエンスパターン

<details>
<summary><b>Advanced Backend Features</b></summary>

バックエンドには custom exception middleware、ProblemDetails 風 responses、Polly retries、circuit breakers、MemoryCache、cache invalidation、typed HTTP clients、external API integrations が含まれます。
</details>

---

## 外科的テストフロー (End-to-End)

<details>
<summary><b>Verification Phases</b></summary>

End-to-end 検証フローは、ユーザー作成、ログイン、Scalar 認可、leads import、leads list、lead assign、details での `assignedToName` 確認で構成されます。

1. `POST /api/v1/users`
2. `POST /api/v1/sessions`
3. Authorize Scalar with BearerAuth.
4. `POST /api/v1/leads/import/microsoft.com`
5. `GET /api/v1/leads`
6. `PATCH /api/v1/leads/{leadId}/assign`
7. `GET /api/v1/leads/{leadId}/details`
</details>

---

## Observability と監視

<details>
<summary><b>Observability & Monitoring Details</b></summary>

本番環境は telemetry、latency、exceptions、logs、dependency calls を Application Insights と Log Analytics に送信します。`deployment.md` には runbook 手順と KQL 例が含まれます。
</details>

---

## AI 駆動機能 (Gemini AI 統合)

<details>
<summary><b>AI Feature Details</b></summary>

Features API は server-side AI agent を通じて Gemini AI を統合し、tasks/leads の categorization、insights、recommendations を支援します。Gemini key は Azure Key Vault に保存され、startup 時に注入されます。
</details>

---

## Team

**API Architects - .NET Team Malmö**

- [Joco Borghol](https://github.com/JocoBorghol) - Fullstack Developer
- [Alexander Jansson](https://github.com/alexanderjson) - Fullstack Developer
- [Hussein Hasnawy](https://github.com/exikoz) - Fullstack Developer

---

## Contact

質問やフィードバックは GitHub Issues でチームに連絡するか、Pull Request を作成してください。

**Repository:** [https://github.com/exikoz/Lianer-backend](https://github.com/exikoz/Lianer-backend)

---

## 技術検証 (ログ)

<details>
<summary><b>System Logs & Verification</b></summary>

### Caching & Invalidation

```text
info: GET /api/v1/leads called (Cache Check)
info: Cache miss. Fetching fresh data and enriching.
info: Saved 10 entities to in-memory store.
...
info: GET /api/v1/leads called (Cache Check)
info: Returning leads from cache.
```

### Resilience with Polly

```text
info: Polly[3]
      Execution attempt. Source: 'CoreApiClient-core-api-pipeline//Retry', 
      Operation Key: '', Result: '200', Attempt: '0', Execution Time: 19.3348ms
```

### Security & Error Handling

```text
warn: Login failed: Invalid password for email test@test.se
fail: Unhandled exception caught by middleware. Status: 401, Path: /api/v1/sessions
...
info: User authenticated successfully: 17c98dcb-9e66-4d3b-9eea-2508d7270839
info: JWT token generated for user: 17c98dcb-9e66-4d3b-9eea-2508d7270839
```
</details>
