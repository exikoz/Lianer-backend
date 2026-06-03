# Deployment and Operations Documentation (Lianer Fullstack)

Detta dokument beskriver utvärderingen, designbesluten och säkerhetsövervägandena kring driftsättningen av Lianer fullstack-applikation.

---

<details>
<summary><b>1. Containerisering av backend-mikrotjänster (Task 1)</b></summary>

### Vad som har gjorts
Vi har containeriserat de två .NET 9.0 API-mikrotjänsterna:
1. Skapat **[Lianer.Core.API/Dockerfile](file:///c:/Users/D/Lianer-backend/Lianer.Core.API/Dockerfile)** med en multi-stage build-struktur.
2. Skapat **[Lianer.Features.API/Dockerfile](file:///c:/Users/D/Lianer-backend/Lianer.Features.API/Dockerfile)** med motsvarande struktur.
3. Skapat en gemensam **[.dockerignore](file:///c:/Users/D/Lianer-backend/.dockerignore)** i roten för att minimera storleken på byggkontexten genom att exkludera lokala kompileringsfiler (`bin/`, `obj/`) och hemligheter.
4. Genomfört en framgångsrik lokal testkörning där containrarna bygger, startar och svarar korrekt på hälso- och integrationstester.

### Varför vi gjorde det
Att köra mikrotjänsterna i containrar ger flera viktiga fördelar:
- **Reproducerbarhet:** Applikationerna körs i en identisk miljö oavsett om det är på en utvecklares dator, i CI/CD-pipelinen eller i Azure. Detta eliminerar problemet "det fungerar på min maskin".
- **Isolering:** Mikrotjänsterna körs isolerat med sina egna beroenden utan att störa varandra på operativsystemsnivå.
- **Förberedelse för molnet:** Containrar är standarden för driftsättning på moderna molnplattformar (t.ex. Azure Container Apps), vilket gör release-processen snabb och automatiserad.

### Överväganden kring det första alternativet
Under planeringsfasen övervägde vi två olika alternativ för körningsmiljön (Run stage) i våra Docker-filer.

#### Alternativ A: Standard runtime-avbild (Standard .NET ASP.NET Core Runtime)
- **Avbild:** `mcr.microsoft.com/dotnet/aspnet:9.0` (Debian/Ubuntu-baserad)
- **Fördelar:** 
  - Enkel lokal felsökning och konfiguration eftersom avbilden innehåller ett operativsystemsskal (`bash`/`sh`).
  - Lätt att installera externa verktyg (t.ex. curl eller ping) för att felsöka nätverksanslutningar inifrån containern.
- **Nackdelar:**
  - **Stor attackyta:** Innehåller hundratals onödiga paket, skal och verktyg som en potentiell angripare kan utnyttja.
  - **Root-privilegier:** Körs som `root` som standard, vilket ökar risken för att en angripare kan ta kontroll över värddatorn via en sårbarhet i containern (container escape).

#### Alternativ B: Chiseled och Rootless avbild (Det valda alternativet)
- **Avbild:** `mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled` + **`USER app`**
- **Fördelar (Varför vi valde detta):**
  - **Maximal säkerhet (Defense in Depth / Säkerhetsdjup):** Chiseled-avbilden är extremt bantad och innehåller **inget skal (bash/sh), ingen pakethanterare och inga extra verktyg**. En angripare kan därmed inte ladda ner skadliga skript eller köra terminalkommandon i containern.
  - **Minsta behörighet:** Genom att köra som den inbyggda icke-root-användaren `app` (UID 1654) förhindras privilegieeskalering mot värddatorn vid eventuella sårbarheter.
  - **Liten storlek:** Avbilden är runt 100 MB mindre än standardavbilden, vilket ger snabbare byggtider i pipelinen och snabbare uppstartstider i molnet.
- **Nackdelar:**
  - Svårare att felsöka inifrån containern eftersom det inte går att starta en interaktiv terminal (`docker exec -it` fungerar inte då det saknas shell).
  - Kräver att man explicit ställer in `.NET` att lyssna på port `8080` (eftersom icke-root-användare inte får lyssna på portar under 1024, t.ex. standardport 80).

</details>

<details>
<summary><b>2. Containerisering & Molnvärdskap för Frontend</b></summary>

### Vad som har gjorts
Jag utvärderade hur frontenden (Vanilla JS) bäst hostas i Azure. Resultatet blev en lösning som uppfyller kraven för både driftsättning och flexibilitet:
1. **Utkast till Container (Rootless Nginx):** Jag skapade en `Dockerfile` som paketerar frontenden med en minimal, obehörig (rootless) Nginx-avbild (`nginxinc/nginx-unprivileged:alpine`) som lyssnar på port 8080. Detta gjordes för att maximera säkerheten enligt "Least Privilege"-principen (VG-krav). Filen ligger nu direkt i roten på frontend-repot, vilket säkerställer att vi har en säker, container-redo version av frontenden (Task 2).
2. **Azure Static Web Apps (Vald lösning):** Efter utvärdering kom jag fram till att **Azure Static Web Apps (SWA)** är det absolut bästa valet för att bygga och hosta Vanilla JS-frontenden, vilket integrerar direkt med projektets CI/CD pipeline (Task 5).

### Varför detta valdes
- **Varför Nginx-Dockerfile-utkastet skapades:** För att garantera applikationens portabilitet. Genom att använda `nginx:alpine` uppnås en minimal, säker och blixtsnabb webbserver. Skulle det i framtiden uppstå ett behov att migrera till t.ex. Azure Container Apps för frontenden, är utkastet redan färdigt.
- **Varför hosting sker via Static Web Apps (SWA):** Eftersom frontenden är byggd i Vanilla JavaScript (statiska filer) utan server-side rendering, är SWA det optimala valet. SWA minimerar driftsoverhead – man behöver inte patcha underliggande operativsystem eller konfigurera Nginx i produktion. Dessutom ingår gratis SSL-certifikat, global distribution via CDN och en sömlös CI/CD-upplevelse via GitHub Actions direkt från start.
- **Övervägt alternativ (Vercel):** Initialt övervägdes Vercel på grund av deras fantastiska serverless-abstraktion, vilket helt hade eliminerat behovet av containerhantering för frontenden och gett extrem säkerhet "out-of-the-box" (inga OS-patchar att hantera). Men eftersom detta är ett .NET-projekt där integration med Microsoft-ekosystemet är i fokus (enligt uppgiftsbeskrivningen), föll det slutgiltiga valet på Azure SWA. Azure SWA ger oss samma smidiga developer experience som Vercel, men inom rätt molnmiljö.

</details>

---

## Kommande Sektioner (För Teamet)

*Här förbereder vi strukturen för resterande Epics. När dina kollegor är klara med sina delar kan ni fylla på med detaljer och arkitekturbeslut (ADR) här för att säkerställa att ni uppfyller kraven för G och VG.*

<details>
<summary><b>3. CI/CD Pipeline (Epic 3) - <i>[Kommande]</i></b></summary>

### Vad som har gjorts
- *[Fyll i hur GitHub Actions / pipelinen är uppsatt för bygg och test]*
- *[Fyll i hur deploy sker till Azure]*

### Varför vi gjorde det (ADR & VG-krav)
- **Quality Gates (VG):** *[Förklara hur deploy endast sker om testerna är gröna. Beskriv er spårbarhet, t.ex. hur image-taggning fungerar med commit-SHA.]*

</details>

<details>
<summary><b>4. Säkerhet & Key Vault (Epic 4) - <i>[Kommande]</i></b></summary>

### Vad som har gjorts
- *[Fyll i hur Azure Key Vault integrerats och hur Managed Identity används]*

### Varför vi gjorde det (ADR & VG-krav)
- **Säkerhetsdjup & Hotbild (VG):** *[Beskriv hotbilden (t.ex. läckta nycklar i koden). Förklara "Least Privilege" med RBAC, och varför hemligheter hämtas on-the-fly.]*

</details>

<details>
<summary><b>5. Övervakning & Felsökbarhet (Epic 5) - <i>[Kommande]</i></b></summary>

### Vad som har gjorts
- *[Fyll i hur Application Insights eller Log Analytics är konfigurerat]*

### Varför vi gjorde det (ADR & VG-krav)
- **Observability på riktigt (VG):** *[Visa hur ni mäter/spårar en hel request-kedja (från frontend till backend). Lägg in en kort runbook/instruktion här för hur man felsöker en kraschande applikation.]*

</details>

<details>
<summary><b>6. Fullstack-integration & Säkerhet (Epic 6) - <i>[Kommande]</i></b></summary>

### Vad som har gjorts
- *[Fyll i hur frontend och backend kommunicerar (CORS-konfiguration, Auth-flöden)]*

### Varför vi gjorde det (ADR & VG-krav)
- **Säkerhetsdjup (VG):** *[Förklara varför `AllowAnyOrigin` inte används i produktion. Beskriv eventuella motåtgärder i auth-flödet och hur systemet är resilient (t.ex. med Polly Circuit Breaker).]*

</details>

<details>
<summary><b>7. AI-Integration (Epic 7) - <i>[Kommande]</i></b></summary>

### Vad som har gjorts
- *[Fyll i vilken AI-tjänst som används och vad den gör i appen]*

### Varför vi gjorde det (ADR & VG-krav)
- **Arkitekturbeslut:** *[Förklara varför ni valde denna tjänst och hur felhanteringen ser ut (UX-fallback om AI-tjänsten är nere).]*

</details>
