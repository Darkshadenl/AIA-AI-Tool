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
RUN dotnet build -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /final
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80
ENTRYPOINT ["dotnet", "aia_api.dll"]

