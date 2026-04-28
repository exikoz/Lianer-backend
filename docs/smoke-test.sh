#!/bin/bash
# ============================================================
# Lianer Backend — Smoke Test Script
# Runs all endpoints on both APIs to verify they work.
# Requires: both services running (Core: 5297, Features: 5266)
# Usage: bash docs/smoke-test.sh
# ============================================================

CORE="http://localhost:5297/api/v1"
FEATURES="http://localhost:5266/api/v1"
PASS=0
FAIL=0
TOKEN=""
CURL="curl.exe"

check() {
  local description="$1"
  local expected_code="$2"
  local actual_code="$3"
  local body="$4"

  if [ "$actual_code" -eq "$expected_code" ]; then
    echo "  ✅ $description (HTTP $actual_code)"
    PASS=$((PASS + 1))
  else
    echo "  ❌ $description — expected $expected_code, got $actual_code"
    echo "     Response: $body"
    FAIL=$((FAIL + 1))
  fi
}

echo ""
echo "=========================================="
echo " CORE API (localhost:5297)"
echo "=========================================="

# --- USERS ---
echo ""
echo "── Users ──"

# Register
RESPONSE=$($CURL -s -w "\n%{http_code}" -X POST "$CORE/users" \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Smoke Testsson","email":"smoke@test.com","password":"Secure@Password1"}')
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "POST /users (register)" 201 "$CODE" "$BODY"

USER_ID=$(echo "$BODY" | grep -o '"userId":"[^"]*"' | head -1 | cut -d'"' -f4)

# Login
RESPONSE=$($CURL -s -w "\n%{http_code}" -X POST "$CORE/sessions" \
  -H "Content-Type: application/json" \
  -d '{"email":"smoke@test.com","password":"Secure@Password1"}')
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "POST /sessions (login)" 200 "$CODE" "$BODY"

TOKEN=$(echo "$BODY" | grep -o '"accessToken":"[^"]*"' | head -1 | cut -d'"' -f4)

# Get all users
RESPONSE=$($CURL -s -w "\n%{http_code}" "$CORE/users")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /users (list)" 200 "$CODE" "$BODY"

# Get user by ID
RESPONSE=$($CURL -s -w "\n%{http_code}" "$CORE/users/$USER_ID")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /users/{id}" 200 "$CODE" "$BODY"

# Update user
RESPONSE=$($CURL -s -w "\n%{http_code}" -X PUT "$CORE/users/$USER_ID" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{\"id\":\"$USER_ID\",\"fullName\":\"Smoke Updated\",\"email\":\"smoke@test.com\"}")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "PUT /users/{id} (update)" 204 "$CODE" "$BODY"

# Get user not found
RESPONSE=$($CURL -s -w "\n%{http_code}" "$CORE/users/00000000-0000-0000-0000-000000000001")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /users/{id} (not found)" 404 "$CODE" "$BODY"

# Validation error
RESPONSE=$($CURL -s -w "\n%{http_code}" -X POST "$CORE/users" \
  -H "Content-Type: application/json" \
  -d '{"fullName":"","email":"bad","password":"x"}')
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "POST /users (validation error)" 400 "$CODE" "$BODY"

# --- SESSIONS ---
echo ""
echo "── Sessions ──"

# Login with wrong password
RESPONSE=$($CURL -s -w "\n%{http_code}" -X POST "$CORE/sessions" \
  -H "Content-Type: application/json" \
  -d '{"email":"smoke@test.com","password":"WrongPassword1!"}')
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "POST /sessions (wrong password)" 401 "$CODE" "$BODY"

# Google SSO URL
RESPONSE=$($CURL -s -w "\n%{http_code}" "$CORE/sessions/google/url")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /sessions/google/url" 200 "$CODE" "$BODY"

# --- ACTIVITIES ---
echo ""
echo "── Activities ──"

# Create activity
RESPONSE=$($CURL -s -w "\n%{http_code}" -X POST "$CORE/activities" \
  -H "Content-Type: application/json" \
  -d "{\"description\":\"Smoke test activity\",\"assignedTo\":\"$USER_ID\",\"createdBy\":\"$USER_ID\",\"startDate\":\"2026-04-28T00:00:00\",\"endDate\":\"2026-05-28T00:00:00\",\"status\":0}")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "POST /activities (create)" 201 "$CODE" "$BODY"

ACTIVITY_ID=$(echo "$BODY" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

# Get activity by ID
RESPONSE=$($CURL -s -w "\n%{http_code}" "$CORE/activities/$ACTIVITY_ID")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /activities/{id}" 200 "$CODE" "$BODY"

# List activities (paginated)
RESPONSE=$($CURL -s -w "\n%{http_code}" "$CORE/activities?currentPage=1&pageSize=10")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /activities (paginated list)" 200 "$CODE" "$BODY"

# List activities by user
RESPONSE=$($CURL -s -w "\n%{http_code}" "$CORE/activities/user/$USER_ID?currentPage=1&pageSize=10")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /activities/user/{id} (by user)" 200 "$CODE" "$BODY"

# Update activity
RESPONSE=$($CURL -s -w "\n%{http_code}" -X PUT "$CORE/activities" \
  -H "Content-Type: application/json" \
  -d "{\"id\":\"$ACTIVITY_ID\",\"description\":\"Updated activity\",\"assignedTo\":\"$USER_ID\",\"startDate\":\"2026-04-28T00:00:00\",\"endDate\":\"2026-06-28T00:00:00\",\"status\":1}")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "PUT /activities (update)" 200 "$CODE" "$BODY"

# --- NOTES ---
echo ""
echo "── Notes ──"

# Create note
RESPONSE=$($CURL -s -w "\n%{http_code}" -X POST "$CORE/activities/$ACTIVITY_ID/notes" \
  -H "Content-Type: application/json" \
  -d "{\"title\":\"Smoke note\",\"content\":\"Test content\",\"createdBy\":\"$USER_ID\"}")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "POST /activities/{id}/notes (create)" 201 "$CODE" "$BODY"

NOTE_ID=$(echo "$BODY" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

# Get note by ID
RESPONSE=$($CURL -s -w "\n%{http_code}" "$CORE/activities/$ACTIVITY_ID/notes/$NOTE_ID")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /activities/{id}/notes/{noteId}" 200 "$CODE" "$BODY"

# List notes (paginated)
RESPONSE=$($CURL -s -w "\n%{http_code}" "$CORE/activities/$ACTIVITY_ID/notes?currentPage=1&pageSize=10")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /activities/{id}/notes (list)" 200 "$CODE" "$BODY"

# Update note
RESPONSE=$($CURL -s -w "\n%{http_code}" -X PUT "$CORE/activities/$ACTIVITY_ID/notes/$NOTE_ID" \
  -H "Content-Type: application/json" \
  -d "{\"id\":\"$NOTE_ID\",\"title\":\"Updated note\",\"content\":\"Updated content\"}")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "PUT /activities/{id}/notes/{noteId} (update)" 200 "$CODE" "$BODY"

# Delete note
RESPONSE=$($CURL -s -w "\n%{http_code}" -X DELETE "$CORE/activities/$ACTIVITY_ID/notes/$NOTE_ID")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "DELETE /activities/{id}/notes/{noteId}" 204 "$CODE" "$BODY"

# --- CONTACTS ---
echo ""
echo "── Contacts ──"

# Create contact
RESPONSE=$($CURL -s -w "\n%{http_code}" -X POST "$CORE/contacts" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Anna","lastName":"Svensson","role":"Developer","company":"Acme AB","phone":["070-1234567"],"email":["anna@acme.se"],"status":0,"isFavorite":false}')
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "POST /contacts (create)" 201 "$CODE" "$BODY"

CONTACT_ID=$(echo "$BODY" | grep -o '"[0-9a-f-]\{36\}"' | head -1 | tr -d '"')

# Get contact by ID
RESPONSE=$($CURL -s -w "\n%{http_code}" "$CORE/contacts/$CONTACT_ID")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /contacts/{id}" 200 "$CODE" "$BODY"

# Update contact
RESPONSE=$($CURL -s -w "\n%{http_code}" -X PUT "$CORE/contacts/$CONTACT_ID" \
  -H "Content-Type: application/json" \
  -d "{\"id\":\"$CONTACT_ID\",\"firstName\":\"Anna\",\"lastName\":\"Updated\",\"role\":\"Senior Dev\",\"company\":\"Acme AB\",\"phone\":[\"070-1234567\"],\"email\":[\"anna@acme.se\"],\"status\":1,\"isFavorite\":true}")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "PUT /contacts/{id} (update)" 200 "$CODE" "$BODY"

# Delete contact
RESPONSE=$($CURL -s -w "\n%{http_code}" -X DELETE "$CORE/contacts/$CONTACT_ID")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "DELETE /contacts/{id}" 204 "$CODE" "$BODY"

# --- CLEANUP: Delete activity ---
RESPONSE=$($CURL -s -w "\n%{http_code}" -X DELETE "$CORE/activities/$ACTIVITY_ID")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "DELETE /activities/{id} (cleanup)" 204 "$CODE" "$BODY"

# --- CLEANUP: Delete user ---
RESPONSE=$($CURL -s -w "\n%{http_code}" -X DELETE "$CORE/users/$USER_ID" \
  -H "Authorization: Bearer $TOKEN")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "DELETE /users/{id} (cleanup)" 204 "$CODE" "$BODY"

echo ""
echo "=========================================="
echo " FEATURES API (localhost:5266)"
echo "=========================================="

# --- Re-register and login for Features API tests ---
RESPONSE=$($CURL -s -w "\n%{http_code}" -X POST "$CORE/users" \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Features Tester","email":"features@test.com","password":"Secure@Password1"}')
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
F_USER_ID=$(echo "$BODY" | grep -o '"userId":"[^"]*"' | head -1 | cut -d'"' -f4)

RESPONSE=$($CURL -s -w "\n%{http_code}" -X POST "$CORE/sessions" \
  -H "Content-Type: application/json" \
  -d '{"email":"features@test.com","password":"Secure@Password1"}')
BODY=$(echo "$RESPONSE" | head -n -1)
F_TOKEN=$(echo "$BODY" | grep -o '"accessToken":"[^"]*"' | head -1 | cut -d'"' -f4)

echo ""
echo "── Leads ──"

# Enrich domain (Hunter.io)
RESPONSE=$($CURL -s -w "\n%{http_code}" "$FEATURES/leads/enrich/stripe.com")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /leads/enrich/stripe.com (Hunter.io)" 200 "$CODE" "$BODY"

# Import leads
RESPONSE=$($CURL -s -w "\n%{http_code}" -X POST "$FEATURES/leads/import/stripe.com" \
  -H "Authorization: Bearer $F_TOKEN")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "POST /leads/import/stripe.com" 200 "$CODE" "$BODY"

# Get leads (paginated)
RESPONSE=$($CURL -s -w "\n%{http_code}" "$FEATURES/leads?page=1&pageSize=5")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /leads (paginated)" 200 "$CODE" "$BODY"

LEAD_ID=$(echo "$BODY" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

# Get leads with search
RESPONSE=$($CURL -s -w "\n%{http_code}" "$FEATURES/leads?search=stripe")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /leads?search=stripe (filtering)" 200 "$CODE" "$BODY"

# Get leads with sorting
RESPONSE=$($CURL -s -w "\n%{http_code}" "$FEATURES/leads?sortBy=name&sortOrder=asc")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /leads?sortBy=name (sorting)" 200 "$CODE" "$BODY"

# Get lead details
if [ -n "$LEAD_ID" ]; then
  RESPONSE=$($CURL -s -w "\n%{http_code}" "$FEATURES/leads/$LEAD_ID/details")
  BODY=$(echo "$RESPONSE" | head -n -1)
  CODE=$(echo "$RESPONSE" | tail -1)
  check "GET /leads/{id}/details" 200 "$CODE" "$BODY"
fi

# Lead not found
RESPONSE=$($CURL -s -w "\n%{http_code}" "$FEATURES/leads/00000000-0000-0000-0000-000000000001/details")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "GET /leads/{id}/details (not found)" 404 "$CODE" "$BODY"

# Import without auth
RESPONSE=$($CURL -s -w "\n%{http_code}" -X POST "$FEATURES/leads/import/test.com")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "POST /leads/import (no auth)" 401 "$CODE" "$BODY"

# --- CLEANUP ---
RESPONSE=$($CURL -s -w "\n%{http_code}" -X DELETE "$CORE/users/$F_USER_ID" \
  -H "Authorization: Bearer $F_TOKEN")
BODY=$(echo "$RESPONSE" | head -n -1)
CODE=$(echo "$RESPONSE" | tail -1)
check "DELETE /users (cleanup)" 204 "$CODE" "$BODY"

# ============================================================
echo ""
echo "=========================================="
echo " RESULTS: $PASS passed, $FAIL failed"
echo "=========================================="

if [ "$FAIL" -gt 0 ]; then
  exit 1
fi
