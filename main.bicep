@description('Container app name for backend')
param containerAppName string = 'balancehub-backend'

// Deploy to existing Container Apps environment
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerAppName
  location: resourceGroup().location
  properties: {
    environmentId: '/subscriptions/2268c6e3-f50f-4cca-a023-9f0ed3838f97/resourceGroups/BalanceHubProd/providers/Microsoft.App/managedEnvironments/balancehub-env'
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 80
        transport: 'auto'
      }
    }
    template: {
      containers: [
        {
          name: 'balancehub-backend'
          image: 'mcr.microsoft.com/dotnet/aspnet:9.0'
          resources: {
            cpu: 0.5
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 5
      }
    }
  }
}
