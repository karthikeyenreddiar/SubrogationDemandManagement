param appLinkName string
param location string = resourceGroup().location
param skuName string = 'B1'
param appName string
param appSettings array = []


resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: appLinkName
  location: location
  kind: 'linux'
  sku: {
    name: skuName
    tier: 'Basic'
  }
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2021-03-01' = {
  name: appName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|7.0'
      appSettings: appSettings
    }
  }
}

output webAppName string = webApp.name
output webAppHostName string = webApp.properties.defaultHostName
