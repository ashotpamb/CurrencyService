#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["../CurrencyService/CurrencyService.csproj", "CurrencyService/"]
COPY ["../ExchangeDateData/ExchangeData.csproj", "ExchangeDateData/"]
COPY ["../ArchiveCleanupService/ArchiveCleanupService.csproj", "ArchiveCleanupService/"]
RUN dotnet restore "./CurrencyService/CurrencyService.csproj"
COPY . .
WORKDIR "/src/CurrencyService"
RUN dotnet build "./CurrencyService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CurrencyService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CurrencyService.dll"]