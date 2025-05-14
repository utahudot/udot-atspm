# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SpeedEventEmitter/SpeedEventEmitter.csproj", "SpeedEventEmitter/"]
RUN dotnet restore "SpeedEventEmitter/SpeedEventEmitter.csproj"
COPY SpeedEventEmitter/ SpeedEventEmitter/
WORKDIR "/src/SpeedEventEmitter"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Run stage
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SpeedEventEmitter.dll"]
