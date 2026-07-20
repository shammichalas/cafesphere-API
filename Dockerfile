# Multi-stage Dockerfile for CafeSphere ASP.NET Core 9 Web API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy Solution & project files for layer caching
COPY CafeSphere.sln ./
COPY src/CafeSphere.Domain/CafeSphere.Domain.csproj src/CafeSphere.Domain/
COPY src/CafeSphere.Shared/CafeSphere.Shared.csproj src/CafeSphere.Shared/
COPY src/CafeSphere.Application/CafeSphere.Application.csproj src/CafeSphere.Application/
COPY src/CafeSphere.Persistence/CafeSphere.Persistence.csproj src/CafeSphere.Persistence/
COPY src/CafeSphere.Infrastructure/CafeSphere.Infrastructure.csproj src/CafeSphere.Infrastructure/
COPY src/CafeSphere.API/CafeSphere.API.csproj src/CafeSphere.API/
COPY tests/CafeSphere.Tests/CafeSphere.Tests.csproj tests/CafeSphere.Tests/

RUN dotnet restore

# Copy all source files and build
COPY . .
WORKDIR /app/src/CafeSphere.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "CafeSphere.API.dll"]
