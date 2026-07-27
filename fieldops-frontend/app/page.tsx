import Link from "next/link";

/**
 * Placeholder — el dashboard operativo real vive en 06-Modules/02-Operaciones
 * (aún no implementado en este scaffold). Configuración es el primer
 * módulo construido por ser la base de aislamiento multiempresa de todos
 * los demás (08-Backend/README.md).
 */
export default function HomePage() {
  return (
    <div className="flex h-full flex-col items-center justify-center px-8 text-center">
      <h1 className="font-display text-3xl font-bold text-foreground">FieldOps</h1>
      <p className="mt-2 max-w-md text-sm text-foreground-muted">
        Este scaffold implementa el módulo Configuración
        (Company · Partner · User · Membership · Role · Permission ·
        FeatureFlag). El resto de los módulos operativos se construyen
        siguiendo el mismo patrón.
      </p>
      <Link
        href="/settings"
        className="mt-6 rounded-lg bg-signal px-4 py-2 text-sm font-medium text-signal-foreground hover:bg-signal/90"
      >
        Ir a Configuración
      </Link>
    </div>
  );
}
