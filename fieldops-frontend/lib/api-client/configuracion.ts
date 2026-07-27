import { Company, CompanyUser, Membership } from "@/features/configuracion/types";

/**
 * Cliente del módulo Configuración. Contra el backend real, estas
 * funciones llaman /api/v1/companies, /api/v1/users/{id}/memberships, etc.
 * (06-Modules/07-Configuracion/README.md §APIs). Como el backend .NET no
 * corre en este entorno, se simulan con datos de ejemplo — reemplazar
 * por fetch() real contra 08-Backend/ al integrar.
 */

const MOCK_DELAY = 300;

const mockMemberships: Membership[] = [
  {
    membershipId: "m-1",
    companyId: "c-1",
    companyName: "Protecnus Seguridad SRL",
    roleName: "Admin",
    isActive: true,
  },
  {
    membershipId: "m-2",
    companyId: "c-2",
    companyName: "Mantenimiento Austral SA",
    roleName: "Supervisor",
    isActive: true,
  },
];

const mockUsers: CompanyUser[] = [
  { userId: "u-1", email: "admin@protecnus.example", roleName: "Admin", status: "active" },
  { userId: "u-2", email: "supervisor@protecnus.example", roleName: "Supervisor", status: "active" },
  { userId: "u-3", email: "nuevo.tecnico@protecnus.example", roleName: "Técnico", status: "invited" },
];

function delay<T>(value: T): Promise<T> {
  return new Promise((resolve) => setTimeout(() => resolve(value), MOCK_DELAY));
}

export const configuracionApi = {
  listMembershipsByUser: (_userId: string): Promise<Membership[]> => delay(mockMemberships),

  getCompanyById: (companyId: string): Promise<Company> =>
    delay({
      id: companyId,
      name: mockMemberships.find((m) => m.companyId === companyId)?.companyName ?? "Empresa",
      partnerId: "p-1",
      countryCode: "AR",
      defaultCurrency: "ARS",
    }),

  listUsersByCompany: (_companyId: string): Promise<CompanyUser[]> => delay(mockUsers),

  inviteUser: (_companyId: string, email: string, _roleId: string): Promise<CompanyUser> =>
    delay({ userId: `u-${Date.now()}`, email, roleName: "Técnico", status: "invited" }),
};
