using FieldOps.Modules.Configuracion.Application.Commands.CreateCompany;
using FieldOps.Modules.Configuracion.Application.Queries.GetCompanyById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FieldOps.Modules.Configuracion.Api.Controllers;

/// <summary>
/// Versionado por URL (08-Backend/README.md, decisión confirmada).
/// CreateCompany es una de las pocas acciones que NO requiere Company
/// activa (es quien la crea) — el resto del módulo sí.
/// </summary>
[ApiController]
[Route("api/v1/companies")]
public sealed class CompaniesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous] // el onboarding de una empresa nueva no requiere sesión previa
    public async Task<IActionResult> Create(CreateCompanyCommand command, CancellationToken ct)
    {
        var companyId = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { companyId }, new { companyId });
    }

    [HttpGet("{companyId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid companyId, CancellationToken ct)
    {
        var company = await mediator.Send(new GetCompanyByIdQuery(companyId), ct);
        return company is null ? NotFound(new { error = "CompanyNotFound" }) : Ok(company);
    }
}
