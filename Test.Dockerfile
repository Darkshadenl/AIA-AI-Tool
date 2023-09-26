# Use SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Test

# Copy the entire solution
COPY Core/aia_api.sln Core/
COPY Core/aia_api.csproj Core/
COPY InterfacesAia/interfacesAia.csproj InterfacesAia/
COPY TestProject/TestProject.csproj TestProject/

# Restore packages
RUN dotnet restore Core/aia_api.sln

# Copy all other files
COPY . .

# Optional: If you want to run tests as soon as the container starts
CMD ["sh", "-c", "cd TestProject && dotnet test"]
