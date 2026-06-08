@description('The name of the Key Vault')
param keyVaultName string

@description('The principal ID of the Managed Identity (e.g. from the Container App)')
param principalId string

@description('The Principal Type (ServicePrincipal for Managed Identity, User, or Group)')
@allowed([
  'ServicePrincipal'
  'User'
  'Group'
])
param principalType string = 'ServicePrincipal'

// Referens till det existerande (eller nyskapade) Key Vaultet
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Key Vault Secrets User Role ID i Azure
var keyVaultSecretsUserRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')

// Ge identiteten rättighet att läsa secrets
resource secretReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, principalId, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: keyVaultSecretsUserRoleId
    principalId: principalId
    principalType: principalType
  }
}
