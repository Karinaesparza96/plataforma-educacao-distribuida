# 🎓 Plataforma Educacional Distribuida com DevOps Completo

[![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com)
[![Angular](https://img.shields.io/badge/Angular-18-red)](https://angular.dev)
[![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=flat&logo=docker&logoColor=white)](https://www.docker.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-enabled-success)](https://kubernetes.io)
[![CI - Build & Test](https://github.com/Karinaesparza96/plataforma-educacao-distribuida/actions/workflows/ci.yml/badge.svg)](https://github.com/Karinaesparza96/plataforma-educacao-distribuida/actions/workflows/ci.yml)
[![CD - Deploy](https://github.com/Karinaesparza96/plataforma-educacao-distribuida/actions/workflows/cd.yml/badge.svg)](https://github.com/Karinaesparza96/plataforma-educacao-distribuida/actions/workflows/cd.yml)

Plataforma educacional distribuida com microservicos em .NET 9, frontend Angular, RabbitMQ, SQL Server e DevOps.

> Projeto academico (MBA): foco em demonstrar arquitetura, DevOps e boas praticas.

## Integrantes
- Karina Esparza

## 📋 Indice
- [Pre-requisitos](#pre-requisitos)
- [Execucao rapida (Docker Compose)](#execucao-rapida-docker-compose)
- [Execucao com Kubernetes](#execucao-com-kubernetes)
- [Comandos uteis](#comandos-uteis)
- [CI/CD](#cicd)
- [Desenvolvimento](#desenvolvimento)
- [Testes](#testes)
- [Monitoramento](#monitoramento)
- [Seguranca](#seguranca)

## 🚀 Pre-requisitos
### Obrigatorios
- Docker >= 20.10
- Docker Compose >= 2.0
- Docker Desktop v4.20+ (com Kubernetes habilitado)
- kubectl
- Git

### 🛠️ Para desenvolvimento
- .NET SDK 9.0
- Node.js 18+
- Visual Studio 2022 ou VS Code

## Estrutura do projeto
```
mba.modulo5/
|-- .github/                     # Automacao GitHub Actions
|   `-- workflows/
|       |-- ci.yml               # Pipeline de Build e Testes
|       `-- cd.yml               # Pipeline de Deploy
|-- k8s/                         # Manifests Kubernetes
|   |-- base/
|   |-- configmaps/
|   |-- deployments/
|   |-- pvc/
|   |-- secrets/
|   `-- services/
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

## 🐳 Execucao rapida (Docker Compose)
Forma mais simples para validar tudo localmente.

### 1) Clonar o repositorio
```bash
git clone https://github.com/Karinaesparza96/plataforma-educacao-distribuida.git
cd mba.modulo5
```

### 2) Configurar variaveis de ambiente
```powershell
Copy-Item .env.example .env
```

### 3) Subir o ambiente
```powershell
docker compose up -d --build
```

### 4) Acessos
- Frontend: http://localhost:4200
- RabbitMQ Management: http://localhost:15672 (admin/admin123)

### 5) Parar
```bash
docker compose down
```

## ☸️ Execucao com Kubernetes

### 1) Preparar o Docker Desktop
- Instale o Docker Desktop
- Ative o Kubernetes (Settings > Kubernetes > Enable)

### 2) Configurar variaveis de ambiente
```powershell
Copy-Item .env.example .env
```

### 3) Executar o setup
```powershell
.\setup-k8s.ps1
```
Observacao:
- O script faz port-forward direto no deployment do frontend (4200 -> 8080).

O script:
- Cria o namespace
- Gera e aplica k8s/secrets/app-secrets.yaml a partir do .env (se existir)
- Aplica os manifests e aguarda os pods

### 4) Verificar status
```powershell
kubectl get pods -n plataforma -w
```

### 5) Acessos
- Frontend: http://localhost:4200
- BFF API: http://localhost:5000/swagger

### 6) Parar / Resetar
```bash
kubectl delete namespace plataforma
```

### Problemas ao baixar imagens (ErrImagePull/ImagePullBackOff)
Se ocorrer erro de download no RabbitMQ ou Redis, faca o pull manual e recrie os pods:
```powershell
docker pull rabbitmq:3-management
docker pull redis:7-alpine

kubectl delete pod -n plataforma -l app=rabbitmq
kubectl delete pod -n plataforma -l app=redis
```

## 🧩 Comandos uteis
```powershell
kubectl get pods -n plataforma -o wide
kubectl logs -f deployment/auth-api -n plataforma
kubectl describe pod <nome-do-pod> -n plataforma
kubectl rollout restart deployment/auth-api -n plataforma
kubectl get events -n plataforma --sort-by=.metadata.creationTimestamp
```

## 🔄 CI/CD
- CI: build e testes em pull request/push (ci.yml)
- CD: build/push das imagens (cd.yml)

## Desenvolvimento
### Executar APIs localmente
```powershell
cd src\backend\auth-api; dotnet run
cd src\backend\conteudo-api; dotnet run
cd src\backend\alunos-api; dotnet run
cd src\backend\pagamentos-api; dotnet run
cd src\backend\bff-api; dotnet run
```

### Executar frontend localmente
```powershell
cd src\frontend
npm install
npm start
```

## 🧪 Testes
```bash
dotnet test src/backend/auth-api/tests/Auth.UnitTests
dotnet test src/backend/pagamentos-api/tests/Pagamentos.UnitTests
dotnet test src/backend/alunos-api/tests/Alunos.IntegrationTests
```

## 📊 Monitoramento
```bash
docker-compose logs -f
docker-compose logs -f auth-api
```

## 🔒 Seguranca
- JWT, health checks e segregacao de rede em ambiente de demo
- Nao usar as configuracoes atuais em producao real
```
