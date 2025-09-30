#!/bin/bash

# BalanceHub Local Development Script
echo "🚀 Starting BalanceHub Local Development Environment"
echo "================================================="

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

# Restore packages
echo "📦 Restoring .NET packages..."
dotnet restore

# Run database migrations
echo "🗄️ Running database migrations..."
dotnet ef database update

# Start backend in background
echo "🌐 Starting backend API server..."
dotnet run &
BACKEND_PID=$!

cd ../..

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

# Wait for Ctrl+C
trap "echo '🛑 Shutting down servers...'; kill $BACKEND_PID $FRONTEND_PID 2>/dev/null; exit 0" INT
wait
