# Common build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
EXPOSE 80
EXPOSE 443

WORKDIR /app

COPY ./aia_api.sln .
COPY ./Core/AiaApi.csproj Core/
COPY ./InterfacesAia/InterfacesAia.csproj InterfacesAia/
COPY ./TestProject/TestProject.csproj TestProject/
RUN dotnet restore
COPY . .
RUN dotnet build --configuration Debug --no-restore

ENV Logging__LogLevel__Default=Debug
ENV ASPNETCORE_ENVIRONMENT=Development

# Test stage
FROM build AS test
CMD ["sh", "-c", "cd TestProject && dotnet test --logger \"console;verbosity=detailed\""]

# Development stage
FROM build AS dev
WORKDIR /app
ENTRYPOINT ["dotnet", "watch", "--project", "Core", "run", "--urls=http://+:5000", "--configuration", "Debug"]
CMD ["tail", "-f", "/dev/null"]

# Production stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS production
COPY --from=publish /app/publish ./app
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Logging__LogLevel__Default=Information

CMD ["dotnet", "/app/AiaApi.dll"]
