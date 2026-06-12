using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PickupOrderSystem.Application.DTOs;
using PickupOrderSystem.Application.Interfaces;
using PickupOrderSystem.Domain.Exceptions;

namespace PickupOrderSystem.API.Endpoints;

public static class PickupRequestEndpoints
{
    public static IEndpointRouteBuilder MapPickupRequestEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/pickup-requests", async (
            ClaimsPrincipal user,
            IPickupRequestService service,
            string? status,
            string? clientName,
            DateOnly? from,
            DateOnly? to,
            int page = 1,
            int pageSize = 10) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var role = user.FindFirstValue(ClaimTypes.Role)!;
            var result = await service.GetListAsync(
                role == "Cliente" ? userId : null,
                status, clientName, from, to, page, pageSize);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Solicitações de Coleta")
        .WithSummary("Lista solicitações com filtros e paginação")
        .WithDescription("""
            Retorna uma lista paginada de solicitações de coleta.

            - **Colaborador**: visualiza todas as solicitações. Pode filtrar por nome de cliente.
            - **Cliente**: visualiza apenas as próprias solicitações. Filtro por cliente é ignorado.

            **Filtros disponíveis:** `status`, `clientName`, `from` (data inicial), `to` (data final), `page`, `pageSize` (máx. 50).
            """)
        .WithOpenApi();

        app.MapGet("/pickup-requests/{id:guid}", async (Guid id, ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var role = user.FindFirstValue(ClaimTypes.Role)!;
            try
            {
                var dto = await service.GetByIdAsync(id, userId, role);
                return Results.Ok(dto);
            }
            catch (NotFoundException) { return Results.NotFound(); }
            catch (ForbiddenException) { return Results.Forbid(); }
        })
        .RequireAuthorization()
        .WithTags("Solicitações de Coleta")
        .WithSummary("Busca solicitação por ID")
        .WithDescription("Retorna os detalhes completos de uma solicitação, incluindo histórico de status, atribuição ativa e ocorrências. Cliente só pode visualizar as próprias solicitações.")
        .WithOpenApi();

        app.MapPost("/pickup-requests", async (CreatePickupRequestRequest body, ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
            try
            {
                var (newId, identificationNumber) = await service.CreateAsync(body, userId, userName);
                return Results.Created($"/pickup-requests/{newId}", new { Id = newId, IdentificationNumber = identificationNumber });
            }
            catch (BusinessRuleException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization("ColaboradorOuCliente")
        .WithTags("Solicitações de Coleta")
        .WithSummary("Cria nova solicitação de coleta")
        .WithDescription("Cria uma nova solicitação com status inicial **Aberta**. Disponível para Colaboradores e Clientes. Motoristas não têm acesso a este endpoint.")
        .WithOpenApi();

        app.MapPatch("/pickup-requests/{id:guid}/status", async (Guid id, UpdateStatusRequest body, ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
            try
            {
                await service.UpdateStatusAsync(id, body.Status, userId, userName);
                return Results.NoContent();
            }
            catch (NotFoundException) { return Results.NotFound(); }
            catch (BusinessRuleException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization("Colaborador")
        .WithTags("Solicitações de Coleta")
        .WithSummary("Atualiza o status da solicitação")
        .WithDescription("""
            Avança o status da solicitação conforme o fluxo permitido. Restrito a Colaboradores.

            **Transições válidas:**
            - `Aberta` → `Cancelada`
            - `Atribuida` → `EmColeta`, `Cancelada`
            - `EmColeta` → `Coletado`, `FalhaNaColeta`, `Cancelada`
            - `Coletado` → `ACaminho`, `Cancelada`
            - `ACaminho` → `Concluida`, `Cancelada`
            - `FalhaNaColeta` → `AguardandoDecisao`, `Cancelada`
            - `AguardandoDecisao` → `Atribuida`, `Cancelada`
            """)
        .WithOpenApi();

        app.MapPost("/pickup-requests/{id:guid}/assign", async (Guid id, AssignRequest body, ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
            try
            {
                await service.AssignAsync(id, body, userId, userName);
                return Results.NoContent();
            }
            catch (NotFoundException) { return Results.NotFound(); }
            catch (BusinessRuleException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization("Colaborador")
        .WithTags("Solicitações de Coleta")
        .WithSummary("Atribui motorista e veículo à solicitação")
        .WithDescription("Atribui um motorista e veículo ativos à solicitação. Permitido apenas nos status **Aberta** e **AguardandoDecisao**. Encerra a atribuição anterior automaticamente. Restrito a Colaboradores.")
        .WithOpenApi();

        app.MapPost("/pickup-requests/{id:guid}/occurrences", async (Guid id, CreateOccurrenceRequest body, ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
            try
            {
                await service.RegisterOccurrenceAsync(id, body, userId, userName);
                return Results.Created($"/pickup-requests/{id}/occurrences", null);
            }
            catch (NotFoundException) { return Results.NotFound(); }
            catch (BusinessRuleException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization("Colaborador")
        .WithTags("Solicitações de Coleta")
        .WithSummary("Registra uma ocorrência na solicitação")
        .WithDescription("""
            Registra um evento ou intercorrência vinculado à solicitação. Restrito a Colaboradores.

            **Tipos válidos:** `Atraso`, `DanoNaCarga`, `AcessoNegado`, `EnderecoIncorreto`, `RemetenteIndisponivel`, `CargaDivergente`, `Outros`
            """)
        .WithOpenApi();

        app.MapPost("/pickup-requests/{id:guid}/fail", async (Guid id, RegisterFailureRequest body, ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
            try
            {
                await service.RegisterFailureAsync(id, body, userId, userName);
                return Results.NoContent();
            }
            catch (NotFoundException) { return Results.NotFound(); }
            catch (BusinessRuleException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization("Colaborador")
        .WithTags("Solicitações de Coleta")
        .WithSummary("Registra falha e altera status para FalhaNaColeta")
        .WithDescription("Operação atômica: registra uma ocorrência de falha e altera o status da solicitação para **FalhaNaColeta** em uma única transação. Permitido nos status **EmColeta**, **Coletado** e **ACaminho**. Restrito a Colaboradores.")
        .WithOpenApi();

        return app;
    }
}
