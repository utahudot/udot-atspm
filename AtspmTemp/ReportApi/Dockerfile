##See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DomainCore/DomainCore.csproj", "DomainCore/"]
COPY ["ApplicationCore/ApplicationCore.csproj", "ApplicationCore/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["MySqlDatabaseProvider/MySqlDatabaseProvider.csproj", "MySqlDatabaseProvider/"]
COPY ["OracleDatabaseProvider/OracleDatabaseProvider.csproj", "OracleDatabaseProvider/"]
COPY ["PostgreSQLDatabaseProvider/PostgreSQLDatabaseProvider.csproj", "PostgreSQLDatabaseProvider/"]
COPY ["SqlDatabaseProvider/SqlDatabaseProvider.csproj", "SqlDatabaseProvider/"]
COPY ["SqlLiteDatabaseProvider/SqlLiteDatabaseProvider.csproj", "SqlLiteDatabaseProvider/"]
COPY ["ReportApi/ReportApi.csproj", "ReportApi/"]
RUN dotnet restore "ReportApi/ReportApi.csproj"
COPY . .
RUN echo "Contents of /src:"
RUN ls /src
WORKDIR "/src/ReportApi"
ENV DOTNET_BUILD_SUPPRESS_WARNINGS=CS1591
RUN dotnet build "ReportApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReportApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
COPY --from=publish /app/publish .
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "ReportApi.dll"]



