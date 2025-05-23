#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DataApi/DataApi.csproj", "DataApi/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["MySqlDatabaseProvider/MySqlDatabaseProvider.csproj", "MySqlDatabaseProvider/"]
COPY ["OracleDatabaseProvider/OracleDatabaseProvider.csproj", "OracleDatabaseProvider/"]
COPY ["PostgreSQLDatabaseProvider/PostgreSQLDatabaseProvider.csproj", "PostgreSQLDatabaseProvider/"]
COPY ["SqlDatabaseProvider/SqlDatabaseProvider.csproj", "SqlDatabaseProvider/"]
COPY ["SqlLiteDatabaseProvider/SqlLiteDatabaseProvider.csproj", "SqlLiteDatabaseProvider/"]
RUN dotnet restore "DataApi/DataApi.csproj"
COPY . .
WORKDIR "/src/DataApi"
RUN dotnet build "DataApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataApi.dll"]

LABEL org.opencontainers.image.title="Atspm Data Api"
LABEL org.opencontainers.image.description="Data Api for use with the Atspm platform"
LABEL org.opencontainers.image.vendor="Udot.Utah.Gov"
LABEL org.opencontainers.image.source=https://github.com/OpenSourceTransportation/Atspm
LABEL org.opencontainers.image.documentation=https://github.com/OpenSourceTransportation/Atspm/wiki/Data-Api
LABEL org.opencontainers.image.licenses=Apache-2.0