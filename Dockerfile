# ── Build stage ──────────────────────────────────────────────────────────────
# cache-bust: 2026-03-21
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ProcurementInventory.sln ./
COPY src/ProcurementInventory.Api/*.csproj ./src/ProcurementInventory.Api/
RUN dotnet restore

COPY src/ ./src/
RUN dotnet publish src/ProcurementInventory.Api/ProcurementInventory.Api.csproj \
    -c Release -o /out --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /out ./

# Render 會注入 PORT 環境變數
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}

ENTRYPOINT ["dotnet", "ProcurementInventory.Api.dll"]
