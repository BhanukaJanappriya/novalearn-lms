interface Step {
  title: string;
  description: string;
}

const STEPS: Step[] = [
  {
    title: "Create your account",
    description: "Sign up in minutes. No credit card needed for free courses.",
  },
  {
    title: "Set up or join a course",
    description: "Ready-made templates and enrolment rules do the busywork.",
  },
  {
    title: "Track it in real time",
    description: "Dashboards built for every role, from student to dean.",
  },
];

/** A short numbered walkthrough between the hero and the social proof. */
export function HowItWorks() {
  return (
    <section>
      <h2 className="flex items-center gap-2 text-sm font-semibold uppercase tracking-wide text-brand-foreground/70">
        How it works
      </h2>
      <ol className="mt-3 space-y-4">
        {STEPS.map((step, i) => (
          <li key={step.title} className="flex gap-3">
            <span className="grid h-7 w-7 shrink-0 place-items-center rounded-full border border-brand-foreground/30 text-xs font-semibold text-brand-foreground">
              {i + 1}
            </span>
            <div>
              <p className="text-sm font-semibold text-brand-foreground">{step.title}</p>
              <p className="mt-0.5 text-xs text-brand-foreground/70">{step.description}</p>
            </div>
          </li>
        ))}
      </ol>
    </section>
  );
}
