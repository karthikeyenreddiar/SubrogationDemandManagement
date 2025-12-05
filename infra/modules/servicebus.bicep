param serviceBusName string
param location string = resourceGroup().location
param skuName string = 'Standard'

resource serviceBus 'Microsoft.ServiceBus/namespaces@2021-11-01' = {
  name: serviceBusName
  location: location
  sku: {
    name: skuName
    tier: skuName
  }
}

resource pdfQueue 'Microsoft.ServiceBus/namespaces/queues@2021-11-01' = {
  parent: serviceBus
  name: 'pdf-generation'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT5M'
  }
}

resource emailQueue 'Microsoft.ServiceBus/namespaces/queues@2021-11-01' = {
  parent: serviceBus
  name: 'email-delivery'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT5M'
  }
}

// Authorization Rule for Listen/Send
resource authRule 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2021-11-01' = {
  parent: serviceBus
  name: 'RootManageSharedAccessKey'
  properties: {
    rights: [
      'Listen'
      'Manage'
      'Send'
    ]
  }
}

output serviceBusId string = serviceBus.id
output serviceBusName string = serviceBus.name
output connectionString string = listKeys(authRule.id, authRule.apiVersion).primaryConnectionString
