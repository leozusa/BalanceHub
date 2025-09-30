#!/bin/bash

# BalanceHub Web Deployment Script
echo "🚀 Starting BalanceHub Web Deployment"
echo "==================================="

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
echo "📋 Checking prerequisites..."

if ! command_exists az; then
    echo "❌ Azure CLI is not installed. Please install it from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

if ! command_exists git; then
    echo "❌ Git is not installed. Please install it first."
    exit 1
fi

echo "✅ Prerequisites check passed"

# Configuration - Update these values for your deployment
RESOURCE_GROUP="BalanceHubProd"
BACKEND_APP_NAME="balancehub-api"
FRONTEND_APP_NAME="balancehub-frontend"
LOCATION="East US"

echo ""
echo "🔧 Configuration:"
echo "   Resource Group: $RESOURCE_GROUP"
echo "   Backend App: $BACKEND_APP_NAME"
echo "   Frontend App: $FRONTEND_APP_NAME"
echo "   Location: $LOCATION"
echo ""

# Check if user is logged in to Azure
echo "🔐 Checking Azure login status..."
if ! az account show >/dev/null 2>&1; then
    echo "❌ You are not logged in to Azure. Please run: az login"
    exit 1
fi

echo "✅ Azure login confirmed"

# Create resource group if it doesn't exist
echo ""
echo "🏗️ Creating resource group if needed..."
az group create --name $RESOURCE_GROUP --location "$LOCATION" --output table

# Deploy Backend (C# API)
echo ""
echo "🔧 Deploying backend API..."
cd backend/BalanceHub.API

# Build and publish backend
echo "📦 Building backend application..."
dotnet publish -c Release -o publish

# Deploy to Azure Web App
echo "🚀 Deploying to Azure Web App..."
az webapp up \
    --name $BACKEND_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --location "$LOCATION" \
    --output table

BACKEND_URL="https://$BACKEND_APP_NAME.azurewebsites.net"
echo "✅ Backend deployed to: $BACKEND_URL"

cd ../..

# Deploy Frontend (Angular SPA)
echo ""
echo "🎨 Deploying frontend application..."
cd frontend/balancehub-frontend

# Build frontend for production
echo "📦 Building frontend for production..."
npm run build

# Update API URL for production
API_CONFIG_FILE="src/environments/environment.prod.ts"
if [ -f "$API_CONFIG_FILE" ]; then
    echo "🔗 Updating API URL to point to deployed backend..."
    sed -i.bak "s|apiUrl: '.*'|apiUrl: '$BACKEND_URL/api'|g" "$API_CONFIG_FILE"
fi

# Deploy to Azure Static Web Apps
echo "🚀 Deploying to Azure Static Web Apps..."
az staticwebapp up \
    --name $FRONTEND_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --source . \
    --app-location "." \
    --api-location "../backend/BalanceHub.API" \
    --output-location "dist/balancehub-frontend" \
    --token $(az staticwebapp secrets list --name $FRONTEND_APP_NAME --resource-group $RESOURCE_GROUP --query "properties.apiKey" -o tsv 2>/dev/null || echo "") \
    --output table

FRONTEND_URL="https://$FRONTEND_APP_NAME.azurestaticapps.net"
echo "✅ Frontend deployed to: $FRONTEND_URL"

cd ../..

# Display deployment results
echo ""
echo "🎉 Deployment Complete!"
echo "======================"
echo "🔗 Frontend URL: $FRONTEND_URL"
echo "🔗 Backend API: $BACKEND_URL"
echo "🔗 API Documentation: $BACKEND_URL/api/swagger"
echo ""
echo "📱 Your BalanceHub application is now live!"
echo "🌐 Access it at: $FRONTEND_URL"
echo ""
echo "🔧 Useful commands:"
echo "   - View backend logs: az webapp log tail --name $BACKEND_APP_NAME --resource-group $RESOURCE_GROUP"
echo "   - View frontend logs: az staticwebapp logs --name $FRONTEND_APP_NAME --resource-group $RESOURCE_GROUP"
echo "   - Restart backend: az webapp restart --name $BACKEND_APP_NAME --resource-group $RESOURCE_GROUP"
echo ""

# Optional: Open the deployed application
read -p "🌐 Would you like to open the deployed application in your browser? (y/n): " -n 1 -r
echo ""
if [[ $REPLY =~ ^[Yy]$ ]]; then
    if command_exists open; then
        open "$FRONTEND_URL"
    elif command_exists xdg-open; then
        xdg-open "$FRONTEND_URL"
    else
        echo "Please open $FRONTEND_URL in your browser manually."
    fi
fi

echo "✅ Deployment script completed successfully!"
