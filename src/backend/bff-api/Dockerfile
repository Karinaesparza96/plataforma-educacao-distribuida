FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

# Stage: build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY ["bff-api/src/BFF.API/BFF.API.csproj", "bff-api/src/BFF.API/"]
COPY ["bff-api/src/BFF.Application/BFF.Application.csproj", "bff-api/src/BFF.Application/"]
COPY ["bff-api/src/BFF.Domain/BFF.Domain.csproj", "bff-api/src/BFF.Domain/"]
COPY ["bff-api/src/BFF.Infrastructure/BFF.Infrastructure.csproj", "bff-api/src/BFF.Infrastructure/"]
COPY ["building-blocks/core/Core.csproj", "building-blocks/core/"]

# Restaurar dependências
RUN dotnet restore "bff-api/src/BFF.API/BFF.API.csproj"

# Copiar código fonte
COPY . .

# Build da aplicação
WORKDIR "/src/bff-api/src/BFF.API"
RUN dotnet build "BFF.API.csproj" -c Release -o /app/build

# Stage: publish
FROM build AS publish
RUN dotnet publish "BFF.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage: runtime
FROM base AS final
WORKDIR /app

# Instalar curl para health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# OCI labels
ARG BUILD_DATE="unknown"
ARG VCS_REF="unknown"
ARG VERSION="dev"
LABEL org.opencontainers.image.title="bff-api" \
      org.opencontainers.image.description="BFF API (.NET)" \
      org.opencontainers.image.created=$BUILD_DATE \
      org.opencontainers.image.revision=$VCS_REF \
      org.opencontainers.image.version=$VERSION

# Usuário não-root
RUN getent group app >/dev/null || addgroup --system app \
    && id -u app >/dev/null 2>&1 || adduser --system --ingroup app app \
    && chown -R app:app /app

# Copiar arquivos publicados
COPY --from=publish --chown=app:app /app/publish .

# Definir variáveis de ambiente
ENV ASPNETCORE_URLS="http://+:5000"

USER app

ENTRYPOINT ["dotnet", "BFF.API.dll"] 
