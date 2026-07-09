# ==========================================================
# FastShip - Docker Build (Railway CI trigger)
# Optimizations: DebugType=none, GlobalizationInvariant=1
# ==========================================================

# ==========================================================
# Stage 1: Build
# ==========================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore (cached layer)
COPY ShipFoodCore/ShipFoodCore.csproj ./ShipFoodCore/
RUN dotnet restore ShipFoodCore/ShipFoodCore.csproj

# Copy everything else and publish
# Railway Optimizations: DebugType=none (-10MB), DebugSymbols=false (-5MB)
COPY . .
RUN dotnet publish ShipFoodCore/ShipFoodCore.csproj -c Release -o /app/publish --no-restore \
    -p:DebugType=none \
    -p:DebugSymbols=false

# ==========================================================
# Stage 2: Runtime
# ==========================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY mysql_utf8.sql ./mysql_utf8.sql
COPY seed.sql ./seed.sql

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

ENTRYPOINT ["dotnet", "ShipFoodCore.dll"]
