# Use SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy the entire solution
COPY Core/aia_api.sln Core/
COPY Core/aia_api.csproj Core/
COPY InterfacesAia/interfacesAia.csproj InterfacesAia/
COPY TestProject/TestProject.csproj TestProject/

# Restore packages
RUN dotnet restore Core/aia_api.sln

# Copy all other files
COPY . .

# Build the project
WORKDIR /app/Core
RUN dotnet build -c Release -o /app/publish

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
ENV DOTNET_USE_POLLING_FILE_WATCHER 1
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "aia_api.dll", "--urls", "http://+:5000"]


