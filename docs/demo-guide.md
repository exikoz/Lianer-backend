# Lianer Backend — Demo Guide

Core API: `http://localhost:5297`
Features API: `http://localhost:5266`
Scalar Docs: `http://localhost:5297/scalar/v1` / `http://localhost:5266/scalar/v1`

---

## 1. Registrera en användare (Core API)

**POST** `http://localhost:5297/api/v1/users`

```json
{
  "fullName": "Demo Testsson",
  "email": "demo@example.com",
  "password": "Secure@Password1"
}
```

Förväntat svar: `201 Created`
```json
{
  "userId": "...",
  "fullName": "Demo Testsson",
  "email": "demo@example.com",
  "createdAt": "..."
}
```

---

## 2. Logga in (Core API)

**POST** `http://localhost:5297/api/v1/sessions`

```json
{
  "email": "demo@example.com",
  "password": "Secure@Password1"
}
```

Förväntat svar: `200 OK`
```json
{
  "accessToken": "eyJhbG...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "userId": "...",
    "fullName": "Demo Testsson",
    "email": "demo@example.com"
  }
}
```

> Kopiera `accessToken` och klistra in den i Scalars "BearerAuth" fält (lås-ikonen). Scalar skickar sedan Authorization-headern automatiskt på alla skyddade endpoints.
>
> Samma token fungerar på båda API:erna. Klistra in den i BearerAuth på både Core API (`localhost:5297/scalar/v1`) och Features API (`localhost:5266/scalar/v1`).

---

## 3. Hämta alla användare (Core API)

**GET** `http://localhost:5297/api/v1/users`

Förväntat svar: `200 OK` — lista med användare. Kör igen för att visa caching (snabbare svar).

---

## 4. Hämta en specifik användare (Core API)

**GET** `http://localhost:5297/api/v1/users/{userId}`

Använd userId från steg 1. Förväntat: `200 OK` med användardata.

Testa med ett fejk-ID: `http://localhost:5297/api/v1/users/00000000-0000-0000-0000-000000000000`

Förväntat: `404 Not Found` med ProblemDetails JSON.

---

## 5. Uppdatera användare (Core API, kräver JWT)

**PUT** `http://localhost:5297/api/v1/users/{userId}`

Kräver inloggning (token sätts automatiskt via Scalar efter steg 2).

```json
{
  "id": "{userId}",
  "fullName": "Demo Uppdaterad",
  "email": "demo@example.com"
}
```

Förväntat: `204 No Content`

Verifiera ändringen:

**GET** `http://localhost:5297/api/v1/users/{userId}`

Förväntat: `200 OK` — `fullName` ska nu vara `"Demo Uppdaterad"`.

---

## 6. Validering — skicka ogiltig data (Core API)

**POST** `http://localhost:5297/api/v1/users`

```json
{
  "fullName": "",
  "email": "inte-en-email",
  "password": "kort"
}
```

Förväntat: `400 Bad Request` med ValidationProblemDetails (visar custom filter).

---

## 7. Google SSO — hämta login-URL (Core API)

**GET** `http://localhost:5297/api/v1/sessions/google/url`

Förväntat: `200 OK` med Google OAuth URL. Öppna URL:en i webbläsaren för att visa att Google-inloggningssidan laddas.

> Fullständigt SSO-flöde kräver en frontend som tar emot callback. För demo: visa att URL:en genereras korrekt och förklara OAuth2-flödet.

---

## 8. Hunter.io — berika en domän (Features API)

**GET** `http://localhost:5266/api/v1/leads/enrich/sogeti.se`

Förväntat: `200 OK` med organisation och e-postlista från Hunter.io.

---

## 9. Importera leads från Hunter.io (Features API, kräver JWT)

**POST** `http://localhost:5266/api/v1/leads/import/sogeti.se`

Kräver inloggning (token sätts automatiskt via Scalar efter steg 2).

Förväntat: `200 OK`
```json
{
  "message": "Import completed for sogeti.se",
  "totalFound": 10,
  "imported": 10,
  "duplicatesSkipped": 0
}
```

---

## 10. Hämta leads med paginering och sökning (Features API)

**GET** `http://localhost:5266/api/v1/leads?page=1&pageSize=5`

Förväntat: `200 OK` med paginerad data:
```json
{
  "data": [...],
  "page": 1,
  "pageSize": 5,
  "totalCount": 10,
  "totalPages": 2
}
```

Testa sökning: `http://localhost:5266/api/v1/leads?search=sogeti`

Testa sortering: `http://localhost:5266/api/v1/leads?sortBy=name&sortOrder=asc`

---

## 11. Microservice-kommunikation — lead details (Features API)

**GET** `http://localhost:5266/api/v1/leads/{leadId}/details`

Använd ett leadId från steg 10. Features API anropar Core API internt för att hämta användarnamn.

Förväntat: `200 OK` med lead + assignedToName.

---

## 12. Rate Limiting — testa överbelastning

Skicka samma GET-request snabbt 100+ gånger.

Förväntat: `429 Too Many Requests` efter 100 anrop inom 1 minut.

---

## 13. Felhantering — ProblemDetails (båda API:er)

Hämta en användare som inte finns:

**GET** `http://localhost:5297/api/v1/users/00000000-0000-0000-0000-000000000001`

Förväntat: `404 Not Found` med RFC 7807 ProblemDetails:
```json
{
  "status": 404,
  "title": "Not Found",
  "detail": "...",
  "instance": "/api/v1/users/00000000-0000-0000-0000-000000000001",
  "type": "https://httpstatuses.com/404",
  "traceId": "..."
}
```

---

## 14. Utan JWT — skyddade endpoints

Ta bort token från BearerAuth-fältet i Features API Scalar (eller öppna ett nytt inkognito-fönster).

**POST** `http://localhost:5266/api/v1/leads/import/stripe.com`

Förväntat: `401 Unauthorized` — visar att endpoints är skyddade utan giltig token.

---

## 15. Kör testerna

```bash
dotnet test --verbosity normal
```

Förväntat: 31 tester passerar (unit tests med Moq + integration tests med WebApplicationFactory).

---

## Snabb checklista för redovisningen

- [ ] Båda API:er körs (Core: 5297, Features: 5266)
- [ ] Registrera + logga in → JWT token
- [ ] CRUD på users (GET, POST, PUT, DELETE)
- [ ] Validering → 400 med felmeddelanden
- [ ] Hunter.io enrichment + import
- [ ] Paginering + sökning på leads
- [ ] Microservice-kommunikation (leads → users)
- [ ] Rate limiting (429)
- [ ] ProblemDetails felhantering
- [ ] Auth-skyddade endpoints (401 utan token)
- [ ] Scalar API-dokumentation
- [ ] Tester passerar
