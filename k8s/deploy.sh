#!/bin/bash

set -e

echo "==============================="
echo "🚀 Deploy da Plataforma Educacional"
echo "==============================="

K8S_DIR="./k8s"

echo "👉 Aplicando namespace..."
kubectl apply -f $K8S_DIR/namespace.yml

echo "👉 Subindo infraestrutura (Redis, RabbitMQ, SQL Server, PVCs)..."
kubectl apply -f $K8S_DIR/infra/infra-pvcs.yml
kubectl apply -f $K8S_DIR/infra/sqlserver-secret.yml
kubectl apply -f $K8S_DIR/infra/sqlserver-deployment.yml
kubectl apply -f $K8S_DIR/infra/rabbitmq-deployment.yml
kubectl apply -f $K8S_DIR/infra/redis-deployment.yml

echo "⏳ Aguardando SQL Server iniciar (pode levar 20–40 segundos)..."
kubectl wait --for=condition=ready pod -l app=sqlserver -n plataforma --timeout=180s || true

echo "👉 Subindo serviços (APIs + BFF + Frontend)..."
for svc in auth conteudo alunos pagamentos bff
do
    echo "➡️ Aplicando: $svc"

    kubectl apply -f $K8S_DIR/services/$svc/configmap.yml

    if [ -f "$K8S_DIR/services/$svc/secret.yml" ]; then
        kubectl apply -f $K8S_DIR/services/$svc/secret.yml
    fi

    kubectl apply -f $K8S_DIR/services/$svc/deployment.yml
    kubectl apply -f $K8S_DIR/services/$svc/service.yml
done


echo "➡️ Aplicando: frontend"
kubectl apply -f $K8S_DIR/services/frontend/deployment.yml
kubectl apply -f $K8S_DIR/services/frontend/service.yml


echo "👉 Aplicando ingress..."
kubectl apply -f $K8S_DIR/ingress.yml

echo "⏳ Aguardando pods ficarem prontos..."
kubectl wait --for=condition=ready pod -l app=bff-api -n plataforma --timeout=180s || true
kubectl wait --for=condition=ready pod -l app=frontend -n plataforma --timeout=180s || true

echo "==============================="
echo "🎉 Deploy concluído!"
echo "==============================="
echo ""
echo "🌐 Acesse a aplicação em:"
echo "👉 http://plataforma.local"
echo ""
echo "Se não abrir, adicione esta linha ao seu /etc/hosts (Linux/macOS):"
echo "127.0.0.1   plataforma.local"
echo ""
echo "No Windows (PowerShell admin):"
echo "notepad C:\\Windows\\System32\\drivers\\etc\\hosts"
echo ""
