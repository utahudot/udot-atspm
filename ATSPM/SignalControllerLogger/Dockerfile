#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app
#ENV DOTNET_ENVIRONMENT=Development
ENV GOOGLE_APPLICATION_CREDENTIALS=/keys/FILE_NAME.json

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["LocationControllerLogger/LocationControllerLogger.csproj", "LocationControllerLogger/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["DomainCore/DomainCore.csproj", "DomainCore/"]
COPY ["ApplicationCore/ApplicationCore.csproj", "ApplicationCore/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "LocationControllerLogger/LocationControllerLogger.csproj"
COPY . .
WORKDIR "/src/LocationControllerLogger"
RUN dotnet build "LocationControllerLogger.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LocationControllerLogger.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LocationControllerLogger.dll"]