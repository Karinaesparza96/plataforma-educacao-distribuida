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

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS="http://+:5002"
ENTRYPOINT ["dotnet", "Conteudo.API.dll"]