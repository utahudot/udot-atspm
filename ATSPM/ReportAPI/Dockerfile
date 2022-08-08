#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ReportAPI/ReportAPI.csproj", "ReportAPI/"]
RUN dotnet restore "ReportAPI/ReportAPI.csproj"
COPY . .
WORKDIR "/src/ReportAPI"
RUN dotnet build "ReportAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReportAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReportAPI.dll"]