using Azure.Identity;

public static class KeyVaultExtensions
{
    public static WebApplicationBuilder SetupAzureKeyVault(this WebApplicationBuilder builder)
    {
            // --- Azure Key Vault (Always active) ---
            var vaultUri = builder.Configuration["AzureKeyVault:VaultUri"];
            if (!string.IsNullOrEmpty(vaultUri))
            {
                try
                {
                    builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
                    Console.WriteLine("Core API: Key Vault connection initialized to: " + vaultUri);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Core API: Key Vault connection failed. Falling back to local secrets. (Error: {ex.Message})");
                }
            }
            return builder;
    }
}