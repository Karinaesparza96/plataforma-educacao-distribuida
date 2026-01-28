FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY ["conteudo-api/src/Conteudo.API/Conteudo.API.csproj", "conteudo-api/src/Conteudo.API/"]
COPY ["conteudo-api/src/Conteudo.Application/Conteudo.Application.csproj", "conteudo-api/src/Conteudo.Application/"]
COPY ["conteudo-api/src/Conteudo.Domain/Conteudo.Domain.csproj", "conteudo-api/src/Conteudo.Domain/"]
COPY ["conteudo-api/src/Conteudo.Infrastructure/Conteudo.Infrastructure.csproj", "conteudo-api/src/Conteudo.Infrastructure/"]
COPY ["building-blocks/core/Core.csproj", "building-blocks/core/"]

RUN dotnet restore "conteudo-api/src/Conteudo.API/Conteudo.API.csproj"

COPY . .

WORKDIR "/src/conteudo-api/src/Conteudo.API"
RUN dotnet build "Conteudo.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Conteudo.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# OCI labels
ARG BUILD_DATE="unknown"
ARG VCS_REF="unknown"
ARG VERSION="dev"
LABEL org.opencontainers.image.title="conteudo-api" \
      org.opencontainers.image.description="Conteudo API (.NET)" \
      org.opencontainers.image.created=$BUILD_DATE \
      org.opencontainers.image.revision=$VCS_REF \
      org.opencontainers.image.version=$VERSION

# Usuário não-root
RUN getent group app >/dev/null || addgroup --system app \
    && id -u app >/dev/null 2>&1 || adduser --system --ingroup app app \
    && chown -R app:app /app

COPY --from=publish --chown=app:app /app/publish .

ENV ASPNETCORE_URLS="http://+:5002"
USER app
ENTRYPOINT ["dotnet", "Conteudo.API.dll"]
