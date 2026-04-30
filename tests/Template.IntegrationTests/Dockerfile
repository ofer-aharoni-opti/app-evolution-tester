FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app
COPY NuGet.Config .
COPY . .

WORKDIR /app/tests/Template.IntegrationTests
RUN dotnet restore Template.IntegrationTests.csproj --configfile /app/NuGet.Config
RUN dotnet build Template.IntegrationTests.csproj --no-restore

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD dotnet test --no-build --list-tests Template.IntegrationTests.csproj || exit 1

ENTRYPOINT ["dotnet", "test", "--no-build", "Template.IntegrationTests.csproj", "--logger:trx"]