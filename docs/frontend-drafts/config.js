// Konfigurationsfil för frontend API-URL:er.
// Denna fil bör laddas in i index.html med: <script src="config.js"></script>
// innan andra skript som gör API-anrop körs.
window.ENV = {
  // Lokala standardportar vid körning via Docker Compose:
  CORE_API_URL: window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
    ? 'http://localhost:5000'
    : 'https://<din-core-api-url-i-azure>.azurecontainerapps.io',

  FEATURES_API_URL: window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
    ? 'http://localhost:5001'
    : 'https://<din-features-api-url-i-azure>.azurecontainerapps.io'
};
