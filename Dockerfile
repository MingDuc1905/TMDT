# ==========================================================
# Stage 1: Build
# ==========================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore (cached layer)
COPY ShipFoodCore/ShipFoodCore.csproj ./ShipFoodCore/
RUN dotnet restore ShipFoodCore/ShipFoodCore.csproj

# Copy everything else and publish
COPY . .
RUN dotnet publish ShipFoodCore/ShipFoodCore.csproj -c Release -o /app/publish --no-restore

# ==========================================================
# Stage 2: Runtime
# ==========================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY mysql_utf8.sql ./mysql_utf8.sql

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ShipFoodCore.dll"]
