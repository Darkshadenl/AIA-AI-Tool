FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Test
ENV Logging__LogLevel__Default=Debug 

COPY Core/aia_api.sln Core/
COPY Core/aia_api.csproj Core/
COPY InterfacesAia/interfacesAia.csproj InterfacesAia/
COPY TestProject/TestProject.csproj TestProject/

# Restore packages
RUN dotnet restore Core/aia_api.sln

COPY . .

CMD ["sh", "-c", "cd TestProject && dotnet test --logger \"console;verbosity=detailed\""]

