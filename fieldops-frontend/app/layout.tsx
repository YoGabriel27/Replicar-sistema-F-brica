import type { Metadata } from "next";
import "./globals.css";
import { QueryProvider } from "@/lib/query-client";
import { AppShell } from "@/components/layout/AppShell";

// NOTA: en este sandbox no hay acceso de red a fonts.googleapis.com, así
// que las fuentes se fijan por CSS (ver globals.css --font-display /
// --font-sans) en vez de next/font/google. Con acceso de red normal,
// reemplazar por:
//   import { Inter, Space_Grotesk } from "next/font/google";
// y aplicar sus `.variable` aquí — el resto del sistema de diseño
// (tokens en globals.css) no cambia.

export const metadata: Metadata = {
  title: "FieldOps",
  description: "Gestión de activos, mantenimiento y operaciones de campo",
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="es" className="h-full antialiased">
      <body className="min-h-full">
        <QueryProvider>
          <AppShell>{children}</AppShell>
        </QueryProvider>
      </body>
    </html>
  );
}
