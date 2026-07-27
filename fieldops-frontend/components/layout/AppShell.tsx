import Link from "next/link";
import {
  Wrench,
  Truck,
  Users,
  Building2,
  Receipt,
  BarChart3,
  Settings,
} from "lucide-react";
import { CompanySwitcher } from "@/features/configuracion/components/CompanySwitcher";

/**
 * Navegación 1:1 con los módulos de 06-Modules/ (07-Frontend/README.md
 * §Estructura de carpetas) — cada ítem es la ruta raíz de ese módulo.
 */
const NAV_ITEMS = [
  { href: "/work-orders", label: "Activos y OT", icon: Wrench },
  { href: "/dispatch", label: "Despacho", icon: Truck },
  { href: "/workforce", label: "Fuerza de trabajo", icon: Users },
  { href: "/clients", label: "Clientes", icon: Building2 },
  { href: "/billing", label: "Facturación", icon: Receipt },
  { href: "/bi", label: "BI", icon: BarChart3 },
  { href: "/settings", label: "Configuración", icon: Settings },
];

const CURRENT_USER_ID = "u-1"; // placeholder — vendrá de la sesión autenticada

export function AppShell({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex h-screen overflow-hidden">
      <aside className="flex w-64 shrink-0 flex-col bg-sidebar px-3 py-4">
        <div className="mb-5 px-2">
          <span className="font-display text-lg font-bold tracking-tight text-sidebar-foreground">
            FieldOps
          </span>
        </div>

        <div className="mb-4 px-1">
          <CompanySwitcher currentUserId={CURRENT_USER_ID} />
        </div>

        <nav className="flex flex-1 flex-col gap-0.5">
          {NAV_ITEMS.map(({ href, label, icon: Icon }) => (
            <Link
              key={href}
              href={href}
              className="flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium text-sidebar-muted transition-colors hover:bg-white/5 hover:text-sidebar-foreground"
            >
              <Icon className="h-4 w-4" strokeWidth={1.75} />
              {label}
            </Link>
          ))}
        </nav>
      </aside>

      <main className="flex-1 overflow-y-auto bg-background">{children}</main>
    </div>
  );
}
