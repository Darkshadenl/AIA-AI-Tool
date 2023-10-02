# Use SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS dev
WORKDIR /app

# Copy csproj files and restore dependencies
COPY Core/aia_api.sln Core/
COPY Core/AiaApi.csproj Core/
COPY InterfacesAia/InterfacesAia.csproj InterfacesAia/
COPY TestProject/TestProject.csproj TestProject/

WORKDIR /app/Core
RUN dotnet restore aia_api.sln

# Copy all other files
WORKDIR /app
COPY . .

# Switch to the Core directory
WORKDIR /app/Core

# Set environment to Development
ENV ASPNETCORE_ENVIRONMENT=Development

# Run dotnet watch
ENTRYPOINT dotnet watch run --urls=http://+:5000
