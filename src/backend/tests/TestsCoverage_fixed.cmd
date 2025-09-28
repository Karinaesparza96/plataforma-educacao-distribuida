@echo off
setlocal
echo === Limpando relatorios antigos ===
if exist tests\coverage-report rmdir /s /q tests\coverage-report

echo === Limpando testes antigos ===
set "testesCaminho=.\tests"
for /d /r %testesCaminho% %%d in (TestResults) do (
    if exist "%%d" (
        echo Excluindo pasta: %%d
        rd /s /q "%%d"
    )
)

echo === Executando testes Core.Tests ===
dotnet test tests\Core.Tests --collect:"XPlat Code Coverage"

echo === Executando testes BFF.IntegrationTests ===
dotnet test tests\BFF.IntegrationTests --collect:"XPlat Code Coverage"

echo === Executando testes BFF.UnitTests ===
dotnet test tests\BFF.UnitTests --collect:"XPlat Code Coverage"

echo === Executando testes Auth.IntegrationTests ===
dotnet test tests\Auth.IntegrationTests --collect:"XPlat Code Coverage"

echo === Executando testes Auth.UnitTests ===
dotnet test tests\Auth.UnitTests --collect:"XPlat Code Coverage"

echo === Executando testes Conteudo.IntegrationTests ===
dotnet test tests\Conteudo.IntegrationTests --collect:"XPlat Code Coverage"

echo === Executando testes Conteudo.UnitTests ===
dotnet test tests\Conteudo.UnitTests --collect:"XPlat Code Coverage"

echo === Executando testes Alunos.IntegrationTests ===
dotnet test tests\Alunos.IntegrationTests --collect:"XPlat Code Coverage"

echo === Executando testes Alunos.Tests ===
dotnet test tests\Alunos.Tests --collect:"XPlat Code Coverage"

echo === Executando testes Pagamentos.UnitTests ===
dotnet test tests\Pagamentos.UnitTests --collect:"XPlat Code Coverage"

echo === Executando testes Pagamentos.IntegrationTests ===
dotnet test tests\Pagamentos.IntegrationTests --collect:"XPlat Code Coverage"

echo === Gerando Relatório de Cobertura ===
reportgenerator -reports:tests\**\coverage.cobertura.xml -targetdir:tests\coverage-report -reporttypes:Html

echo === Abrindo o Relatório ===
if exist tests\coverage-report\index.html (
    start tests\coverage-report\index.html
) else (
    echo !!! ERRO: Relatório HTML não foi gerado !!!
)

endlocal
