#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app
#ENV DOTNET_ENVIRONMENT=Development
ENV GOOGLE_APPLICATION_CREDENTIALS=/keys/FILE_NAME.json

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SignalControllerLogger/SignalControllerLogger.csproj", "SignalControllerLogger/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["DomainCore/DomainCore.csproj", "DomainCore/"]
COPY ["ApplicationCore/ApplicationCore.csproj", "ApplicationCore/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "SignalControllerLogger/SignalControllerLogger.csproj"
COPY . .
WORKDIR "/src/SignalControllerLogger"
RUN dotnet build "SignalControllerLogger.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SignalControllerLogger.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalControllerLogger.dll"]