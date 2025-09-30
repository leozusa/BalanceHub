# 🚀 BalanceHub Deployment & Testing Guide

This guide explains how to test BalanceHub locally and deploy it to the internet using the provided automation scripts.

## 📋 Prerequisites

Before running any scripts, ensure you have the following installed:

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js & npm** - [Download here](https://nodejs.org/)
- **Azure CLI** (for web deployment) - [Install here](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- **Git** - Should already be installed

## 🏠 Local Testing

### Quick Start with Automation Script

```bash
# Make sure you're in the project root directory
pwd  # Should show: /Users/leozusa/Documents/GitHub/BalanceHub

# Run the local development script
./run_local.sh
```

**What this script does:**
- ✅ Checks all prerequisites
- 🔧 Sets up the backend (.NET API)
- 🎨 Sets up the frontend (Angular app)
- 🚀 Starts both servers automatically
- 📱 Opens your browser (optional)

**Expected Output:**
```
🎉 BalanceHub is now running locally!
=====================================
🔗 Frontend: http://localhost:4200
🔗 Backend API: http://localhost:5234
🔗 API Documentation: http://localhost:5234/swagger

📱 Open http://localhost:4200 in your browser to access the application
```

### Manual Setup (Alternative)

If you prefer to run everything manually:

```bash
# Backend Setup
cd backend/BalanceHub.API
dotnet restore
dotnet ef database update
dotnet run

# Frontend Setup (in another terminal)
cd frontend/balancehub-frontend
npm install
npm start
```

## 🌐 Web Deployment

### Quick Deploy with Automation Script

```bash
# Deploy to Azure (make sure you're logged in first)
./run_web.sh
```

**Before running the deployment script:**

1. **Login to Azure:**
   ```bash
   az login
   ```

2. **Update the script configuration** (optional):
   Edit `run_web.sh` and modify these variables if needed:
   ```bash
   RESOURCE_GROUP="BalanceHub-RG"
   BACKEND_APP_NAME="balancehub-api"
   FRONTEND_APP_NAME="balancehub-frontend"
   LOCATION="East US"
   ```

**What the deployment script does:**
- ✅ Validates Azure login and prerequisites
- 🏗️ Creates Azure resource group
- 🔧 Builds and deploys backend API to Azure Web App
- 🎨 Builds and deploys frontend to Azure Static Web Apps
- 🔗 Automatically updates API URLs for production
- 🌐 Provides live URLs for your deployed application

**Expected Output:**
```
🎉 Deployment Complete!
======================
🔗 Frontend URL: https://balancehub-frontend.azurestaticapps.net
🔗 Backend API: https://balancehub-api.azurewebsites.net
🔗 API Documentation: https://balancehub-api.azurewebsites.net/api/swagger

📱 Your BalanceHub application is now live!
🌐 Access it at: https://balancehub-frontend.azurestaticapps.net
```

## 🧪 Testing Your Application

### Local Testing

1. **Start the application:**
   ```bash
   ./run_local.sh
   ```

2. **Open your browser** and go to: http://localhost:4200

3. **Test the login functionality:**
   - The application will show a login page
   - You'll need to create user accounts first (database is created automatically)
   - Test both Employee and Manager roles

4. **API Testing:**
   - API Documentation: http://localhost:5234/swagger
   - Health Check: http://localhost:5234/api/health

### Production Testing

1. **After deployment, test your live application:**
   - Frontend: `https://your-frontend-app.azurestaticapps.net`
   - Backend API: `https://your-backend-app.azurewebsites.net`
   - API Docs: `https://your-backend-app.azurewebsites.net/api/swagger`

2. **Useful Azure Commands:**
   ```bash
   # Check backend logs
   az webapp log tail --name your-backend-app --resource-group BalanceHub-RG

   # Check frontend logs
   az staticwebapp logs --name your-frontend-app --resource-group BalanceHub-RG

   # Restart backend if needed
   az webapp restart --name your-backend-app --resource-group BalanceHub-RG
   ```

## 🔧 Troubleshooting

### Common Issues

**1. Port Conflicts:**
- Make sure ports 4200 (frontend) and 5234 (backend) are available
- Check if other applications are using these ports

**2. Database Issues:**
- Delete `backend/BalanceHub.API/BalanceHub.db` if you need a fresh database
- The database will be recreated automatically

**3. Azure Deployment Issues:**
- Ensure you're logged in: `az login`
- Check your subscription: `az account show`
- Verify resource group exists: `az group list`

**4. Build Issues:**
- Clear npm cache: `npm cache clean --force`
- Clear .NET cache: `dotnet nuget locals all --clear`

### Getting Help

If you encounter issues:

1. **Check the logs** - Both scripts provide detailed output
2. **Verify prerequisites** - Run the scripts to see what's missing
3. **Check Azure status** - Visit [Azure Status](https://status.azure.com/)

## 📁 Project Structure

```
BalanceHub/
├── run_local.sh          # Local development script
├── run_web.sh           # Web deployment script
├── backend/
│   └── BalanceHub.API/  # .NET API backend
├── frontend/
│   └── balancehub-frontend/  # Angular frontend
└── DEPLOYMENT_README.md # This file
```

## 🚀 Next Steps

After successful deployment:

1. **Set up user accounts** in your production database
2. **Configure authentication** for your specific needs
3. **Set up monitoring** with Azure Application Insights
4. **Configure custom domains** (optional)
5. **Set up CI/CD pipelines** for automated deployments

---

**Happy coding! 🎉**
