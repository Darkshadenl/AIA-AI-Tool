# Use SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the entire solution
COPY Core/aia_api.sln Core/
COPY Core/aia_api.csproj Core/
COPY InterfacesAia/interfacesAia.csproj InterfacesAia/
COPY TestProject/TestProject.csproj TestProject/

# Keep container running
CMD ["tail", "-f", "/dev/null"]
