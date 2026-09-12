import { BarChart3, ShieldCheck, Sparkles, type LucideIcon } from "lucide-react";

interface Feature {
  icon: LucideIcon;
  title: string;
  description: string;
}

const FEATURES: Feature[] = [
  {
    icon: Sparkles,
    title: "Built for every course",
    description: "Content, assignments and quizzes, all under one roof.",
  },
  {
    icon: BarChart3,
    title: "Insight as you teach",
    description: "Live analytics surface what's not working before the exam does.",
  },
  {
    icon: ShieldCheck,
    title: "Enterprise-grade access",
    description: "Role-based permissions, audit trails and session controls.",
  },
];

/** Short "why NovaLearn" bullets for the brand panel's scroll, below the social proof. */
export function FeatureHighlights() {
  return (
    <section>
      <h2 className="flex items-center gap-2 text-sm font-semibold uppercase tracking-wide text-brand-foreground/70">
        Why teams choose NovaLearn
      </h2>
      <ul className="mt-3 space-y-2.5">
        {FEATURES.map((feature) => {
          const Icon = feature.icon;
          return (
            <li
              key={feature.title}
              className="flex items-start gap-3 rounded-xl border border-brand-foreground/15 bg-brand-foreground/5 p-3.5"
            >
              <span className="grid h-9 w-9 shrink-0 place-items-center rounded-lg bg-brand-foreground/15 text-brand-foreground">
                <Icon className="h-4 w-4" aria-hidden />
              </span>
              <div>
                <p className="text-sm font-semibold text-brand-foreground">{feature.title}</p>
                <p className="mt-0.5 text-xs text-brand-foreground/70">{feature.description}</p>
              </div>
            </li>
          );
        })}
      </ul>
    </section>
  );
}
