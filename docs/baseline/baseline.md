# Architecture Baseline – Lianer Fullstack

## Syfte

Denna dokumentation beskriver nuvarande arkitektur, systemdelar, datalager, externa beroenden, hemligheter och miljöer.

Dokumentet har skrivits för att ge utvecklare en tydlig överblick över hur systemet är uppbyggt, vilka delar som ingår och vad som krävs för att köra och felsöka systemet på ett kontrollerat sätt,
samt även se till att bestämda säkerhetsstandarder följs (hantering av hemligheter osv).


---

## Nuvarande arkitektur

Lianer är en fullstack-applikation som består av en separat frontend och två backend-mikrotjänster.

Systemet består av:

- **Lianer Frontend** – statisk Vanilla JS-applikation
    
- **Lianer.Core.API** – backend-API för kärnfunktioner
    
- **Lianer.Features.API** – backend-API för feature- och integrationsfunktioner
    

Frontend kommunicerar med backend via HTTP-anrop mot versionerade REST-endpoints.

---

## Frontend

Frontend är byggd i Vanilla JavaScript med moduler.

Frontend innehåller vyer för:

- dashboard
    
- kalender
    
- uppgifter
    
- kontakter
    
- inställningar
    
- autentisering (login/registrering)
    

Frontend ansvarar för:

- navigering mellan vyer
    
- rendering av UI
    
- användarinteraktioner
    
- API-anrop till backend
    
- lokal UI-state
    
- offline-/demo-relaterad datahantering
    
- import/export-funktioner, till exempel vCard, CSV, iCal och backup
    

I produktion körs frontenden containeriserad i Azure Container Apps och serveras som statiska filer via en rootless Nginx-container på port 8080.

---

## Backend

Backend består av två separata ASP.NET Core API:er.

### Lianer.Core.API

Core API ansvarar för centrala applikationsfunktioner, till exempel:

- användare
    
- JWT, Cookies
    
- sessionshantering och autentisering
    
- Google SSO
    
- aktiviteter
    
- kontakter
    
- anteckningar
    
- AI-agentens CRM-operationer
    

### Lianer.Features.API

Features API ansvarar för kommunikation till externa  feature-funktioner, till exempel:

    
- externa API-integrationer
    
- enrichment-funktioner (exempelvis hämta leads till kontakter)
    
- Hunter.io-integration
    

Backend exponeras via REST-endpoints och dokumenteras via OpenAPI/Scalar.

---

## Datalager

Systemet använder flera former av datalagring beroende på del av applikationen.

### Backend-datalager

Backend använder ett server-side datalager för applikationsdata som hanteras via API:erna.

Exempel på data som hanteras av backend:

- användare
    
- sessioner
    
- aktiviteter
    
- kontakter
    
- anteckningar
    
- lead-data
    
- AI-relaterade CRM-förslag och operationer
    


### Frontend-lagring

Frontend använder även lokal browser-lagring för UI-state, demo-data och offline-liknande funktionalitet.

Exempel:

- localStorage för applikationsstate, inställningar, filter och vyval
    
- JSON-backup/import/export
    
- iCal-importerade events
    

Detta används som frontend-state och användarvänlig lokal lagring, men backend bör ses som den enda källan för server-side applikationsdata.

---

## Externa beroenden

Projektet använder flera externa beroenden.

### Infrastruktur och drift

- Azure Container Apps
    
- Azure Container Registry
    
- Azure Key Vault
    
- Managed Identity
    
- Application Insights
    
- GitHub Actions
    

### Autentisering och externa API:er

- Google OAuth / Google SSO
    
- Hunter.io API
    
- Gemini API
    

### Frontend-relaterade beroenden

- Nginx för servering av statiska frontendfiler i produktion
    
- vCard/QR/iCal-relaterade klientfunktioner
    
- Jest och jest-axe för testning
    
- ESLint och Lighthouse för kvalitetssäkring
    

---

## Hemligheter och känslig konfiguration

Projektet använder hemligheter för sådant som inte får hårdkodas i källkoden.

Exempel på secrets:

- JWT signing key
    
