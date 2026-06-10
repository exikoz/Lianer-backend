# Deployment and Operations Documentation (Lianer Fullstack)

[English](README.md) | [Swedish](README.sv.md) | [Dokumentation](deployment.md) | [ADR](docs/adr/0001-choosing-azure-hosting.md) | [AI](README.sv.md#ai-driven-funktion-gemini-ai-integration) | <a href="https://lianer-frontend.icybush-5ce7e353.italynorth.azurecontainerapps.io/" target="_blank" rel="noopener noreferrer">Frontend Live-demo</a>

Detta dokument beskriver utvärderingen, designbesluten och säkerhetsövervägandena kring driftsättningen av Lianer fullstack-applikation.

<details>
<summary><b>Produktionsmiljö</b></summary>

- **Frontend:** `https://lianer-frontend.icybush-5ce7e353.italynorth.azurecontainerapps.io`
- **Core API:** `https://lianer-core-api.icybush-5ce7e353.italynorth.azurecontainerapps.io`
- **Features API:** `https://lianer-features-api.icybush-5ce7e353.italynorth.azurecontainerapps.io`

</details>

---

<details>
<summary><b>Epic 1: Projekt-setup & baslinje för drift</b></summary>

### Vad som har gjorts
- **Arkitekturinventering:** Kartlagt hela systemarkitekturen bestående av `Lianer.Core.API` (.NET 9), `Lianer.Features.API` (.NET 9) och frontenden (Vanilla JS SPA). Externa integrationer inkluderar Google OAuth 2.0 (SSO), Hunter.io API samt en Gemini AI-integration.
- **Miljödefinitioner (Lokal vs. Produktion):**
  - **Lokal utvecklingsmiljö:** API-tjänsterna körs lokalt med HTTPS på specifika portar (Core: 5297/7115, Features: 5266/7089). Lokala hemligheter hanteras säkert utanför källkoden via **User Secrets** (`dotnet user-secrets`).
  - **Produktionsmiljö i Azure:** Hela stacken (frontend, Core API och Features API) är driftsatt i regionen **Italy North** i en gemensam **Azure Container Apps**-miljö. Hemligheter lagras centralt i **Azure Key Vault** och injiceras lösenordsfritt via **Managed Identity**.
- **Lokal körningsguide:** Dokumenterat exakta instruktioner i [README.sv.md](file:///c:/Users/D/Lianer-backend/README.sv.md) och [README.md](file:///c:/Users/D/Lianer-backend/README.md) för hur man sätter upp User Secrets lokalt, bygger och kör projektet manuellt eller via Multiple Startup Projects.

### Varför vi gjorde det (Arkitekturbeslut)
- Att ha en separation mellan lokala hemligheter (User Secrets) och produktion (Key Vault) eliminerar risken för att känsliga produktionsnycklar eller databasuppgifter råkar checkas in i Git-arkivet.
- Genom att standardisera miljöer och dokumentera den lokala uppstartsguiden kan alla i teamet omedelbart köra, testa och debugga applikationen lokalt på samma villkor.

</details>

<details>
<summary><b>Epic 2: Containerisering & Azure-hosting (fullstack)</b></summary>

### Backend-mikrotjänster (Lianer.Core.API & Lianer.Features.API)

#### Vad som har gjorts
Vi har containeriserat de två .NET 9.0 API-mikrotjänsterna:
1. Skapat **[Lianer.Core.API/Dockerfile](file:///c:/Users/D/Lianer-backend/Lianer.Core.API/Dockerfile)** med en multi-stage build-struktur.
2. Skapat **[Lianer.Features.API/Dockerfile](file:///c:/Users/D/Lianer-backend/Lianer.Features.API/Dockerfile)** med motsvarande struktur.
3. Skapat en gemensam **[.dockerignore](file:///c:/Users/D/Lianer-backend/.dockerignore)** i roten för att minimera storleken på byggkontexten genom att exkludera lokala kompileringsfiler (`bin/`, `obj/`) och hemligheter.
4. Genomfört en framgångsrik lokal testkörning där containrarna bygger, startar och svarar korrekt på hälso- och integrationstester.

#### Varför vi gjorde det
Att köra mikrotjänsterna i containrar ger flera viktiga fördelar:
- **Reproducerbarhet:** Applikationerna körs i en identisk miljö oavsett om det är på en utvecklares dator, i CI/CD-pipelinen eller i Azure. Detta eliminerar problemet "det fungerar på min maskin".
- **Isolering:** Mikrotjänsterna körs isolerat med sina egna beroenden utan att störa varandra på operativsystemsnivå.
- **Förberedelse för molnet:** Containrar är standarden för driftsättning på moderna molnplattformar (t.ex. Azure Container Apps), vilket gör release-processen snabb och automatiserad.

#### Överväganden kring det första alternativet
Under planeringsfasen övervägde vi två olika alternativ för körningsmiljön (Run stage) i våra Docker-filer.

##### Alternativ A: Standard runtime-avbild (Standard .NET ASP.NET Core Runtime)
- **Avbild:** `mcr.microsoft.com/dotnet/aspnet:9.0` (Debian/Ubuntu-baserad)
- **Fördelar:** 
  - Enkel lokal felsökning och konfiguration eftersom avbilden innehåller ett operativsystemsskal (`bash`/`sh`).
  - Lätt att installera externa verktyg (t.ex. curl eller ping) för att felsöka nätverksanslutningar inifrån containern.
- **Nackdelar:**
  - **Stor attackyta:** Innehåller hundratals onödiga paket, skal och verktyg som en potentiell angripare kan utnyttja.
  - **Root-privilegier:** Körs som `root` som standard, vilket ökar risken för att en angripare kan ta kontroll över värddatorn via en sårbarhet i containern (container escape).

##### Alternativ B: Chiseled och Rootless avbild (Det valda alternativet)
- **Avbild:** `mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled` + **`USER app`**
- **Fördelar (Varför vi valde detta):**
  - **Maximal säkerhet (Defense in Depth / Säkerhetsdjup):** Chiseled-avbilden är extremt bantad och innehåller **inget skal (bash/sh), ingen pakethanterare och inga extra verktyg**. En angripare kan därmed inte ladda ner skadliga skript eller köra terminalkommandon i containern.
  - **Minsta behörighet:** Genom att köra som den inbyggda icke-root-användaren `app` (UID 1654) förhindras privilegieeskalering mot värddatorn vid eventuella sårbarheter.
  - **Liten storlek:** Avbilden är runt 100 MB mindre än standardavbilden, vilket ger snabbare byggtider i pipelinen och snabbare uppstartstider i molnet.
- **Nackdelar:**
  - Svårare att felsöka inifrån containern eftersom det inte går att starta en interaktiv terminal (`docker exec -it` fungerar inte då det saknas shell).
  - Kräver att man explicit ställer in `.NET` att lyssna på port `8080` (eftersom icke-root-användare inte får lyssna på portar under 1024, t.ex. standardport 80).

---

### Frontend-container (Nginx & Azure Container Apps)

#### Vad som har gjorts
Efter att ha utvärderat hur frontenden (Vanilla JS) bäst hostas i Azure har vi implementerat följande molnarkitektur:
1. **Container (Rootless Nginx):** Vi har skapat en `Dockerfile` för frontenden som paketerar appen i en minimal, obehörig (rootless) Nginx-avbild (`nginxinc/nginx-unprivileged:alpine`) som lyssnar på port 8080. Detta för att maximera säkerheten enligt "Least Privilege"-principen.
2. **Azure Container Apps (Slutgiltig Lösning):** Frontenden hostas som en Container App i samma miljö (`Managed Environment`) som våra backend-API:er, vilket ger en enhetlig nätverks- och driftsättningsarkitektur.
3. **Byggprocess (Workaround för Studentprenumeration):** För att hantera begränsningar i Azure for Students (där tjänsten *ACR Tasks* är blockerad) byggs Docker-imagarna lokalt via vanlig Docker Engine (eller i GitHub Actions runner-miljö) innan de pushas till Azure Container Registry (ACR).

#### Varför detta valdes
Under implementationen stötte vi på flera strikta Azure Policy-begränsningar knutna till Student-kontona, vilket tvingade fram avgörande arkitekturval:
- **Övervägt alternativ 1 (Azure Static Web Apps):** Initialt var planen att använda SWA eftersom det är industristandard för Vanilla JS. Dock upptäcktes att studentkontona endast tillåter resurser i 5 specifika regioner (bl.a. *Italy North*). Eftersom SWA nu inte är tillgängligt i någon av dessa 5 godkända regioner var denna tjänst **fysiskt omöjlig** att skapa (Policy Error: `RequestDisallowedByAzure`).
- **Valt alternativ (Azure Container Apps):** Genom att styra om till att containerisera även frontenden (vilket uppfyller alternativa betygskrav) kunde vi runda region-spärrarna. Detta gav oss oväntade fördelar:
  - **Enhetlighet:** Hela ekosystemet (frontend och backend) ligger nu i samma Container Apps-miljö.
  - **Säkerhetsdjup:** Den rootless Nginx-containern adderar ytterligare ett lager av minsta behörighet (Defense in Depth).

</details>

<details>
<summary><b>Epic 3: CI/CD Pipeline</b></summary>

### Vad som har gjorts
Vi har implementerat en fullständig och enhetlig CI/CD-pipeline via GitHub Actions över både vårt Frontend- och Backend-repository:
1. **Pull Request Validation (`pr.yml`):** Varje gång en PR skapas mot `main` eller `dev` triggas en pipeline som validerar koden. I frontenden körs `npm ci`, ESLint, och Jest-tester. I backenden körs `dotnet restore`, `build` och `test`. Dessutom görs en "dry run"-byggning (`docker build`) av samtliga Dockerfiler (Nginx, Core API, Features API) för att garantera att infrastrukturen håller.
2. **Continuous Deployment (`deploy.yml`):** Vid godkänd merge till `main` triggas en deploy-pipeline. Pipelinen loggar in mot Azure via lösenordsfria hemligheter (Service Principal / Federated Credentials), bygger produktionsversionerna av Docker-containrarna och laddar upp (pushar) dem till Azure Container Registry (ACR).
3. **Automatiskt Moln-uppdatering:** Som sista steg i `deploy.yml` anropar pipelinen Azure Container Apps (`az containerapp update`) och instruerar Azure att dra ner och driftsätta den nyss uppladdade imagen i Italy North-miljön.

### Varför vi gjorde det (ADR)
- **Quality Gates (Förhindra trasig kod i produktion):** Genom att tvinga alla PRs att passera bygg- och teststegen i `pr.yml` innan de kan mergas, fungerar GitHub Actions som en strikt Quality Gate. Om ett enhetstest fallerar eller om en Docker-image inte går att bygga, blockeras hela sammanslagningen. Detta maximerar kodkvaliteten och systemstabiliteten (kvalitetskrav uppfyllt).
- **Extrem Spårbarhet & SHA-taggning:** Istället för att bara tagga våra Docker-avbilder med `latest` (vilket gör det omöjligt att veta exakt vilken kod som körs om något kraschar), skapade vi ett arkitekturbeslut att **alltid tagga avbilderna med GitHub Commit SHA** (t.ex. `lianer-frontend:${{ github.sha }}`). När vi därefter beordrar Azure Container Apps to använda denna specifika SHA-tagg, har vi uppnått absolut spårbarhet. Uppstår ett fel i produktion kan vi direkt matcha det mot den specifika raden kod i GitHub-repot.
</details>

<details>
<summary><b>Epic 4: Säkerhet & Key Vault</b></summary>

### Vad som har gjorts
För att garantera högsta möjliga säkerhet för applikationerna har vi implementerat Azure Key Vault och därmed helt eliminerat hanteringen av lösenord och anslutningssträngar (secrets) direkt i källkoden eller `appsettings.json`.
1. **Infrastruktur som kod (Bicep):** Vi har definierat infrastrukturen för Key Vault i `infra/keyvault.bicep`. Skapandet av valvet är helt automatiserat och sker genom vår CI/CD pipeline i `deploy.yml`.
2. **Managed Identities (Lösenordsfri åtkomst):** Vi har skapat `infra/roleAssignments.bicep` som använder Azure RBAC för att tilldela rollen *Key Vault Secrets User* till våra mikrotjänster. Detta ger tjänsterna automatisk åtkomst i Azure.
3. **Azure SDK Integration:** I `Program.cs` för både Core och Features API använder vi paketet `Azure.Identity`. Genom att anropa `DefaultAzureCredential()` hämtar .NET-koden automatiskt in rättigheter baserat på miljön den körs i, och kan hämta hemligheter från Key Vaultet helt transparent.

![Key Vault RBAC Rolltilldelningar](docs/images/keyvault-identity-rbac.png)

### Varför vi gjorde det (ADR)
- **Infrastruktur som kod och rollbaserad åtkomst (RBAC):** Vi valde aktivt att inte bygga infrastrukturen med manuella skript, utan att från start använda rollbaserad åtkomst via Bicep-kod (`enableRbacAuthorization: true`). Detta ger en mycket säkrare och modernare åtkomstmodell (Zero Trust), full spårbarhet via Git och minimerar manuella fel jämfört med de äldre behörighetsmetoderna. Våra Container Apps får *enbart* läsrättigheter till hemligheterna (rollen *Key Vault Secrets User*).
- **Workaround för Azure for Students (Enhetlig Resursgrupp):** På grund av hårda begränsningar ("Policy Error") och spärrar i skolkontots prenumeration (t.ex. inga ACR Tasks, och inga Static Web Apps i den tillåtna regionen Italy North) tvingades vi strukturera om infrastrukturen. Lösningen blev att samla hela systemet (frontend, backend och Key Vault) under ett och samma tak i en existerande Resource Group i Italy North. Genom att köra allt som Azure Container Apps kan vi bygga containrarna via GitHub Actions istället. Teamkollegorna har fått riktad åtkomst till denna resursgrupp, och varje enskild container har tilldelats exakt de behörigheter den behöver (Managed Identity) för att allt ska kunna köras och samarbetas kring smidigt trots skolkontots begränsningar.
- **Eliminera hotbilden för läckta nycklar:** Den vanligaste säkerhetsbristen vid molnutveckling är att utvecklare råkar commita (eller spara) produktionslösenord i Git. Genom att tvinga applikationen att hämta dessa "on-the-fly" vid uppstart från Key Vault finns inga riktiga lösenord tillgängliga i klartext för obehöriga som granskar vår källkod.
- **Defense in Depth (Säkerhetsdjup):** I kombination med de rootless Docker-containrarna vi skapade tidigare (Epic 2), lägger Managed Identity till ytterligare ett identitetsbaserat skyddslager. Detta uppfyller kraven för säker molndrift.

</details>

<details>
<summary><b>Epic 5: Övervakning & Felsökbarhet</b></summary>

### Vad som har gjorts
Vi har implementerat en komplett övervaknings- och spårbarhetslösning (Observability) för vår distribuerade fullstack-miljö:
1. **Application Insights SDK Integration:** Installerat `Microsoft.ApplicationInsights.AspNetCore` i både `Lianer.Core.API` och `Lianer.Features.API`. Tjänsterna har konfigurerats att automatiskt ansluta till Azure via miljövariabeln `APPLICATIONINSIGHTS_CONNECTION_STRING`.
2. **Resilient Lokal Fallback:** Om ingen anslutningssträng hittas (t.ex. under lokal utveckling) inaktiveras telemetrinsamlingen utan att applikationerna kraschar eller ger fel.
3. **Log Analytics & Azure-koppling:** Skapat ett Log Analytics-valv i Azure-portalen under resursgruppen `rg-lianer-prod` och anslutit containrarna dit.

![Container App Miljövariabler](docs/images/containerapp-environment-variables.png)

![Container App Running Status](docs/images/containerapp-running-status.png)

### Varför vi gjorde det (ADR)
- **Strukturerad felsökning:** Genom att slussa alla `ILogger.LogError`-anrop från vår `ExceptionMiddleware` till Application Insights, sparas alla unhandled exceptions automatiskt som detaljerade felobjekt med fullständiga stack traces under kategorin *Exceptions* istället för som ostrukturerade textloggar.
- **Distribuerad spårning / Distributed Tracing:** Application Insights SDK spårar automatiskt alla inkommande HTTP-förfrågningar (rutter, svarstider, statuskoder) samt alla utgående beroendeanrop (dependency calls) via `HttpClient`. Detta innebär att när frontenden anropar Features API, som i sin tur gör anrop till Core API och det externa Hunter.io API:et, ritas hela denna kedja upp automatiskt i **Application Map** i Azure. Detta gör att vi kan identifiera exakt var i kedjan en fördröjning eller ett fel uppstår.
![Application Insights Programkarta](docs/images/application-insights-map.png)

---

### Operations Runbook - Felsökning i produktion

Om en deploy misslyckas eller om applikationen kraschar i produktion, följ dessa steg för att lokalisera felet:

#### Steg 1: Kontrollera loggströmmen (Log Stream) och Live Metrics
För att se realtidsloggar direkt från API-containrarna eller analysera prestandadata i realtid:
- **Via Azure-portalen (Log Stream):** Gå till din Container App (t.ex. `lianer-core-api`), klicka på **Log Stream** under sektionen *Monitoring* i sidomenyn.
- **Via Application Insights (Live Metrics):** Öppna din Application Insights-resurs i portalen och klicka på **Live Metrics** under sektionen *Undersök* (Investigate) i sidomenyn. Här ser du realtidsprestanda, CPU/minnesanvändning samt en live-vy av loggar och exceptions från samtliga servrar/instanser som är online.

![Application Insights Live Metrics](docs/images/application-insights-live-metrics.png)

- **Via Azure CLI (Log Stream):** Kör följande kommando i terminalen för att visa loggar i realtid:
  ```bash
  az containerapp logs show \
    --name lianer-core-api \
    --resource-group rg-lianer-prod \
    --follow
  ```

#### Steg 2: Sök i Application Insights (Transaction Search)
Om en användare rapporterar ett specifikt fel (t.ex. med ett `traceId` från det strukturerade felmeddelandet):
1. Gå till din **Application Insights**-resurs i Azure-portalen.
2. Klicka på **Transaction Search** i sidomenyn.
3. Klistra in `traceId` eller sök på t.ex. statuskod `500` för att se den exakta transaktionskedjan och tillhörande felmeddelande med stack-trace.

#### Steg 3: Analysera med KQL (Kusto Query Language)
Klicka på **Logs** under din Application Insights eller ditt Log Analytics Workspace för att köra anpassade frågor. Här är teamets standardfrågor:

* **Hitta de 20 senaste misslyckade anropen (Requests):**
  ```kql
  requests
  | where success == false
  | project timestamp, name, resultCode, duration, url
  | order by timestamp desc
  | limit 20
  ```

* **Genomsnittlig svarstid per API-endpoint (Prestanda-analys):**
  ```kql
  requests
  | summarize GenomsnittligSvarstidMs = avg(duration), AntalAnrop = count() by name
  | order by GenomsnittligSvarstidMs desc
  ```

  ![Genomsnittlig svarstid per endpoint](docs/images/log-analytics-average-duration.png)

* **Trafik-trend (Antal anrop per timme senaste dygnet):**
  ```kql
  requests
  | where timestamp > ago(24h)
  | summarize AntalAnrop = count() by bin(timestamp, 1h)
  | render timechart
  ```

  ![Trafik-trend senaste dygnet](docs/images/log-analytics-request-trend.png)

* **Hitta de 20 senaste undantagen (Exceptions) med felmeddelande och fil:**
  ```kql
  exceptions
  | project timestamp, problemId, outerMessage, type, method
  | order by timestamp desc
  | limit 20
  ```

* **Spåra externa API-anrop (t.ex. till Hunter.io eller internt Core API):**
  ```kql
  dependencies
  | summarize MedeltidMs = avg(duration), MisslyckadeAnrop = countif(success == false), TotalaAnrop = count() by target
  | order by MisslyckadeAnrop desc
  ```

  ![Beroende-analys](docs/images/log-analytics-dependencies.png)

</details>

<details>
<summary><b>Epic 6: Fullstack-integration & Säkerhet</b></summary>

### Vad som har gjorts
- **Integration i Azure:** Frontenden och backenden kommunicerar säkert över HTTPS i Azure Container Apps (ACA).
- **Anpassningar för Frontend-kompatibilitet:** Vi åtgärdade följande buggar och inkompatibiliteter:
  1. Skapade en `[HttpGet]`-endpoint i `ContactsController` för att låta frontenden lista kontakter via paginering (`GetContacts`).
  2. Implementerade null-säkring i `ContactSocialDto` för att förhindra applikationskrascher när sociala länkar saknas.
  3. Säkrade `ActivityController` och `NoteController` genom att hämta användarens ID från JWT `ClaimTypes.NameIdentifier` (`CurrentUserId`) på backend istället för att lita på klientinskickad data i request body.
  4. Fixade logik för `Activity.Update` i domänlagret så att det tillåter att användare tilldelas eller tas bort från aktiviteter på ett resilient sätt.

### Varför vi gjorde det (ADR)
- **Säkerhetsdjup:** Vi förbjuder `AllowAnyOrigin` i produktion. CORS-policy i båda API:erna är strikt inställd att enbart tillåta anrop från frontends Container App-domän. 
- **Behörighetssäkerhet:** Att hämta användar-ID direkt från den signerade JWT-token förhindrar manipulering av användaridentiteter vid skapande av aktiviteter.
- Se detaljerad teknisk beskrivning i [Systemdesign och arkitekturbeslut - Fullstack-integration](docs/adr/0002-system-design-och-arkitektur.md#6-fullstack-integration-epic-6).

</details>

<details>
<summary><b>Epic 7: AI-Integration</b></summary>

### Vad som har gjorts
- **Konceptuell Design:** Vi har förberett arkitekturen för en säker, backend-driven AI-funktion i `Lianer.Features.API` som ansluter till Google Gemini AI API för att analysera och kategorisera leads samt skapa smarta rekommendationer.

### Varför vi gjorde det (ADR)
- **Arkitekturbeslut:** API-nyckeln lagras säkert i Azure Key Vault (`Gemini--ApiKey`) och läses lösenordsfritt under körning via Managed Identity.
- **UX-resiliens:** Anropen omsluts av en try-catch-konstruktion med en lokal fallback. Om Gemini AI drabbas av störningar eller rate-limiting faller systemet tillbaka på en standardtext/kategorisering utan att störa användarflödet eller krascha applikationen.
- Se detaljerad teknisk beskrivning i [Systemdesign och arkitekturbeslut - AI-Integration](docs/adr/0002-system-design-och-arkitektur.md#7-ai-integration-epic-7).

</details>

