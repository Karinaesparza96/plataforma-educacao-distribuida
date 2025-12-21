# 🎓 Plataforma Educacional Distribuída com DevOps Completo

Uma plataforma educacional moderna baseada em arquitetura de **microserviços**, desenvolvida com **.NET 9**, **Angular 18**, **RabbitMQ**, **SQL Server** e **Redis**. Este projeto evolui para um **ecossistema DevOps completo** com automação de build, testes, entrega contínua (CI/CD) e orquestração em **Kubernetes**.

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![Angular](https://img.shields.io/badge/Angular-18-red)
![CI/CD](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-lightgrey)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3-orange)

### Integrantes
- Karina Esparza

## 📋 Índice

- [Pré-requisitos](#-pré-requisitos)
- [Execução Rápida (Docker Compose)](#-execução-rápida-com-docker-compose)
- [CI/CD Pipelines](#-cicd-pipelines)
- [Infraestrutura](#%EF%B8%8F-infraestrutura)
- [URLs de Acesso](#-urls-de-acesso)
- [Desenvolvimento](#%EF%B8%8F-desenvolvimento)
- [Testes](#-testes)
- [Building Blocks](#-building-blocks)
- [Usuários de Exemplo](#-usuários-de-exemplo)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Licença](#-licença)

### Visão Geral

A plataforma é composta por **5 microserviços independentes** + **1 BFF** + **1 Frontend**, cada um com seu próprio banco de dados e responsabilidades específicas.

## Estrutura do Projeto

```
mba.modulo5/
├── .github/                         # 🆕 Automação GitHub Actions
│   └── workflows/
│       ├── ci.yml                   # Pipeline de Build e Testes
│       └── cd.yml                   # Pipeline de Deploy
├── k8s/                             # Ainda não implementado
├── src/backend/                     # Microserviços .NET
│   ├── auth-api/
│   ├── alunos-api/
│   ├── conteudo-api/
│   ├── pagamentos-api/
│   ├── bff-api/
│   └── building-blocks/             # Componentes compartilhados
├── src/frontend/                    # Angular 18 SPA
├── docker-compose.yml               # Orquestração local simples
└── README.md
```

## 🚀 Pré-requisitos

### Obrigatórios
- **Docker** >= 20.10
- **Docker Compose** >= 2.0
- **Git**

### Para Kubernetes (Opcional)
- **Kubectl** (CLI do Kubernetes)
- **Minikube**, **Kind** ou **Docker Desktop** (com Kubernetes habilitado)

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
docker compose up --build
```

### 3. Acessar a Aplicação

Após ~5 minutos de inicialização:

- **🌐 Frontend**: http://localhost:4200 (aluno1@auth.api/Teste@123 ou admin@auth.api/Teste@123)
- **📊 RabbitMQ Management**: http://localhost:15672 (admin/admin123)

## ☸️ Execução no Kubernetes


## 🔄 CI/CD Pipelines

O projeto utiliza **GitHub Actions** para automação completa do ciclo de vida de desenvolvimento. Os workflows estão localizados no diretório `.github/workflows/`.

### 🛠️ CI - Integração Contínua (`ci.yml`)
Disparado automaticamente a cada *Pull Request* ou *Push* na branch principal (`main`).
* **Build**: Restaura as dependências e compila todos os microserviços .NET e o Frontend Angular.
* **Test**: Executa a suíte de testes unitários e de integração (xUnit) para garantir a integridade do código.
* **Analysis**: Realiza validações de qualidade e cobertura de código (Coverlet).

### 🚀 CD - Entrega Contínua (`cd.yml`)
Disparado após a conclusão bem-sucedida do pipeline de CI na branch `main`.
* **Dockerize**: Gera as imagens Docker para cada microserviço e para o frontend.
* **Push**: Envia as imagens tagueadas para o Container Registry configurado.
* **Deploy**: Aplica os manifestos de atualização no cluster Kubernetes utilizando as definições da pasta `k8s/`.

## 🏗️ Infraestrutura

### RabbitMQ
- **Management UI**: http://localhost:15672
- **Credenciais**: admin/admin123

### Redis
- **Host**: localhost:6379
- **Uso**: Cache distribuído para BFF

## 🌐 URLs de Acesso

### Aplicação
| Serviço | URL | Deção |
|---------|-----|-----------|
| 📱 **Frontend** | http://localhost:4200 | Interface do usuário |
| 🔗 **BFF API** | http://localhost:5000 | Gateway para frontend |

### APIs (Swagger)
| API | HTTP | Deção |
|-----|-------|------|-----------|
| 🔐 **Auth** | http://localhost:5001 | Autenticação |
| 📚 **Conteudo** | http://localhost:5002 | Cursos e aulas |
| 🎓 **Alunos** | http://localhost:5003 Matrículas |
| 💳 **Pagamentos** | http://localhost:5004 | Transações |

### Infraestrutura
| Serviço | URL | Credenciais |
|---------|-----|-------------|
| 🐰 **RabbitMQ** | http://localhost:15672 | admin/admin123 |
| 🔴 **Redis** | localhost:6379 | (sem senha) |

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


## 🧩 Building Blocks
A pasta `building-blocks/` contém componentes reutilizáveis entre microserviços:

- **Core** (`Core.csproj`)  
  - Communication (mensagens entre serviços)  
  - DomainObjects (objetos base de domínio)  
  - DomainValidations (validações reutilizáveis)  
  - Exceptions (exceções customizadas)  
  - Mediator (implementação do padrão Mediator)  
  - Notification (notificações de domínio)  
  - SharedDtos (DTOs comuns)  
  - Utils (funções auxiliares)  

- **MessageBus** (`MessageBus.csproj`)  
  - Implementação de **comunicação assíncrona** via RabbitMQ  
  - Base para publicação e consumo de eventos entre microserviços
  
## 👤 Usuários de Exemplo
A aplicação já possui usuários pré-configurados para testes:

| Usuário | Senha | Perfil |
|---------|-------|--------|
| `admin@auth.api` | `Teste@123` | Administrador |
| `aluno1@auth.api` | `Teste@123` | Aluno |

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

## 🛑 Controle do Sistema

### Parar Sistema
```powershell
docker-compose down
```

### Parar e Limpar Tudo (incluindo volumes)
```bash
# ⚠️ CUIDADO: Remove dados do banco
docker-compose down -v
docker system prune -f
```

### Reiniciar um Serviço
```bash
docker-compose restart [service-name]

# Exemplo
docker-compose restart auth-api
```

## 🔧 Solução de Problemas

### Problema: Containers não iniciam
**Solução:**
```bash
# Verificar se as portas estão ocupadas
netstat -tulpn | grep -E '(4200|5000|5001|5002|5003|5004|1433|5672|15672|6379)'

# Parar containers conflitantes
docker-compose down
docker container prune -f

# Reiniciar
docker-compose up
```

### Problema: Erro de conexão com banco
**Solução:**
```bash

# Reiniciar SQL Server
docker-compose restart sqlserver

# Aguardar 60 segundos e reiniciar APIs
sleep 60
docker-compose restart auth-api conteudo-api alunos-api pagamentos-api
```

### Problema: Frontend não carrega
**Solução:**
```bash
# Verificar logs do frontend
docker-compose logs frontend

# Verificar se BFF está rodando
curl http://localhost:5000/health

# Rebuild do frontend
docker-compose build frontend
docker-compose up -d frontend
```

### Problema: RabbitMQ não conecta
**Solução:**
```bash
# Verificar RabbitMQ
docker-compose logs rabbitmq

# Reiniciar serviços que usam RabbitMQ
docker-compose restart auth-api alunos-api pagamentos-api
```

### Monitoramento de Recursos
```bash
# Ver uso detalhado
docker stats --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}\t{{.NetIO}}\t{{.BlockIO}}"
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
