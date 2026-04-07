# 1. Etapa de compilación (Build)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

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

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libgdiplus \
        libc6-dev \
        libx11-dev \
        libxext-dev \
        libxrender-dev \
        libfontconfig1 \
        libfreetype6 \
        libjpeg62-turbo \
        libpng16-16 \
        libx11-6 \
        libxcb1 \
        libxext6 \
        libxrender1 \
        xfonts-75dpi \
        xfonts-base \
    && rm -rf /var/lib/apt/lists/*

# 🔥 INSTALAR WKHTMLTOPDF
RUN wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6-1/wkhtmltox_0.12.6-1.bullseye_amd64.deb \
    && apt install -y ./wkhtmltox_0.12.6-1.bullseye_amd64.deb \
    && rm wkhtmltox_0.12.6-1.bullseye_amd64.deb

# 3. Imagen final de ejecución (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
# En la parte final de tu Dockerfile
COPY --from=publish /app/publish/Templates ./Templates

# Render usa el puerto 8080 por defecto. 
# ASP.NET Core leerá esta variable para levantar el servidor ahí.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ERP.Core.Manager.Api.dll"]