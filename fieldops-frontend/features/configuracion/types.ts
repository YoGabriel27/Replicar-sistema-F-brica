/**
 * Tipos del módulo Configuración — en producción se generan desde el
 * OpenAPI de 08-Backend/ (07-Frontend/README.md §API), nunca a mano.
 * Aquí están escritos a mano porque el backend no corre en este entorno
 * (ver README del scaffold).
 */
export interface Company {
  id: string;
  name: string;
  partnerId: string | null;
  countryCode: string;
  defaultCurrency: string;
}

export interface Membership {
  membershipId: string;
  companyId: string;
  companyName: string;
  roleName: string;
  isActive: boolean;
}

export interface CompanyUser {
  userId: string;
  email: string;
  roleName: string;
  status: "active" | "invited" | "inactive";
}
