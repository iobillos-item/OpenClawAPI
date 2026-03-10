FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY OpenClawApi.sln .
COPY src/OpenClawApi.Domain/OpenClawApi.Domain.csproj src/OpenClawApi.Domain/
COPY src/OpenClawApi.Application/OpenClawApi.Application.csproj src/OpenClawApi.Application/
COPY src/OpenClawApi.Infrastructure/OpenClawApi.Infrastructure.csproj src/OpenClawApi.Infrastructure/
COPY src/OpenClawApi.Api/OpenClawApi.Api.csproj src/OpenClawApi.Api/
COPY tests/OpenClawApi.Application.Tests/OpenClawApi.Application.Tests.csproj tests/OpenClawApi.Application.Tests/
COPY tests/OpenClawApi.Api.Tests/OpenClawApi.Api.Tests.csproj tests/OpenClawApi.Api.Tests/

RUN dotnet restore

# Copy everything and build
COPY . .
RUN dotnet build -c Release --no-restore

# Run tests
RUN dotnet test -c Release --no-build --no-restore

# Publish
FROM build AS publish
RUN dotnet publish src/OpenClawApi.Api/OpenClawApi.Api.csproj -c Release --no-build -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "OpenClawApi.Api.dll"]
