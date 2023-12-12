## Use the .NET SDK image to build the project
#FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
#WORKDIR /src
#COPY ["DomainCore/DomainCore.csproj", "DomainCore/"]
#COPY ["ApplicationCore/ApplicationCore.csproj", "ApplicationCore/"]
#COPY ["Data/Data.csproj", "Data/"]
#COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
#COPY ["ReportApi/ReportApi.csproj", "ReportApi/"]
#COPY ["WatchDog/WatchDog.csproj", "WatchDog/"]
#RUN dotnet restore "WatchDog/WatchDog.csproj"
#COPY . .
#WORKDIR "/src/WatchDog"
#RUN dotnet build "WatchDog.csproj" -c Release -o /app/build/WatchDog
#
## Publish the application
#FROM build AS publish
#RUN dotnet publish "WatchDog.csproj" -c Release -o /app/publish/WatchDog
#
## Generate runtime image
#FROM mcr.microsoft.com/dotnet/runtime:7.0 AS final
#WORKDIR /app
#COPY --from=publish /app/publish/WatchDog .
#ENTRYPOINT ["dotnet", "WatchDog.dll"]
#

# Build ReportApi project


# Build WatchDog project
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS watchdog-build
WORKDIR /src
COPY ["WatchDog/WatchDog.csproj", "WatchDog/"]
RUN dotnet restore "WatchDog/WatchDog.csproj"
COPY ["WatchDog/", "WatchDog/"]
# Copy ReportApi.dll from the previous stage
COPY --from=reportapi-build /app/reportapi-build/ReportApi.dll /app/watchdog-build/
WORKDIR "/src/WatchDog"
RUN dotnet build "WatchDog.csproj" -c Release -o /app/watchdog-build

# Publish the WatchDog application
FROM watchdog-build AS publish
RUN dotnet publish "WatchDog.csproj" -c Release -o /app/publish

# Generate runtime image
FROM mcr.microsoft.com/dotnet/runtime:7.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WatchDog.dll"]
