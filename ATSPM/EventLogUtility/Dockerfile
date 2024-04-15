#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
#ENV GOOGLE_APPLICATION_CREDENTIALS=/keys/FILE_NAME.json

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["EventLogUtility/EventLogUtility.csproj", "EventLogUtility/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["ApplicationCore/ApplicationCore.csproj", "ApplicationCore/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["DomainCore/DomainCore.csproj", "DomainCore/"]
COPY ["MySqlDatabaseProvider/MySqlDatabaseProvider.csproj", "MySqlDatabaseProvider/"]
COPY ["OracleDatabaseProvider/OracleDatabaseProvider.csproj", "OracleDatabaseProvider/"]
COPY ["PostgreSQLDatabaseProvider/PostgreSQLDatabaseProvider.csproj", "PostgreSQLDatabaseProvider/"]
COPY ["SqlDatabaseProvider/SqlDatabaseProvider.csproj", "SqlDatabaseProvider/"]
COPY ["SqlLiteDatabaseProvider/SqlLiteDatabaseProvider.csproj", "SqlLiteDatabaseProvider/"]
RUN dotnet restore "EventLogUtility/EventLogUtility.csproj"
COPY . .
WORKDIR "/src/EventLogUtility"
RUN dotnet build "EventLogUtility.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventLogUtility.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventLogUtility.dll"]
