# syntax=docker/dockerfile:1.7

# =========================
# 1. BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copiar únicamente los .csproj para aprovechar la caché de restore
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/ERP.Core.Manager.Api/ERP.Core.Manager.Api.csproj", "src/ERP.Core.Manager.Api/"]

# Restore con GitHub Packages usando secretos (no quedan en ARG/ENV ni en el historial de la imagen)
RUN --mount=type=secret,id=gh_user \
    --mount=type=secret,id=gh_token \
    dotnet nuget add source "https://nuget.pkg.github.com/alpac-organization/index.json" \
        --name "GitHub" \
        --username "$(cat /run/secrets/gh_user)" \
        --password "$(cat /run/secrets/gh_token)" \
        --store-password-in-clear-text \
    && dotnet restore "src/ERP.Core.Manager.Api/ERP.Core.Manager.Api.csproj" \
    && rm -f ~/.nuget/NuGet/NuGet.Config

# Copiar el resto del código
COPY src/ ./src/

WORKDIR /src/src/ERP.Core.Manager.Api

# =========================
# 2. PUBLISH STAGE
# =========================
FROM build AS publish

# publish ya compila, no hace falta "dotnet build" antes
RUN dotnet publish "ERP.Core.Manager.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# =========================
# 3. RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Chromium + fuentes (las dependencias de libs vienen con el paquete chromium)
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        chromium \
        fonts-liberation \
        ca-certificates \
        openssl \
    && rm -rf /var/lib/apt/lists/*

COPY openssl.cnf /etc/ssl/openssl.cnf
ENV OPENSSL_CONF=/etc/ssl/openssl.cnf

# Verificar Chromium
RUN chromium --version

# Copiar la aplicación (incluye Templates si están en el publish)
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium

EXPOSE 8080

ENTRYPOINT ["dotnet", "ERP.Core.Manager.Api.dll"]