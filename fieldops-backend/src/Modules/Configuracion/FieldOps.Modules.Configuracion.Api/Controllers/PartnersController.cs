using FieldOps.Modules.Configuracion.Application.Commands.CreatePartner;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FieldOps.Modules.Configuracion.Api.Controllers;

[ApiController]
[Route("api/v1/partners")]
[Authorize] // requiere rol de soporte de plataforma o flujo de alta comercial — política concreta en 13-Security/
public sealed class PartnersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreatePartnerCommand command, CancellationToken ct)
    {
        var partnerId = await mediator.Send(command, ct);
        return Ok(new { partnerId });
    }
}
