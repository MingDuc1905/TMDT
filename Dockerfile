# ==========================================================
# FastShip - Docker Build (Render-optimized)
# ==========================================================

# ==========================================================
# Stage 1: Restore (cached layer)
# ==========================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

# NuGet config for reliable restore
COPY nuget.config ./

# Only copy project file first — Docker caches this layer
COPY ShipFoodCore/ShipFoodCore.csproj ./ShipFoodCore/

# Restore with optimizations for low-memory
ENV NUGET_XMLDOC_MODE=none
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN dotnet restore ShipFoodCore/ShipFoodCore.csproj --verbosity quiet

# ==========================================================
# Stage 2: Build
# ==========================================================
FROM restore AS build

# Cache invalidation: increment CACHE_BUST value below to force fresh Docker build
# Change value → file content changes → layer hash changes → cache miss
ARG CACHE_BUST
RUN echo "${CACHE_BUST}" > /tmp/.cache-bust && echo "Build CACHE_BUST=${CACHE_BUST}"

# Copy source files (dockerignore excludes .agents/, Skills/, .git/, etc.)
COPY ShipFoodCore/ ./ShipFoodCore/

# Publish with trimmed output
RUN dotnet publish ShipFoodCore/ShipFoodCore.csproj -c Release -o /app/publish \
    --no-restore \
    --verbosity quiet \
    -p:DebugType=none \
    -p:DebugSymbols=false \
    -p:CopyOutputSymbolsToPublishDirectory=false

# ==========================================================
# Stage 3: Runtime (slim image)
# ==========================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .
COPY seed.sql ./seed.sql

# Suppress ICU warning (kosher for ASP.NET)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

EXPOSE 8080

ENTRYPOINT ["dotnet", "ShipFoodCore.dll"]
