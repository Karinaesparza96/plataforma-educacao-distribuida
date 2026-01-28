<#
.SYNOPSIS
    Deploy rápido da Plataforma Educacional no Kubernetes.
#>

$Namespace = "plataforma"

# Cores
$ColorHeader  = "Cyan"
$ColorStep    = "Yellow"
$ColorSuccess = "Green"
$ColorWarning = "Magenta"
$ColorError   = "Red"
$ColorInfo    = "DarkCyan"

Clear-Host
Write-Host "Plataforma Educacional - Deploy Automatico" -ForegroundColor $ColorHeader
Write-Host "------------------------------------------" -ForegroundColor $ColorHeader

# 1. Verificação rápida de kubectl e cluster
Write-Host "Verificando kubectl e cluster..." -ForegroundColor $ColorHeader
if (-not (Get-Command kubectl -ErrorAction SilentlyContinue)) {
    Write-Host "ERRO: kubectl não encontrado. Instale-o primeiro." -ForegroundColor $ColorError
    exit 1
}
$clusterCheck = kubectl cluster-info --request-timeout=5s 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRO: Sem conexão com o cluster." -ForegroundColor $ColorError
    Write-Host $clusterCheck -ForegroundColor $ColorWarning
    exit 1
}
Write-Host "OK" -ForegroundColor $ColorSuccess

# 2. Criar namespace base
Write-Host ""
Write-Host "Aplicando namespace base..." -ForegroundColor $ColorHeader
kubectl apply -f k8s/base/plataforma.yaml | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRO: Falha ao aplicar namespace base." -ForegroundColor $ColorError
    exit 1
}
Write-Host "OK" -ForegroundColor $ColorSuccess

# 3. Gerar/aplicar secrets obrigatorios (automático)
Write-Host ""
Write-Host "Aplicando secrets obrigatorios..." -ForegroundColor $ColorHeader
$secretFile = "k8s/secrets/app-secrets.yaml"
$envFile = ".env"

function Get-EnvValue([hashtable]$envMap, [string]$key) {
    if (-not $envMap.ContainsKey($key) -or [string]::IsNullOrWhiteSpace($envMap[$key])) {
        return $null
    }
    return $envMap[$key]
}

if (Test-Path $envFile) {
    $envMap = @{}
    Get-Content $envFile | ForEach-Object {
        $line = $_.Trim()
        if ($line -eq "" -or $line.StartsWith("#")) { return }
        $parts = $line -split "=", 2
        if ($parts.Count -lt 2) { return }
        $k = $parts[0].Trim()
        $v = $parts[1].Trim().Trim('"')
        $envMap[$k] = $v
    }

    $sqlPassword = Get-EnvValue $envMap "SQL_PASSWORD"
    $rabbitPassword = Get-EnvValue $envMap "RABBITMQ_DEFAULT_PASS"
    $jwtSecret = Get-EnvValue $envMap "JWT_SECRET"

    if ($sqlPassword -and $rabbitPassword -and $jwtSecret) {
        @"
apiVersion: v1
kind: Secret
metadata:
  name: app-secrets
  namespace: $Namespace
type: Opaque
stringData:
  sql-password: $sqlPassword
  rabbit-password: $rabbitPassword
  jwt-secret: $jwtSecret
  auth-conn: "Server=sqlserver;Database=AuthDB;User Id=sa;Password=$sqlPassword;TrustServerCertificate=True;"
  conteudo-conn: "Server=sqlserver;Database=ConteudoDB;User Id=sa;Password=$sqlPassword;TrustServerCertificate=True;"
  alunos-conn: "Server=sqlserver;Database=AlunosDB;User Id=sa;Password=$sqlPassword;TrustServerCertificate=True;"
  pagamentos-conn: "Server=sqlserver;Database=PagamentosDB;User Id=sa;Password=$sqlPassword;TrustServerCertificate=True;"
"@ | Set-Content -Path $secretFile -NoNewline
    }
}

