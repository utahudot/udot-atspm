#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DataApi/DataApi.csproj", "DataApi/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["DomainCore/DomainCore.csproj", "DomainCore/"]
COPY ["ApplicationCore/ApplicationCore.csproj", "ApplicationCore/"]
RUN dotnet restore "DataApi/DataApi.csproj"
COPY . .
WORKDIR "/src/DataApi"
RUN dotnet build "DataApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ATSPM.DataApi.dll"]