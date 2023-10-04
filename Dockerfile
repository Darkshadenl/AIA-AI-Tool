# Common build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY Core/aia_api.sln Core/
COPY Core/AiaApi.csproj Core/
COPY InterfacesAia/InterfacesAia.csproj InterfacesAia/
COPY TestProject/TestProject.csproj TestProject/
RUN dotnet restore Core/aia_api.sln
COPY . .

# Test stage
FROM build AS test
ENV ASPNETCORE_ENVIRONMENT=Test
ENV Logging__LogLevel__Default=Debug
CMD ["sh", "-c", "cd TestProject && dotnet test --logger \"console;verbosity=detailed\""]



# Debug stage
FROM build AS debug
WORKDIR /app/Core
EXPOSE 80
EXPOSE 443

RUN apt-get update
RUN apt-get install -y unzip
RUN curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l ~/vsdbg

RUN dotnet build AiaApi.csproj -c Release -o /app
ENTRYPOINT ["dotnet", "AiaApi.dll"]




# Development stage
FROM build AS dev
ENV ASPNETCORE_ENVIRONMENT=Development
WORKDIR /app/Core
ENTRYPOINT ["dotnet", "watch", "run", "--urls=http://+:5000", "--configuration", "Debug"]

# Production stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "aia_api.dll"]
