FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
USER app

# This stage builds the Worker Service
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["SpeedEventEmitter/SpeedEventEmitter.csproj", "SpeedEventEmitter/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Data/Data.csproj", "Data/"]
RUN dotnet restore "SpeedEventEmitter/SpeedEventEmitter.csproj"
COPY . .
WORKDIR "/src/SpeedEventEmitter"
RUN dotnet build "SpeedEventEmitter.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SpeedEventEmitter.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpeedEventEmitter.dll"]
