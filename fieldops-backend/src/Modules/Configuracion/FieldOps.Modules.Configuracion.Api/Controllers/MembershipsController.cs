using FieldOps.Modules.Configuracion.Application.Commands.CreateMembership;
using FieldOps.Modules.Configuracion.Application.Commands.InviteUser;
using FieldOps.Modules.Configuracion.Application.Commands.RevokeMembership;
using FieldOps.Modules.Configuracion.Application.Queries.ListMembershipsByUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FieldOps.Modules.Configuracion.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public sealed class MembershipsController(IMediator mediator) : ControllerBase
{
    [HttpPost("users/invite")]
    public async Task<IActionResult> InviteUser(InviteUserCommand command, CancellationToken ct)
    {
        try
        {
            var userId = await mediator.Send(command, ct);
            return Ok(new { userId });
        }
        catch (InvalidOperationException ex) when (ex.Message == "UserAlreadyExists")
        {
            return Conflict(new { error = "UserAlreadyExists" });
        }
    }

    [HttpPost("memberships")]
    public async Task<IActionResult> CreateMembership(CreateMembershipCommand command, CancellationToken ct)
    {
        try
        {
            var membershipId = await mediator.Send(command, ct);
            return Ok(new { membershipId });
        }
        catch (InvalidOperationException ex) when (ex.Message == "MembershipAlreadyExists")
        {
            return Conflict(new { error = "MembershipAlreadyExists" });
        }
    }

    [HttpDelete("memberships/{membershipId:guid}")]
    public async Task<IActionResult> RevokeMembership(Guid membershipId, CancellationToken ct)
    {
        await mediator.Send(new RevokeMembershipCommand(membershipId), ct);
        return NoContent();
    }

    /// <summary>Alimenta el Selector de Empresa Activa del frontend.</summary>
    [HttpGet("users/{userId:guid}/memberships")]
    public async Task<IActionResult> ListByUser(Guid userId, CancellationToken ct)
    {
        var memberships = await mediator.Send(new ListMembershipsByUserQuery(userId), ct);
        return Ok(memberships);
    }
}
