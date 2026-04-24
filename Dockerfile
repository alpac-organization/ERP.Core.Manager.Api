# 1. Etapa de compilación (Build)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG GH_PACKAGE_TOKEN
ARG GH_USER

# Creamos un nuget.config temporal para la restauración
RUN dotnet nuget add source "https://nuget.pkg.github.com/alpac-organization/index.json" \
    --name "GitHub" \
    --username "$GH_USER" \
    --password "$GH_PACKAGE_TOKEN" \
    --store-password-in-clear-text

# Copiar los archivos .csproj de todas las capas para restaurar dependencias
# Esto es vital para que Docker aproveche el caché si no cambian las dependencias
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/ERP.Core.Manager.Api/ERP.Core.Manager.Api.csproj", "src/ERP.Core.Manager.Api/"]

# Restaurar las dependencias del proyecto principal (esto restaura las demás por cascada)
RUN dotnet restore "src/ERP.Core.Manager.Api/ERP.Core.Manager.Api.csproj"

# Copiar todo el contenido de la carpeta src
COPY src/ ./src/

# Compilar el proyecto de la API
WORKDIR "/src/src/ERP.Core.Manager.Api"
RUN dotnet build "ERP.Core.Manager.Api.csproj" -c Release -o /app/build

# 2. Etapa de publicación
FROM build AS publish
RUN dotnet publish "ERP.Core.Manager.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y \
    chromium \
    chromium-browser \
    fonts-liberation \
    libnss3 \
    libxss1 \
    libasound2t64 \
    libatk-bridge2.0-0 \
    libgtk-3-0 \
    libgbm1 \
    libxshmfence1 \
    libxkbcommon0 \
    --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*

RUN which chromium || echo "Chromium no encontrado"

ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium
ENV DOTNET_RUNNING_IN_CONTAINER=true
    
COPY --from=publish /app/publish .
COPY --from=publish /app/publish/Templates ./Templates

# Render usa el puerto 8080 por defecto. 
# ASP.NET Core leerá esta variable para levantar el servidor ahí.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ERP.Core.Manager.Api.dll"]