- Google OAuth client secret
    
- Hunter.io API key
    
- Gemini API key
    
- connection strings
    
- Azure-relaterade deployment-secrets
    
- Key Vault-konfiguration
    

Hemligheter får **aldrig** checkas in i Git oavsett steg i utveckling.  

I lokal utveckling används:

- `dotnet user-secrets`
    
- lokala miljövariabler
    
- lokal utvecklingskonfiguration för icke-känsliga värden
    

I produktion används:

- Azure Key Vault
    
- Managed Identity
    
- RBAC med minsta möjliga behörighet
    

Container Apps ska endast få den behörighet de behöver, till exempel rollen `Key Vault Secrets User` för att kunna läsa secrets men inte skapa, ändra eller radera dem.

---

## Säkerhetsstrategi

Projektet använder flera säkerhetslager.

Exempel:

- JWT-autentisering för skyddade endpoints

- HttpOnly Cookies med CSRF-Cookies 
    
- HTTPS i produktion
    
- CORS begränsat till godkända frontend-origins
    
- inga secrets i källkod
    
- Azure Key Vault för produktionshemligheter
    
- Managed Identity istället för lösenord i produktion
    
- rootless/chiseled backend-containers
    
- rootless Nginx-container för frontend
    
- accept/neka-flöde för AI-agentens ändringar
    
- prompt injection-skydd i AI-agentens systemprompt
    

---

## Observability

Application Insights används för övervakning och felsökning.

Det kan användas för att följa:

- inkommande HTTP-requests
    
- statuskoder
    
- svarstider
    
- exceptions
    
- dependency calls
    
- AI-anrop
    
- fel i produktionsmiljön
    

Detta gör det enklare att felsöka hela vägen  från frontend -> backend -> externa API:er

---

## CI/CD

Projektet använder GitHub Actions.

### Pull Request-pipeline

`pr.yml` används som quality gate och kör exempelvis:

- build
    
- tester
    
- Docker build dry-run
    

Syftet är att trasig kod eller trasig container-konfiguration inte ska mergas.

### Deployment-pipeline

`deploy.yml` körs vid merge/push till `main`.

Den ansvarar för att:

- bygga Docker-images
    
- tagga images med commit SHA
    
- pusha images till Azure Container Registry
    
- deploya till Azure Container Apps
    

Commit SHA-taggning gör det möjligt att spåra exakt vilken kodversion som körs i produktion.



## Environments



### Development

Används under utvecklingsstadiet. Kan köras snabbt
med `dotnet run` och Localhost. Kan även kommunicera
med Frontend klienten (tillåtna portar finns i `appSettings.development.json`).

- API:er körs på localhost
    
- frontend körs lokalt och använder konfigurering för att
bygga API calls till Backend. 
    
- backend kan köras med HTTPS launch profile
    
- secrets måste hanteras via User Secrets eller lokala miljövariabler
    
- CORS tillåter lokala frontend-origins

- Använder in-memory databas (Efcore)


### Production

I produktionsmiljö körs både backend och frontend containeriserat med Azure Container Apps.
    
- Azure Container Registry används för Docker-images
    
- Azure Key Vault för att hantera hemligheter
    
- Managed Identity för lösenordsfri secret-access
    
- Application Insights (OpenTelemetry) för observability

- Frontend serveras vid en rootless Nginx-container och körs även i Azure Container Apps


## Skillnader mellan miljöerna

---


| Område       | Dev                            | Prod                                           |
| ------------ | ------------------------------ | ---------------------------------------------- |
| Syfte        | Utveckling och felsökning      | Riktig drift                                   |
| Frontend     | Lokal dev/static server        | Container App med rootless Nginx               |
| Backend      | `dotnet run`                   | Container Apps                                 |
| API-adresser | localhost                      | publika Azure-URL:er                           |
| Secrets      | User Secrets / lokala env vars | Azure Key Vault                                |
| Identity     | lokal utvecklarkonfiguration   | Managed Identity                               |
| CORS         | lokala origins                 | produktionsorigin                              |
| Logging      | terminal/debugger              | Application Insights + Container App logs      |
