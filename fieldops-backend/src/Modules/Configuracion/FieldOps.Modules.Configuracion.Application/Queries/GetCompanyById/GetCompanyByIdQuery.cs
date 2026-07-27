using FieldOps.Modules.Configuracion.Application.Abstractions;
using MediatR;

namespace FieldOps.Modules.Configuracion.Application.Queries.GetCompanyById;

public sealed record CompanyDto(Guid Id, string Name, Guid? PartnerId, string CountryCode, string DefaultCurrency);

public sealed record GetCompanyByIdQuery(Guid CompanyId) : IRequest<CompanyDto?>;

public sealed class GetCompanyByIdQueryHandler(
    IConfiguracionRepository repository
) : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
{
    public async Task<CompanyDto?> Handle(GetCompanyByIdQuery request, CancellationToken ct)
    {
        var company = await repository.GetCompanyByIdAsync(request.CompanyId, ct);
        return company is null
            ? null
            : new CompanyDto(company.Id, company.Name, company.PartnerId, company.CountryCode, company.DefaultCurrency);
    }
}
