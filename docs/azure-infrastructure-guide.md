# Azure Cloud Infrastructure Provisioning Guide

Detta dokument beskriver hur man upprättar molninfrastrukturen i Azure för **Lianer fullstack** (Epic 2, Task 5) med hjälp av Azure CLI.

---

## 1. Förutsättningar
- Installerad [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
- Logga in på ditt Azure-konto:
  ```bash
  az login
  ```
- Kontrollera att rätt subscription är vald:
  ```bash
  az account list --output table
  az account set --subscription "<din-subscription-name-eller-id>"
  ```

---

## 2. Infrastruktur-steg (Steg-för-steg)

### Steg 1: Skapa en Resource Group (K-170)
Vi grupperar alla resurser i Västeuropa (`westeurope`) för minimal latens.
```bash
az group create \
  --name rg-lianer-prod \
  --location westeurope
```

### Steg 2: Skapa Azure Container Registry (ACR) (K-171)
Här sparar vi våra Docker-avbildningar för Core API och Features API. Vi använder Basic SKU för att minimera kostnaden.
```bash
az acr create \
  --resource-group rg-lianer-prod \
  --name acrlianerprod \
  --sku Basic \
  --admin-enabled true
```

### Steg 3: Skapa Azure Container Apps Environment (K-172)
Detta är den delade miljön (nätverk, loggning) där våra två backend-mikrotjänster ska köras.
```bash
az containerapp env create \
  --name env-lianer-prod \
  --resource-group rg-lianer-prod \
  --location westeurope
```

### Steg 4: Skapa Azure Static Web App (SWA) för Frontend (K-173)
Sätt upp en Static Web App ansluten till ditt separata frontend-repo på GitHub.
```bash
# Detta kommando skapar resursen och genererar en deployment token för GitHub Actions.
# Byt ut --source mot din frontend-repo URL.
az staticwebapp create \
  --name swa-lianer-frontend \
  --resource-group rg-lianer-prod \
  --source "https://github.com/exikoz/Lianer-frontend" \
  --branch main \
  --location westeurope
```
*Efter att detta körts kommer en pipeline-fil skapas i ditt frontend-repo för automatisk deployment vid varje push till main.*

### Steg 5: Aktivera System-assigned Managed Identity (K-174)
Vi aktiverar en systemspecifik Managed Identity på våra API-tjänster. Det gör att de kan autentisera mot Key Vault utan hårkodade credentials.

*När API-containrarna har deployats till Container Apps körs följande:*
```bash
# Slå på identitet för Core API
az containerapp identity assign \
  --name ca-lianer-core \
  --resource-group rg-lianer-prod \
  --system-assigned

# Slå på identitet för Features API
az containerapp identity assign \
  --name ca-lianer-features \
  --resource-group rg-lianer-prod \
  --system-assigned
```

### Steg 6: Ge behörighet till Azure Key Vault (Minsta behörighet)
Hämta respektive Principal ID för de två applikationerna och tilldela rollen `Key Vault Secrets User` i Azure Key Vault.

```bash
# 1. Hämta Principal ID för Core API
CORE_PRINCIPAL_ID=$(az containerapp show --name ca-lianer-core --resource-group rg-lianer-prod --query "identity.principalId" --output tsv)

# 2. Hämta Principal ID för Features API
FEATURES_PRINCIPAL_ID=$(az containerapp show --name ca-lianer-features --resource-group rg-lianer-prod --query "identity.principalId" --output tsv)

# 3. Ge behörighet (Secrets User) i Key Vault (ersätt <key-vault-name> med ditt KV-namn)
az keyvault set-policy \
  --name kv-lianer-dev \
  --object-id $CORE_PRINCIPAL_ID \
  --secret-permissions get list

az keyvault set-policy \
  --name kv-lianer-dev \
  --object-id $FEATURES_PRINCIPAL_ID \
  --secret-permissions get list
```

### Steg 7: Konfigurera CORS i produktion (Viktigt för VG) (K-175)
Konfigurera CORS på Container Apps så att endast anrop från Static Web Apps-domänen tillåts. Inget `*` (AllowAnyOrigin) får användas i produktion!

```bash
# Hämta den genererade domänen för din Static Web App
SWA_DOMAIN=$(az staticwebapp show --name swa-lianer-frontend --query "defaultHostname" --output tsv)
SWA_URL="https://$SWA_DOMAIN"

# Konfigurera CORS på ca-lianer-core
az containerapp ingress cors update \
  --name ca-lianer-core \
  --resource-group rg-lianer-prod \
  --allowed-origins $SWA_URL \
  --allowed-methods GET POST PUT PATCH DELETE \
  --allowed-headers "*" \
  --allow-credentials true

# Konfigurera CORS på ca-lianer-features
az containerapp ingress cors update \
  --name ca-lianer-features \
  --resource-group rg-lianer-prod \
  --allowed-origins $SWA_URL \
  --allowed-methods GET POST PUT PATCH DELETE \
  --allowed-headers "*" \
  --allow-credentials true
```
