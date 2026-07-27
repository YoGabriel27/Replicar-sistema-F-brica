"use client";

import { useQuery } from "@tanstack/react-query";
import { configuracionApi } from "@/lib/api-client/configuracion";
import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";

const STATUS_LABEL: Record<string, { label: string; variant: "signal" | "attention" | "muted" }> = {
  active: { label: "Activo", variant: "signal" },
  invited: { label: "Invitado", variant: "attention" },
  inactive: { label: "Inactivo", variant: "muted" },
};

export function UsersTable({ companyId }: { companyId: string }) {
  const { data: users, isLoading } = useQuery({
    queryKey: ["users", companyId],
    queryFn: () => configuracionApi.listUsersByCompany(companyId),
  });

  if (isLoading) {
    return <Card className="h-48 animate-pulse"><div /></Card>;
  }

  return (
    <Card>
      <table className="w-full text-left text-sm">
        <thead>
          <tr className="border-b border-border text-xs font-medium text-foreground-muted">
            <th className="px-5 py-3 font-medium">Email</th>
            <th className="px-5 py-3 font-medium">Rol en esta empresa</th>
            <th className="px-5 py-3 font-medium">Estado</th>
          </tr>
        </thead>
        <tbody>
          {users?.map((user) => (
            <tr key={user.userId} className="border-b border-border last:border-0">
              <td className="px-5 py-3 font-medium text-foreground">{user.email}</td>
              <td className="px-5 py-3 text-foreground-muted">{user.roleName}</td>
              <td className="px-5 py-3">
                <Badge variant={STATUS_LABEL[user.status].variant}>
                  {STATUS_LABEL[user.status].label}
                </Badge>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </Card>
  );
}
