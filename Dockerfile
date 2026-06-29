# =========================
# 1. BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG GH_PACKAGE_TOKEN
ARG GH_USER

# GitHub Packages
RUN dotnet nuget add source "https://nuget.pkg.github.com/alpac-organization/index.json" \
    --name "GitHub" \
    --username "$GH_USER" \
    --password "$GH_PACKAGE_TOKEN" \
    --store-password-in-clear-text

# Copiar csproj para cache
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/ERP.Core.Manager.Api/ERP.Core.Manager.Api.csproj", "src/ERP.Core.Manager.Api/"]

# Restore
RUN dotnet restore "src/ERP.Core.Manager.Api/ERP.Core.Manager.Api.csproj"

# Copiar todo el código
COPY src/ ./src/

WORKDIR "/src/src/ERP.Core.Manager.Api"

# Build
RUN dotnet build "ERP.Core.Manager.Api.csproj" -c Release -o /app/build


# =========================
# 2. PUBLISH STAGE
# =========================
FROM build AS publish
RUN dotnet publish "ERP.Core.Manager.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false


# =========================
# 3. RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

USER root

# =========================
# CHROMIUM + DEPENDENCIAS
# =========================
RUN apt-get update && apt-get install -y \
    chromium \
    chromium-browser \
    fonts-liberation \
    libnss3 \
    libatk-bridge2.0-0 \
    libx11-xcb1 \
    libxcb-dri3-0 \
    libdrm2 \
    libgbm1 \
    libasound2t64 \
    libxshmfence1 \
    libpulse0 \
    libxss1 \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# =========================
# 🔥 VERIFICACIÓN CHROMIUM
# =========================

RUN echo "===== CHECK CHROMIUM =====" && \
    RUN which chromium-browser || which chromium || true && \
    chromium --version || true && \
    echo "✅ Chromium instalado correctamente" && \
    echo "========================="

RUN sed -i 's/CipherString = DEFAULT@SECLEVEL=2/CipherString = DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf

RUN apt-get update && \
    apt-get install -y ca-certificates && \
    update-ca-certificates

RUN apk add --no-cache openssl
# =========================
# APP COPY
# =========================
COPY --from=publish /app/publish .

# Templates (si aplica)
COPY --from=publish /app/publish/Templates ./Templates

# =========================
# ENV CONFIG
# =========================
ENV ASPNETCORE_URLS=http://+:8080
ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium

EXPOSE 8080

# =========================
# START APP
# =========================
ENTRYPOINT ["dotnet", "ERP.Core.Manager.Api.dll"]