if (-not (Test-Path $secretFile)) {
    $secretFile = "k8s/secrets/app-secrets.sample.yaml"
}

if (-not (Test-Path $secretFile)) {
    Write-Host "ERRO: Arquivo de secrets nao encontrado (k8s/secrets/app-secrets.yaml ou k8s/secrets/app-secrets.sample.yaml)." -ForegroundColor $ColorError
    exit 1
}

$secretContent = Get-Content $secretFile -Raw
if ($secretContent -match "CHANGE_ME") {
    Write-Host "ERRO: Secrets com placeholders 'CHANGE_ME'. Edite o arquivo antes de aplicar." -ForegroundColor $ColorError
    Write-Host "Arquivo: $secretFile" -ForegroundColor $ColorWarning
    exit 1
}

kubectl apply -f $secretFile | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRO: Falha ao aplicar secrets." -ForegroundColor $ColorError
    exit 1
}
Write-Host "OK" -ForegroundColor $ColorSuccess

# 4. Deploy
Write-Host ""
Write-Host "Aplicando manifests..." -ForegroundColor $ColorHeader

function Apply { param([string]$file, [string]$desc)
    Write-Host "$desc " -ForegroundColor $ColorStep -NoNewline
    kubectl apply -f $file | Out-Null
    if ($LASTEXITCODE -eq 0) { Write-Host "OK" -ForegroundColor $ColorSuccess } else { Write-Host "FALHA" -ForegroundColor $ColorError }
}

function ApplyFolder { param([string]$path, [string]$desc)
    Write-Host "$desc " -ForegroundColor $ColorStep -NoNewline
    $files = Get-ChildItem -Path $path -Filter *.yaml | Sort-Object Name
    foreach ($f in $files) {
        kubectl apply -f $f.FullName | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Host "FALHA" -ForegroundColor $ColorError; return }
    }
    Write-Host "OK" -ForegroundColor $ColorSuccess
}

ApplyFolder "k8s/pvc"         "1/4 - PVCs"
ApplyFolder "k8s/configmaps"  "2/4 - ConfigMaps"
ApplyFolder "k8s/deployments" "3/4 - Deployments"
Write-Host "Aguardando SqlServer ficar pronto..." -ForegroundColor $ColorWarning
kubectl wait --for=condition=ready pod -l app=sqlserver -n $Namespace --timeout=180s 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRO: SqlServer nao ficou pronto dentro do timeout." -ForegroundColor $ColorError
    exit 1
}
ApplyFolder "k8s/services"    "4/4 - Services"

# 3. Aguarda pods e inicia port-forwards
Write-Host ""
Write-Host "Aguardando pods ficarem prontos..." -ForegroundColor $ColorWarning
kubectl wait --for=condition=ready pod -l app=bff-api -n $Namespace --timeout=120s 2>$null
kubectl wait --for=condition=ready pod -l app=frontend -n $Namespace --timeout=120s 2>$null

Write-Host ""
Write-Host "Deploy concluido" -ForegroundColor $ColorSuccess
kubectl get pods -n $Namespace

Write-Host "Iniciando port-forwards..." -ForegroundColor $ColorHeader
Start-Process powershell -ArgumentList "-NoExit -Command kubectl port-forward svc/bff-api 5000:5000 -n $Namespace" -WindowStyle Minimized
Start-Process powershell -ArgumentList "-NoExit -Command kubectl port-forward deployment/frontend 4200:8080 -n $Namespace" -WindowStyle Minimized

Write-Host ""
Write-Host "Acessos:" -ForegroundColor $ColorSuccess
Write-Host "Frontend: http://localhost:4200"
Write-Host "BFF:      http://localhost:5000"
Write-Host ""
Write-Host "Pressione qualquer tecla para sair (tuneis continuam rodando)..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Write-Host "Finalizado." -ForegroundColor $ColorSuccess
