using Azure.Identity;

public static class KeyVaultExtensions
{
    public static WebApplicationBuilder SetupAzureKeyVault(this WebApplicationBuilder builder)
    {
            // --- Azure Key Vault (Always active) ---
            var vaultUri = builder.Configuration["AzureKeyVault:VaultUri"];
            if (!string.IsNullOrEmpty(vaultUri))
            {
                builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
                Console.WriteLine("Core API: Key Vault connection initialized to: " + vaultUri);
            }
            return builder;
    }
}