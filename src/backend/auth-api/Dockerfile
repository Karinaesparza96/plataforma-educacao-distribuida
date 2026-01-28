FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY ["auth-api/src/Auth.API/Auth.API.csproj", "auth-api/src/Auth.API/"]
COPY ["auth-api/src/Auth.Application/Auth.Application.csproj", "auth-api/src/Auth.Application/"]
COPY ["auth-api/src/Auth.Domain/Auth.Domain.csproj", "auth-api/src/Auth.Domain/"]
COPY ["auth-api/src/Auth.Infrastructure/Auth.Infrastructure.csproj", "auth-api/src/Auth.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "auth-api/src/Auth.API/Auth.API.csproj"

# Copiar código fonte
COPY . .

# Build da aplicação
WORKDIR "/src/auth-api/src/Auth.API"
RUN dotnet build "Auth.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Instalar curl para health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# OCI labels
ARG BUILD_DATE="unknown"
ARG VCS_REF="unknown"
ARG VERSION="dev"
LABEL org.opencontainers.image.title="auth-api" \
      org.opencontainers.image.description="Auth API (.NET)" \
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
ENV ASPNETCORE_URLS="http://+:5001"

USER app

ENTRYPOINT ["dotnet", "Auth.API.dll"] 
