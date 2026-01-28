FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto (corrigidos)
COPY ["pagamentos-api/src/Pagamentos.API/Pagamentos.API.csproj", "pagamentos-api/src/Pagamentos.API/"]
COPY ["pagamentos-api/src/Pagamentos.Application/Pagamentos.Application.csproj", "pagamentos-api/src/Pagamentos.Application/"]
COPY ["pagamentos-api/src/Pagamentos.Domain/Pagamentos.Domain.csproj", "pagamentos-api/src/Pagamentos.Domain/"]
COPY ["pagamentos-api/src/Pagamentos.Infrastructure/Pagamentos.Infrastructure.csproj", "pagamentos-api/src/Pagamentos.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "pagamentos-api/src/Pagamentos.API/Pagamentos.API.csproj"

# Copiar código fonte
COPY . .

# Build da aplicação
WORKDIR "/src/pagamentos-api/src/Pagamentos.API"
RUN dotnet build "Pagamentos.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pagamentos.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Instalar curl para health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# OCI labels
ARG BUILD_DATE="unknown"
ARG VCS_REF="unknown"
ARG VERSION="dev"
LABEL org.opencontainers.image.title="pagamentos-api" \
      org.opencontainers.image.description="Pagamentos API (.NET)" \
      org.opencontainers.image.created=$BUILD_DATE \
      org.opencontainers.image.revision=$VCS_REF \
      org.opencontainers.image.version=$VERSION

# Usuário não-root
RUN getent group app >/dev/null || addgroup --system app \
    && id -u app >/dev/null 2>&1 || adduser --system --ingroup app app \
    && chown -R app:app /app

# Copiar arquivos publicados
COPY --from=publish --chown=app:app /app/publish .

# Definir variáveis de ambiente para HTTP apenas
ENV ASPNETCORE_URLS="http://+:5004"

USER app

ENTRYPOINT ["dotnet", "Pagamentos.API.dll"] 
