# Use the official image as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DomainCore/DomainCore.csproj", "DomainCore/"]
COPY ["ApplicationCore/ApplicationCore.csproj", "ApplicationCore/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["WatchDog/WatchDog.csproj", "WatchDog/"]
RUN dotnet restore "WatchDog/WatchDog.csproj"
COPY . .
WORKDIR "/src/WatchDog"
ENV DOTNET_BUILD_SUPPRESS_WARNINGS=CS1591
RUN dotnet build "WatchDog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WatchDog.csproj" -c Release -o /app/publish

# Generate runtime image
FROM base AS final
WORKDIR /app

# Create a non-root user and group
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

# Copy only the necessary files from the publish stage
COPY --from=publish /app/publish .

# Change ownership of the application files to the non-root user
RUN chown -R appuser:appgroup /app

# Switch to the non-root user
USER appuser

# Start the application
ENTRYPOINT ["dotnet", "WatchDog.dll"]
