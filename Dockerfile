# Use Microsoft .NET 9.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy and restore dependencies
COPY *.sln ./
COPY backend/BalanceHub.API/BalanceHub.API.csproj ./backend/BalanceHub.API/
RUN dotnet restore ./backend/BalanceHub.API/BalanceHub.API.csproj

# Copy source code
COPY backend/ ./backend/
WORKDIR /app/backend/BalanceHub.API

# Build the application
RUN dotnet publish -c Release -o /app/out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose port 80 for Railway
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Run the application
ENTRYPOINT ["dotnet", "BalanceHub.API.dll"]
