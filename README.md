# 🎓 Plataforma Educacional Distribuída com DevOps Completo

[![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com)
[![Angular](https://img.shields.io/badge/Angular-18-red)](https://angular.dev)
[![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=flat&logo=docker&logoColor=white)](https://www.docker.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-enabled-success)](https://kubernetes.io)
[![CI - Build & Test](https://github.com/Karinaesparza96/plataforma-educacao-distribuida/actions/workflows/ci.yml/badge.svg)](https://github.com/Karinaesparza96/plataforma-educacao-distribuida/actions/workflows/ci.yml)
[![CD - Deploy](https://github.com/Karinaesparza96/plataforma-educacao-distribuida/actions/workflows/cd.yml/badge.svg)](https://github.com/Karinaesparza96/plataforma-educacao-distribuida/actions/workflows/cd.yml)

Plataforma educacional distribuída construída com microserviços em .NET 9, frontend Angular, RabbitMQ para mensageria assíncrona, SQL Server para persistência e DevOps.

### Integrantes
- Karina Esparza

## 📋 Índice

- [Pré-requisitos](#-pré-requisitos)
- [Execução Rápida (Docker Compose)](#-execução-rápida-com-docker-compose)
- [Execução com Kubernetes](#-execução-com-kubernetes)
- [CI/CD Pipelines](#-cicd-pipelines)
- [Desenvolvimento](#%EF%B8%8F-desenvolvimento)
- [Testes](#-testes)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Licença](#-licença)

## Visão Geral

Projeto de plataforma de educação distribuída com arquitetura de microserviços. Inclui módulos de autenticação, gerenciamento de alunos, conteúdo educacional, processamento de pagamentos e um Backend for Frontend (BFF) para otimizar chamadas ao frontend Angular.

## Tecnologias Principais

- **Backend**: .NET 9, ASP.NET Core, Entity Framework Core, ASP.NET Core Identity, MediatR, MassTransit (RabbitMQ)
- **Frontend**: Angular 18
- **Infraestrutura**: SQL Server, RabbitMQ, Redis
- **Containerização e Orquestração**: Docker, Docker Compose, Kubernetes
- **CI/CD**: GitHub Actions
- **Outros**: JWT para autenticação, Health Checks, Clean Architecture / DDD

## Estrutura do Projeto

```
mba.modulo5/
├── .github/                         # 🆕 Automação GitHub Actions
│   └── workflows/
│       ├── ci.yml                   # Pipeline de Build e Testes
│       └── cd.yml                   # Pipeline de Deploy
├── k8s/                             # 🆕 Manifests Kubernetes
│   ├── 01-base.yaml
│   ├── 02-infra.yaml
│   ├── 03-apis.yaml
│   └── 04-front.yaml                            
├── src/backend/                     # Microserviços .NET
│   ├── auth-api/
│   ├── alunos-api/
│   ├── conteudo-api/
│   ├── pagamentos-api/
│   ├── bff-api/
│   └── building-blocks/             # Componentes compartilhados
├── src/frontend/                    # Angular 18 SPA
├── docker-compose.yml               # 🆕 Orquestração local simples
├── setup-k8s.ps1                    # 🆕 Script PowerShell para aplicar K8s local
└── README.md
```

## 🚀 Pré-requisitos

### Obrigatórios
- **Docker** >= 20.10
- **Docker Compose** >= 2.0
- **Docker Desktop** v4.20+ (com Kubernetes habilitado)
- **Kubectl** (CLI do Kubernetes)
- **Git**

### Para Desenvolvimento
- **.NET SDK 9.0**
- **Node.js 18+** (para Angular)
- **Visual Studio 2022** ou **VS Code**

## 🐳 Execução Rápida com Docker Compose

Esta é a forma mais simples de rodar o ambiente completo localmente para desenvolvimento rápido.

### 1. Clonar o Repositório
```bash
git clone https://github.com/Karinaesparza96/plataforma-educacao-distribuida.git
cd mba.modulo4
```

### 2. Executar o Sistema 
```powershell
docker compose up -d --build
```

### 3. Acessar a Aplicação

Após ~5 minutos de inicialização:

- **🌐 Frontend**: http://localhost:4200 (aluno1@auth.api/Teste@123 ou admin@auth.api/Teste@123)
- **📊 RabbitMQ Management**: http://localhost:15672 (admin/admin123)

## 4. Parar
```bash
docker compose down
````

## ☸️ Execução com Kubernetes

## Passo 1: Instalar e abrir o Docker Desktop

Baixe e instale:  
https://www.docker.com/products/docker-desktop/

Abra o aplicativo e aguarde a inicialização completa.

## Passo 2: Habilitar e iniciar o Kubernetes

1. Clique na engrenagem (**Settings**)
2. Vá para **Kubernetes** (menu esquerdo)
3. Marque **Enable Kubernetes**
4. Clique em **Apply & Restart**
5. Aguarde o status **"Kubernetes is running"** (2–5 minutos)

## Passo 3: Executar o script de setup

Na raiz do projeto, abra o **PowerShell** e execute:

```powershell
.\setup-k8s.ps1
```
O script aplica os manifests da pasta k8s/, cria o namespace plataforma e aguarda os pods.

Verifique:

```powershell
kubectl get pods -n plataforma -w
```
## 4. Acessar os serviços
- **Frontend**: http://localhost:4200 (aluno1@auth.api/Teste@123 ou admin@auth.api/Teste@123)
- **BFF API**: http://localhost:5001/swagger

## 5. Parar / Resetar o ambiente
Remover namespace inteiro:
```bash
kubectl delete namespace plataforma
````
Ou só os recursos:
```bash
kubectl delete -f k8s/ -n plataforma
````

## 🧩 Comandos Úteis
```powershell
# Todos os pods
kubectl get pods -n plataforma -o wide

# Logs de um pod
kubectl logs -f deployment/auth-api -n plataforma

# Descrever pod com erro
kubectl describe pod <nome-do-pod> -n plataforma

# Reiniciar deployment
kubectl rollout restart deployment/auth-api -n plataforma

# Ver eventos
kubectl get events -n plataforma --sort-by=.metadata.creationTimestamp
````

## 🔄 CI/CD Pipelines

O projeto utiliza **GitHub Actions** para automação completa do ciclo de vida de desenvolvimento. Os workflows estão localizados no diretório `.github/workflows/`.

### 🛠️ CI - Integração Contínua (`ci.yml`)
Disparado automaticamente a cada *Pull Request* ou *Push* na branch principal (`main`).
* **Build**: Restaura as dependências e compila todos os microserviços .NET e o Frontend Angular.
* **Test**: Executa a suíte de testes unitários e de integração (xUnit) para garantir a integridade do código.

### 🚀 CD - Entrega Contínua (`cd.yml`)
Disparado após a conclusão bem-sucedida do pipeline de CI na branch `main`.
* **Dockerize**: Gera as imagens Docker para cada microserviço e para o frontend.
* **Push**: Envia as imagens tagueadas para o Container Registry configurado.

## 🛠️ Desenvolvimento

### Executar APIs Localmente
```powershell
# Auth API
cd src\backend\auth-api
dotnet run

# Conteudo API
cd src\backend\conteudo-api
dotnet run

# Alunos API
cd src\backend\alunos-api
dotnet run

# Pagamentos API
cd src\backend\pagamentos-api
dotnet run

# BFF API
cd src\backend\bff-api
dotnet run
```

### Executar Frontend Localmente
```powershell
cd src\frontend
npm install
npm start
```

### Rebuild de um Serviço
```bash
# Rebuild específico
docker-compose build [service-name]
docker-compose up -d [service-name]

# Exemplo: rebuild do Auth API
docker-compose build auth-api
docker-compose up -d auth-api
```

## 🧪 Testes
Cada microserviço possui testes automatizados:

- **UnitTests** → Validação de regras de negócio isoladas.  
- **IntegrationTests** → Testam endpoints reais com banco de dados em memória ou SQL local.

### Executando os testes
```bash
# Testes unitários
dotnet test src/backend/auth-api/tests/Auth.UnitTests
dotnet test src/backend/pagamentos-api/tests/Pagamentos.UnitTests

# Testes de integração
dotnet test src/backend/alunos-api/tests/Alunos.IntegrationTests
```

> Framework utilizado: **xUnit**  
> Cobertura recomendada: **80%+** (utilizando **Coverlet + ReportGenerator**)

## 📊 Cobertura de Testes

![Line Coverage](https://img.shields.io/badge/Line%20Coverage-94.8%25-brightgreen)
![Branch Coverage](https://img.shields.io/badge/Branch%20Coverage-83.5%25-blue)

![Relatório de Cobertura](https://raw.githubusercontent.com/jasonamaral/mba.modulo4/main/src/tests/coverage-report/Coverage.jpg)

## 📊 Monitoramento

### Logs dos Serviços
```bash
# Ver todos os logs
docker-compose logs -f

# Ver logs de um serviço específico
docker-compose logs -f auth-api
docker-compose logs -f frontend
docker-compose logs -f rabbitmq
```

### Health Checks
Todos os serviços possuem endpoints de health check:
- Auth API: http://localhost:5001/health
- Conteudo API: http://localhost:5002/health
- Alunos API: http://localhost:5003/health
- Pagamentos API: http://localhost:5004/health
- BFF API: http://localhost:5000/health

### Monitorar Recursos
```bash
# Ver uso de CPU e memória
docker stats

# Ver apenas containers da plataforma
docker stats --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}"
```

## 🔒 Segurança

### Configurações de Segurança
- ✅ JWT com chave secreta forte
- ✅ Segregação de rede Docker
- ✅ Health checks com timeout
- ✅ Conexões com TrustServerCertificate

### Para Produção (NÃO usar em produção real)
As configurações atuais são para **desenvolvimento/demonstração**:
- Senhas em texto claro
- Certificados auto-assinados
- Configurações de desenvolvimento

### Convenções
- Usar **Clean Architecture** em todos os microserviços
- Seguir princípios **SOLID** e **DDD**
- Implementar **health checks** em novas APIs
- Documentar com **Swagger/OpenAPI**
- Usar **async/await** para operações I/O

### Padrões de Projeto Implementados

#### 🏗️ **Clean Architecture**
- **Dependency Inversion**: Camadas internas não dependem de camadas externas
- **Separation of Concerns**: Cada camada tem responsabilidade específica
- **Testability**: Fácil mock e teste unitário

#### 📋 **CQRS (Command Query Responsibility Segregation)**
- **Commands**: Operações que modificam estado
- **Queries**: Operações que apenas consultam dados
- **Handlers**: Processamento específico para cada comando/query

#### 🎯 **Domain-Driven Design (DDD)**
- **Entities**: Objetos com identidade única
- **Value Objects**: Objetos imutáveis sem identidade
- **Aggregates**: Conjuntos de entidades relacionadas
- **Domain Events**: Eventos que representam mudanças no domínio

#### 🔄 **Repository Pattern**
- **Interfaces**: Definidas na camada de domínio
- **Implementações**: Na camada de infraestrutura
- **Abstração**: Desacoplamento entre domínio e dados

#### 📡 **Event-Driven Architecture**
- **RabbitMQ**: Message broker para comunicação assíncrona
- **Domain Events**: Eventos de domínio publicados
- **Event Handlers**: Processamento de eventos

## 📝 Licença

Este projeto é para fins educacionais (MBA DevXpert - Módulo 5).
