import { Card, CardHeader, CardBody } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import Link from "next/link";
import { ArrowRight } from "lucide-react";

/**
 * Ruta raíz del módulo Configuración
 * (06-Modules/07-Configuracion/README.md §Screens — "Configuración de
 * Empresa"). Datos de la Company activa vendrían del TenantContext
 * resuelto server-side contra el backend real.
 */
export default function SettingsPage() {
  return (
    <div className="mx-auto max-w-3xl px-8 py-10">
      <h1 className="font-display text-2xl font-bold text-foreground">Configuración</h1>
      <p className="mt-1 text-sm text-foreground-muted">
        Datos generales, plan y usuarios de esta empresa.
      </p>

      <Card className="mt-6">
        <CardHeader className="flex items-center justify-between">
          <span className="text-sm font-medium text-foreground">Protecnus Seguridad SRL</span>
          <Badge variant="signal">Plan Operativo</Badge>
        </CardHeader>
        <CardBody className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <div className="text-foreground-muted">País</div>
            <div className="mt-0.5 font-medium text-foreground">Argentina</div>
          </div>
          <div>
            <div className="text-foreground-muted">Moneda base</div>
            <div className="mt-0.5 font-medium text-foreground">ARS</div>
          </div>
        </CardBody>
      </Card>

      <Link
        href="/settings/users"
        className="mt-4 flex items-center justify-between rounded-xl border border-border bg-surface px-5 py-4 text-sm hover:border-signal/40"
      >
        <div>
          <div className="font-medium text-foreground">Usuarios y roles</div>
          <div className="mt-0.5 text-foreground-muted">
            Invitar personas y gestionar su acceso a esta empresa
          </div>
        </div>
        <ArrowRight className="h-4 w-4 text-foreground-muted" />
      </Link>
    </div>
  );
}
