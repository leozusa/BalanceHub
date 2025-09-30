#!/bin/bash

# BalanceHub Local Development Script
echo "🚀 Starting BalanceHub Local Development Environment"
echo "================================================="

# Clean up any existing servers
echo "🧹 Killing any existing servers..."
pkill -f "dotnet.*BalanceHub.API" 2>/dev/null || true
pkill -f "ng serve" 2>/dev/null || true
sleep 2

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
echo "📋 Checking prerequisites..."

if ! command_exists dotnet; then
    echo "❌ .NET SDK is not installed. Please install it from https://dotnet.microsoft.com/download"
    exit 1
fi

if ! command_exists npm; then
    echo "❌ Node.js/npm is not installed. Please install it from https://nodejs.org/"
    exit 1
fi

echo "✅ Prerequisites check passed"

# Setup backend
echo ""
echo "🔧 Setting up backend..."
cd backend/BalanceHub.API

# Remove existing database to start fresh
echo "🗄️ Preparing fresh database..."
rm -f BalanceHub.db

# Restore packages
echo "📦 Restoring .NET packages..."
dotnet restore

# Build the application first
echo "🔨 Building backend..."
dotnet build

# Start backend in background
echo "🌐 Starting backend API server..."
dotnet run &
BACKEND_PID=$!

cd ../..

# Wait a moment for backend to start
echo "⏳ Waiting for backend to start..."
sleep 5

# Initialize database with test users
echo ""
echo "📊 Setting up database with test users..."
curl -X POST http://localhost:5234/api/database/initialize --silent --output /dev/null || echo "⚠️ Database initialization failed or already initialized"

# Setup frontend
echo ""
echo "⚡ Setting up frontend..."
cd frontend/balancehub-frontend

# Install dependencies
echo "📦 Installing npm packages..."
npm install

# Start frontend development server
echo "🎨 Starting frontend development server..."
npm start &
FRONTEND_PID=$!

cd ../..

echo ""
echo "🎉 BalanceHub is now running locally!"
echo "====================================="
echo "🔗 Frontend: http://localhost:4200"
echo "🔗 Backend API: http://localhost:5234"
echo "🔗 API Documentation: http://localhost:5234/swagger"
echo ""
echo "📱 Open http://localhost:4200 in your browser to access the application"
echo ""
echo "🛑 To stop both servers, press Ctrl+C"
echo ""
echo "📋 Test login credentials:"
echo "   john.doe@example.com / test123 (Employee)"
echo "   sarah.smith@example.com / test123 (Manager)"
echo "   alex.jones@example.com / test123 (Employee)"

# Wait for Ctrl+C
trap "echo '🛑 Shutting down servers...'; kill $BACKEND_PID $FRONTEND_PID 2>/dev/null; exit 0" INT
wait
