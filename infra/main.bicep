param appName string = 'subrogationsaas'
param environment string = 'dev'
@secure()
param sqlAdminPassword string
param location string = resourceGroup().location

var suffix = uniqueString(resourceGroup().id)
var storageName = '${appName}st${suffix}'
var sbName = '${appName}-sb-${environment}-${suffix}'
var sqlServerName = '${appName}-sql-${environment}-${suffix}'
var dbName = 'SubrogationDemandManagement'
var appPlanName = '${appName}-plan-${environment}'
var apiAppName = '${appName}-api-${environment}-${suffix}'
var funcAppName = '${appName}-func-${environment}-${suffix}'

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    storageName: storageName
    location: location
  }
}

module serviceBus 'modules/servicebus.bicep' = {
  name: 'serviceBus'
  params: {
    serviceBusName: sbName
    location: location
  }
}

module sql 'modules/sql.bicep' = {
  name: 'sql'
  params: {
    serverName: sqlServerName
    databaseName: dbName
    location: location
    adminLogin: 'sqladmin'
    adminPassword: sqlAdminPassword
  }
}

module api 'modules/appservice.bicep' = {
  name: 'api'
  params: {
    appLinkName: appPlanName
    appName: apiAppName
    location: location
    appSettings: [
      {
        name: 'ConnectionStrings:DefaultConnection'
        value: sql.outputs.connectionString
      }
      {
        name: 'ServiceBus:ConnectionString'
        value: serviceBus.outputs.connectionString
      }
      {
        name: 'BlobStorage:ConnectionString'
        value: storage.outputs.connectionString
      }
      {
        name: 'AzureAdB2C:Instance'
        value: 'https://subrogationsaas.b2clogin.com'
      }
      {
        name: 'AzureAdB2C:ClientId'
        value: 'placeholder-client-id'
      }
      {
        name: 'AzureAdB2C:Domain'
        value: 'subrogationsaas.onmicrosoft.com'
      }
    ]
  }
}

module func 'modules/function.bicep' = {
  name: 'func'
  params: {
    functionAppName: funcAppName
    storageAccountName: storage.outputs.storageName
    location: location
    appSettings: [
      {
        name: 'ConnectionStrings:DefaultConnection'
        value: sql.outputs.connectionString
      }
      {
        name: 'ServiceBusConnection'
        value: serviceBus.outputs.connectionString
      }
      {
        name: 'BlobStorage:ConnectionString'
        value: storage.outputs.connectionString
      }
    ]
  }
}

output apiUrl string = 'https://${api.outputs.webAppHostName}'
output apiAppName string = api.outputs.webAppName
output funcAppName string = func.outputs.functionAppName
