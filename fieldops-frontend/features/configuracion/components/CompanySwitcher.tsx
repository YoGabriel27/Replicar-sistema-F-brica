"use client";

import { useQuery } from "@tanstack/react-query";
import { configuracionApi } from "@/lib/api-client/configuracion";
import { useUiStore } from "@/stores/ui-store";
import { useState } from "react";
import { ChevronsUpDown, Check } from "lucide-react";
import { cn } from "@/lib/utils";

/**
 * El elemento "señal": un usuario con Membership en más de una Company
 * (06-Modules/07-Configuracion/README.md — reventa/partner confirmado)
 * nunca debe dudar en qué empresa está actuando. El punto de color
 * pulsante junto al nombre es la empresa ACTIVA de la sesión — el mismo
 * concepto de "señal operativa" que reaparece en toda la UI (estado de
 * activos, OT, conectividad del técnico).
 */
export function CompanySwitcher({ currentUserId }: { currentUserId: string }) {
  const { companySwitcherOpen, toggleCompanySwitcher, closeCompanySwitcher } = useUiStore();
  const [activeCompanyId, setActiveCompanyId] = useState<string | null>(null);

  const { data: memberships, isLoading } = useQuery({
    queryKey: ["memberships", currentUserId],
    queryFn: () => configuracionApi.listMembershipsByUser(currentUserId),
  });

  const active = memberships?.find((m) => m.companyId === activeCompanyId) ?? memberships?.[0];

  if (isLoading || !memberships) {
    return <div className="h-9 w-48 animate-pulse rounded-lg bg-sidebar-muted/20" />;
  }

  return (
    <div className="relative">
      <button
        onClick={toggleCompanySwitcher}
        className="flex w-56 items-center gap-2.5 rounded-lg border border-white/10 bg-white/5 px-3 py-2 text-left hover:bg-white/10"
      >
        <span className="relative flex h-2 w-2 shrink-0">
          <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-signal opacity-75" />
          <span className="relative inline-flex h-2 w-2 rounded-full bg-signal" />
        </span>
        <span className="flex-1 truncate text-sm font-medium text-sidebar-foreground">
          {active?.companyName}
        </span>
        <ChevronsUpDown className="h-3.5 w-3.5 text-sidebar-muted" />
      </button>

      {companySwitcherOpen && (
        <>
          <div className="fixed inset-0 z-10" onClick={closeCompanySwitcher} />
          <div className="absolute left-0 top-full z-20 mt-1.5 w-72 overflow-hidden rounded-lg border border-border bg-surface shadow-lg">
            <div className="border-b border-border px-3 py-2 text-xs font-medium text-foreground-muted">
              Tus empresas ({memberships.length})
            </div>
            {memberships.map((m) => (
              <button
                key={m.membershipId}
                onClick={() => {
                  setActiveCompanyId(m.companyId);
                  closeCompanySwitcher();
                }}
                className={cn(
                  "flex w-full items-center gap-2 px-3 py-2.5 text-left text-sm hover:bg-background",
                  m.companyId === active?.companyId && "bg-signal/5"
                )}
              >
                <div className="flex-1">
                  <div className="font-medium text-foreground">{m.companyName}</div>
                  <div className="text-xs text-foreground-muted">{m.roleName}</div>
                </div>
                {m.companyId === active?.companyId && <Check className="h-4 w-4 text-signal" />}
              </button>
            ))}
          </div>
        </>
      )}
    </div>
  );
}
