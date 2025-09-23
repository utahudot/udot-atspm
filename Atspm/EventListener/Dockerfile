# See https://aka.ms/customizecontainer to learn how to customize your debug container

# This stage is for running in ASP.NET Core environment (Debug/Release)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
USER app

# This stage builds the Worker Service
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for restore
COPY ["EventListener/EventListener.csproj", "EventListener/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Data/Data.csproj", "Data/"]

# Restore dependencies
RUN dotnet restore "EventListener/EventListener.csproj"

# Copy remaining source and build
COPY . .
WORKDIR "/src/EventListener"
RUN dotnet build "EventListener.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EventListener.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventListener.dll"]

# Metadata labels
LABEL org.opencontainers.image.title="Event Listener"
LABEL org.opencontainers.image.description="Worker service for batching speed events and sending to DataApi"
LABEL org.opencontainers.image.vendor="Udot.Utah.Gov"
LABEL org.opencontainers.image.source="https://github.com/OpenSourceTransportation/Atspm"
LABEL org.opencontainers.image.documentation="https://github.com/OpenSourceTransportation/Atspm/wiki/EventListenerWorker"
LABEL org.opencontainers.image.licenses="Apache-2.0"
