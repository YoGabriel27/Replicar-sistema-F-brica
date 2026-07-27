import { cn } from "@/lib/utils";

type BadgeVariant = "default" | "signal" | "attention" | "critical" | "muted";

const variantClasses: Record<BadgeVariant, string> = {
  default: "bg-foreground/5 text-foreground",
  signal: "bg-signal/10 text-signal",
  attention: "bg-attention/10 text-attention",
  critical: "bg-critical/10 text-critical",
  muted: "bg-border text-foreground-muted",
};

export function Badge({
  children,
  variant = "default",
  className,
}: {
  children: React.ReactNode;
  variant?: BadgeVariant;
  className?: string;
}) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-medium",
        variantClasses[variant],
        className
      )}
    >
      {children}
    </span>
  );
}
