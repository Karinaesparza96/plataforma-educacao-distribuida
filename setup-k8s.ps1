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

# 2. Deploy
Write-Host ""
Write-Host "Aplicando manifests..." -ForegroundColor $ColorHeader

function Apply { param([string]$file, [string]$desc)
    Write-Host "$desc " -ForegroundColor $ColorStep -NoNewline
    kubectl apply -f $file | Out-Null
    if ($LASTEXITCODE -eq 0) { Write-Host "OK" -ForegroundColor $ColorSuccess } else { Write-Host "FALHA" -ForegroundColor $ColorError }
}

Apply "k8s/01-base.yaml"     "1/4 - Base"
Apply "k8s/02-infra.yaml"    "2/4 - Infra"
Write-Host "Aguardando SqlServer (20 segundos)..." -ForegroundColor $ColorWarning
Start-Sleep -Seconds 20
Apply "k8s/03-apis.yaml"     "3/4 - APIs"
Apply "k8s/04-frontend.yaml" "4/4 - Frontend"

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
Start-Process powershell -ArgumentList "-NoExit -Command kubectl port-forward svc/frontend-service 4200:80 -n $Namespace" -WindowStyle Minimized

Write-Host ""
Write-Host "Acessos:" -ForegroundColor $ColorSuccess
Write-Host "Frontend: http://localhost:4200"
Write-Host "BFF:      http://localhost:5000"
Write-Host ""
Write-Host "Pressione qualquer tecla para sair (tuneis continuam rodando)..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Write-Host "Finalizado." -ForegroundColor $ColorSuccess