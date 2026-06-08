# ADR 0001: Val av driftsättnings- och hostingplattform för Lianer fullstack

## Status
Godkänd

## Skapad av
Joco

## Datum
2026-06-02

## Kontext (Context)
Lianer-applikationen består av:
1. **Lianer.Core.API**: En backend-mikrotjänst i .NET 9 för användarhantering, JWT-autentisering, sessioner och Google SSO.
2. **Lianer.Features.API**: En backend-mikrotjänst i .NET 9 för leads-hantering, integration med externa API:er (Hunter.io) och Polly-resiliens.
3. **Lianer Frontend**: En separat Vanilla JS-webbapplikation i ett eget källkodsarkiv.

Vårt mål är att sätta upp en stabil, säker, skalbar och automatiserad produktionsnära miljö i Azure med hjälp av en CI/CD-pipeline. Vi vill uppnå låga driftskostnader, minimal administration, samt robust säkerhet.

## Alternativ och Utvärdering

### Backend-hosting (Mikrotjänster)
Vi utvärderade två huvudsakliga alternativ i Azure för att köra våra backend-containrar:

1. **Azure App Service (Web App for Containers)**
   - *Fördelar*: Mycket välbekant och enkelt för enskilda monolitiska webbapplikationer. Bra stöd för HTTPS och anpassade domäner.
   - *Nackdelar*: Att köra flera mikrotjänster kräver antingen en separat App Service för varje container (vilket blir kostsamt eftersom vi måste betala för flera instanser/planer) eller en komplex multi-container-setup som är svår att skala oberoende av varandra. Stödjer inte inbyggd "skalning till noll" (scale-to-zero) i billiga instanser.

2. **Azure Container Apps (ACA)** - *Rekommenderat*
   - *Fördelar*: Designad specifikt för mikrotjänster och container-orkestrering utan den tunga komplexitet som Kubernetes (AKS) medför. Vi kan köra flera oberoende containrar (Core API och Features API) i samma delade miljö (Container Apps Environment). Den stödjer automatisk skalning baserad på HTTP-trafik, inklusive **skalning till noll** under inaktiva perioder, vilket eliminerar kostnader när applikationen inte används. Inbyggd TLS-terminering, intern nätverkskommunikation och DNS.
   - *Nackdelar*: Något högre initial inlärningskurva än App Service.

### Frontend-hosting
Vi utvärderade hur vi ska köra vår Vanilla JS-frontend:

1. **Containeriserad i en Docker-container (t.ex. med Nginx på ACA)**
   - *Nackdelar*: Överflödigt för en ren statisk klientsida (Vanilla JS). Skapar onödig CPU/minne-konsumtion, kräver container-bygge i pipeline och ger långsammare laddtider då filerna inte distribueras globalt via CDN utan måste hämtas från containern.

2. **Azure Static Web Apps (SWA)** - *Rekommenderat*
   - *Fördelar*: Helt skräddarsydd för statiska webbapplikationer (HTML, JS, CSS). Driftsätts direkt från GitHub till ett globalt distribuerat CDN (Content Delivery Network) vilket minimerar laddtider (latens) och ger extremt hög prestanda. Kostnadsfritt för studenter/hobbyprojekt. Erbjuder gratis SSL-certifikat och sömlös integration med GitHub Actions.

3. **Vercel** - *Övervägt men förkastat*
   - *Fördelar*: Extremt enkel developer experience och hög säkerhet tack vare serverless-abstraktioner. Man slipper helt hantera operativsystem eller Nginx, vilket ger samma "security by minimizing attack surface" som en chiseled container.
   - *Nackdelar*: Eftersom vi bygger ett heltäckande Microsoft-ekosystem (Azure, .NET) vill vi behålla all infrastruktur och CI/CD-integration på samma plattform. Azure SWA erbjuder samma fördelar men inom rätt kontext.

## Beslut (Decision)
Vi väljer att använda följande arkitektur för produktion i Azure:
1. **Azure Container Apps (ACA)** för backend-mikrotjänsterna (`Lianer.Core.API` och `Lianer.Features.API`). De körs som separata Container Apps i en gemensam Container Apps Environment.
2. **Azure Static Web Apps (SWA)** för den statiska Vanilla JS-frontenden, ansluten till det separata frontend-repot.

## Konsekvenser (Consequences)
1. **CORS (Cross-Origin Resource Sharing)**: Eftersom frontenden och API-tjänsterna kommer att ligga på olika domäner (t.ex. `*.azurestaticapps.net` vs `*.azurecontainerapps.io`) måste vi konfigurera CORS-policies i .NET-API:erna för att uttryckligen tillåta anrop från den specifika Static Web Apps-domänen.
2. **Pipelines**: Det krävs två separata pipelines i GitHub Actions. En för backend-repot som bygger Docker-images, pushar till Azure Container Registry (ACR) och deployar till ACA. En för frontend-repot som deployar statiska filer direkt till SWA.
3. **Hemlighetshantering**: Inga hemligheter (JWT-nycklar, Google Client Secrets, Hunter API Key) får finnas i källkoden. Dessa hämtas i produktion från **Azure Key Vault** genom att ge våra Container Apps en **Managed Identity** och läsa behörigheter (RBAC) till valvet.

## Säkerhetsdjup (För VG)
För att säkerställa högsta möjliga säkerhetsstandard och begränsa attackytan implementerar vi följande åtgärder:

1. **Hantering av CORS i produktion**: 
   - Vi förbjuder `AllowAnyOrigin` (`*`) i produktion. CORS-policy i backend ställs in att enbart godkänna anrop från den skarpa Static Web App-domänen.
2. **Minsta behörighet (Least Privilege)**:
   - Vi aktiverar **System-assigned Managed Identity** på båda Container Apps. Ingen API-nyckel eller lösenord för Key Vault sparas i containermiljön.
   - Vi tilldelar endast rollen `Key Vault Secrets User` (RBAC) till mikrotjänsternas identiteter så att de endast kan läsa (inte skriva eller ta bort) de specifika hemligheter de behöver.
3. **Stängda endpoints & Intern kommunikation**:
   - `Lianer.Core.API` exponeras publikt för att tillåta login och registrering från frontend.
   - `Lianer.Features.API` kan exponeras publikt, men dess interna anrop till `Lianer.Core.API` (för att berika leads med användarnamn) sker via det interna container-nätverket i ACA, vilket skyddar intern mikrotjänst-trafik från publika nätverksavlyssningar och man-in-the-middle-attacker.
4. **Secret Rotation**:
   - Genom att använda Azure Key Vault kan vi enkelt rotera våra API-nycklar (t.ex. Hunter.io-nyckeln) och JWT-signeringsnycklar utan att behöva bygga om eller starta om våra containrar manuellt.
5. **Chiseled & Rootless Containers (Containersäkerhet)**:
   - Vi bygger och kör containrarna på `mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled`. Dessa avbilder saknar operativsystemsskal (bash/sh), pakethanterare och systemverktyg, vilket minimerar attackytan till ett absolut minimum.
   - Applikationerna körs dessutom under en icke-root-användare (`USER app` / UID 1654), vilket helt eliminerar risken för privilegieeskalering (privilege escalation) mot värddatorn om container-miljön skulle bli komprometterad.

