FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Application/Application.csproj", "Application/"]
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

FROM base AS final
WORKDIR /app

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
COPY --from=publish /app/publish .
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "WatchDog.dll"]
