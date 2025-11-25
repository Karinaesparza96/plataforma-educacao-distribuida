FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

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

FROM build AS publish
RUN dotnet publish "BFF.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Instalar curl para health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Definir variáveis de ambiente
ENV ASPNETCORE_URLS="http://+:5000"

ENTRYPOINT ["dotnet", "BFF.API.dll"] 