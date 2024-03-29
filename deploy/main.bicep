param functionAppName string = 'durabledice'
param functionAppKey string 

param location string = resourceGroup().location

resource storage 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: 'durabledice'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'

  resource blob 'blobServices' = {
    name: 'default'

    properties: {
      cors: {
        corsRules: [
          {
            allowedHeaders: [
              '*'
            ]
            allowedMethods: [
              'GET'
            ]
            allowedOrigins: [
              '*'
            ]
            exposedHeaders: []
            maxAgeInSeconds: 3600
          }
        ]
      }
    }

    resource container 'containers' = {
      name: 'site'
      properties: {
        publicAccess: 'Blob'
      }
    }
  }

  resource tables 'tableServices' = {
    name: 'default'

    resource historyTable 'tables' = {
      name: 'history'
    }
  }
}

resource hostingplan 'Microsoft.Web/serverfarms@2021-01-15' = {
  name: 'durabledice-asp'
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
    size: 'Y1'
    family: 'Y'
  }
  dependsOn: [
    storage
  ]
}

resource signalr 'Microsoft.SignalRService/signalR@2021-06-01-preview' = {
  name: 'durabledicer'
  location: location
  sku: {
    capacity: 1
    name: 'Free_F1'
    tier: 'Free'
  }
  kind: 'SignalR'
  properties: {
    cors: {
      allowedOrigins: [
        '*' // TODO: tune this down
      ]
    }
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
    ]
    networkACLs: {
      defaultAction: 'Deny'
      publicNetwork: {
        allow: [
          'ServerConnection'
          'ClientConnection'
          'RESTAPI'
          'Trace'
        ]
      }
    }
    upstream: {
      templates: [
        {
          auth: {
            type: 'None'
          }
          hubPattern: '*'
          categoryPattern: '*'
          eventPattern: '*'
          urlTemplate: 'https://${functionAppName}.azurewebsites.net/runtime/webhooks/signalr?code=${functionAppKey}'
        }
      ]
    }
  }
}

resource functionapp 'Microsoft.Web/sites@2021-01-15' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  dependsOn: [
    storage
    hostingplan
    signalr
  ]
  properties: {
    serverFarmId: hostingplan.id
    siteConfig: {
      netFrameworkVersion: 'v6.0'
      cors: {
        allowedOrigins: [
          '*' // TODO: tune this down
        ]
      }
    }
  }
  resource appsettings 'config' = {
    name: 'appsettings'
    properties: {
      AzureWebJobsStorage: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${storage.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
      AzureSignalRConnectionString: signalr.listKeys().primaryConnectionString
      AzureSignalRServiceTransportType: 'Transient'
      FUNCTIONS_EXTENSION_VERSION: '~4'
      FUNCTIONS_WORKER_RUNTIME: 'dotnet'
      WEBSITE_RUN_FROM_PACKAGE: '1'
      WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${storage.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
      WEBSITE_CONTENTSHARE: storage.name
    }
  }
}
