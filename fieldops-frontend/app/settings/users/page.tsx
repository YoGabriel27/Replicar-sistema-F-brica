import { UsersTable } from "@/features/configuracion/components/UsersTable";
import { InviteUserForm } from "@/features/configuracion/components/InviteUserForm";

const CURRENT_COMPANY_ID = "c-1"; // placeholder — vendrá del TenantContext resuelto en el servidor

export default function UsersPage() {
  return (
    <div className="mx-auto max-w-3xl px-8 py-10">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="font-display text-2xl font-bold text-foreground">Usuarios y roles</h1>
          <p className="mt-1 text-sm text-foreground-muted">
            El rol es específico de esta empresa — la misma persona puede
            tener otro rol en otra empresa que administre.
          </p>
        </div>
      </div>

      <div className="mb-4">
        <InviteUserForm companyId={CURRENT_COMPANY_ID} />
      </div>

      <UsersTable companyId={CURRENT_COMPANY_ID} />
    </div>
  );
}
