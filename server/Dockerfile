# =============================================================================
# Dockerfile - PharmaIntel.API (.NET 10)
# Multi-stage build:
#   1) build : restore + publish bang SDK image
#   2) final : runtime ASP.NET image, chi chua artifact da publish
# Build context: thu muc `server/` (chua .slnx + cac project).
# Vi du build & run:
#   docker build -t pharmaintel-api ./server
#   docker run --rm -p 8080:8080 \
#     -e ASPNETCORE_ENVIRONMENT=Production \
#     -e ConnectionStrings__DefaultConnection="..." \
#     -e Jwt__Key="..." \
#     -e Gemini__ApiKey="..." \
#     pharmaintel-api
# =============================================================================

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj truoc de tan dung layer cache cho `dotnet restore`.
# Khi chi sua source, layer restore se duoc reuse.
COPY PharmaIntel.API/PharmaIntel.API.csproj           PharmaIntel.API/
COPY PharmaIntel.Core/PharmaIntel.Core.csproj         PharmaIntel.Core/
COPY PharmaIntel.Infrastructure/PharmaIntel.Infrastructure.csproj PharmaIntel.Infrastructure/

RUN dotnet restore PharmaIntel.API/PharmaIntel.API.csproj

# Copy toan bo source roi publish.
COPY . .
RUN mkdir -p /docs && cp docs/rag-evaluation-cases.json /docs/rag-evaluation-cases.json
RUN dotnet publish PharmaIntel.API/PharmaIntel.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---- Final runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Image mac dinh chay duoi user `app` (UID 1654) - khong root.
# Kestrel mac dinh listen 8080 (HTTP) cho non-root container.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true

EXPOSE 8080

COPY --from=build /app/publish .

# Healthcheck dung endpoint /health/live (liveness - chi check process song).
# Readiness (DB) duoc check rieng o tang orchestration (compose/k8s) qua /health/ready.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget -q --spider http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "PharmaIntel.API.dll"]
