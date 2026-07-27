"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { configuracionApi } from "@/lib/api-client/configuracion";
import { Button } from "@/components/ui/button";
import { useState } from "react";
import { UserPlus } from "lucide-react";

/**
 * 06-Modules/07-Configuracion/README.md §Forms — "Invitar usuario:
 * email, empresa a la que se le da acceso, rol inicial para esa
 * empresa". El comando InviteUser real (08-Backend/) valida
 * UserAlreadyExists — aquí solo se simula el éxito.
 */
export function InviteUserForm({ companyId }: { companyId: string }) {
  const [email, setEmail] = useState("");
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: () => configuracionApi.inviteUser(companyId, email, "role-tecnico"),
    onSuccess: () => {
      setEmail("");
      queryClient.invalidateQueries({ queryKey: ["users", companyId] });
    },
  });

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        if (email) mutation.mutate();
      }}
      className="flex items-center gap-2"
    >
      <input
        type="email"
        required
        placeholder="nombre@empresa.com"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        className="w-64 rounded-lg border border-border bg-surface px-3 py-2 text-sm outline-none focus:border-signal focus:ring-1 focus:ring-signal"
      />
      <Button type="submit" disabled={mutation.isPending}>
        <UserPlus className="h-4 w-4" />
        {mutation.isPending ? "Invitando…" : "Invitar usuario"}
      </Button>
    </form>
  );
}
