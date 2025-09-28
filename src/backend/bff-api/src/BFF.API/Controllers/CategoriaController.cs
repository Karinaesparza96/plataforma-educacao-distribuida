using BFF.API.Models.Request;
using BFF.API.Models.Response;
using BFF.API.Settings;
using BFF.Application.Interfaces.Services;
using Core.Communication;
using Core.Mediator;
using Core.Messages;
using Core.Notification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace BFF.API.Controllers;

/// <summary>
/// Controller de alunos no BFF - Orquestra chamadas para Alunos API
/// </summary>
[ExcludeFromCodeCoverage]
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriaController(
    IApiClientService apiClient,
    IOptions<ApiSettings> apiSettings,
    IMediatorHandler mediator,
    INotificationHandler<DomainNotificacaoRaiz> notifications,
    INotificador notificador,
    ILogger<AuthController> logger) : BffController(mediator, notifications, notificador)
{
    private readonly ApiSettings _apiSettings = apiSettings.Value;

    /// <summary>
    /// Cadastrar nova categoria
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseResult<Guid?>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseResult<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseResult<string>), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> AdicionarCategoria([FromBody] CategoriaRequest request)
    {
        try
        {
            apiClient.SetBaseAddress(_apiSettings.ConteudoApiUrl);
            var result = await apiClient.PostAsyncWithActionResult<CategoriaRequest, ResponseResult<CategoriaResponse>>(
                "/api/categoria", request, "Categoria cadastrada com sucesso");

            if (result.Success && result.Data != null && result.Data.Data != null)
            {
                return RespostaPadraoApi(HttpStatusCode.Created, result.Data.Data, result.Message);
            }

            if (result.ErrorContent != null)
            {
                return StatusCode(result.StatusCode, result.ErrorContent);
            }

            return ProcessarErro(HttpStatusCode.BadRequest, result.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao cadastrar categoria");
            return ProcessarErro(HttpStatusCode.InternalServerError, "Erro interno do servidor");
        }
    }
}
