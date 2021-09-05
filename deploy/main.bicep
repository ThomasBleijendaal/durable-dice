param functionAppName string = 'durabledice'

var https = 'https://'

resource storage 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: 'durabledice'
  location: resourceGroup().location
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
        'publicAccess': 'Blob'
      }
    }
  }
}

resource hostingplan 'Microsoft.Web/serverfarms@2021-01-15' = {
  name: 'durabledice-asp'
  location: resourceGroup().location
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
  location: resourceGroup().location
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
          urlTemplate: 'https://${functionAppName}.azurewebsites.net/runtime/webhooks/signalr'
        }
      ]
    }
  }
}

resource functionapp 'Microsoft.Web/sites@2021-01-15' = {
  name: functionAppName
  location: resourceGroup().location
  kind: 'functionapp'
  dependsOn: [
    storage
    hostingplan
    signalr
  ]
  properties: {
    serverFarmId: hostingplan.id
    siteConfig: {
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
      // AzureSignalRConnectionString: 'Endpoint=${https}${signalr.name}.service.signalr.net;AccessKey=${signalr.listKeys().primaryKey.value};Version=1.0;'
      AzureSignalRServiceTransportType: 'Transient'
      FUNCTIONS_EXTENSION_VERSION: '~3'
      WEBSITE_RUN_FROM_PACKAGE: '1'
      WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${storage.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
      WEBSITE_CONTENTSHARE: storage.name
    }
  }
}
