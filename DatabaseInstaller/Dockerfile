# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["DatabaseInstaller/DatabaseInstaller.csproj", "DatabaseInstaller/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["MySqlDatabaseProvider/MySqlDatabaseProvider.csproj", "MySqlDatabaseProvider/"]
COPY ["OracleDatabaseProvider/OracleDatabaseProvider.csproj", "OracleDatabaseProvider/"]
COPY ["PostgreSQLDatabaseProvider/PostgreSQLDatabaseProvider.csproj", "PostgreSQLDatabaseProvider/"]
COPY ["SqlDatabaseProvider/SqlDatabaseProvider.csproj", "SqlDatabaseProvider/"]
COPY ["SqlLiteDatabaseProvider/SqlLiteDatabaseProvider.csproj", "SqlLiteDatabaseProvider/"]

# Restore dependencies
RUN dotnet restore "DatabaseInstaller/DatabaseInstaller.csproj"

# Copy all source files and build
COPY . .
WORKDIR "/src/DatabaseInstaller"
RUN dotnet build "DatabaseInstaller.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "DatabaseInstaller.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set entry point
ENTRYPOINT ["dotnet", "DatabaseInstaller.dll"]

# Labels for metadata
LABEL org.opencontainers.image.title="ATSPM Database Installer"
LABEL org.opencontainers.image.description="Database installer for the ATSPM platform"
LABEL org.opencontainers.image.vendor="Udot.Utah.Gov"
LABEL org.opencontainers.image.source=https://github.com/OpenSourceTransportation/Atspm
LABEL org.opencontainers.image.documentation=https://github.com/OpenSourceTransportation/Atspm/wiki/DatabaseInstaller
LABEL org.opencontainers.image.licenses=Apache-2.0
