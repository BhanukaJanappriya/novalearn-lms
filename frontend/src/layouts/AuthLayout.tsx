import { Logo } from "@/components/Logo";
import { ThemeToggle } from "@/components/ThemeToggle";
import { AUTH_HERO_URL } from "@/lib/imagery";
import { AuthShowcase } from "@/features/auth/testimonials/AuthShowcase";
import { usePublicSettings } from "@/features/settings/api/queries";

interface AuthLayoutProps {
  title: string;
  subtitle: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
}

/** Two-pane authentication shell: brand/marketing panel + focused form card. */
export function AuthLayout({ title, subtitle, children, footer }: AuthLayoutProps) {
  const { data: platform } = usePublicSettings();
  const siteName = platform?.siteName ?? "NovaLearn";

  return (
    <div className="grid min-h-screen lg:grid-cols-2">
      {/*
        Brand panel. Marked as a primary-colored surface so the custom cursor (CursorField) swaps
        to a color that stays visible on it — see the "on-brand-surface" rule in index.css.
      */}
      <div
        data-cursor-surface="primary"
        className="relative hidden overflow-hidden bg-primary lg:flex lg:flex-col lg:justify-between lg:p-12"
      >
        {/*
          A photograph under the brand wash. It sits behind the existing gradients and is purely
          decorative, so if it never loads the panel looks exactly as it did before.
        */}
        <img
          src={AUTH_HERO_URL}
          alt=""
          aria-hidden
          loading="lazy"
          decoding="async"
          onLoad={(event) => event.currentTarget.classList.add("opacity-40")}
          className="absolute inset-0 h-full w-full object-cover opacity-0 transition-opacity duration-[1200ms] ease-out"
        />
        <div className="absolute inset-0 bg-primary/70" aria-hidden />
        <div
          className="absolute inset-0 opacity-30"
          style={{
            backgroundImage:
              "radial-gradient(circle at 20% 20%, #A78BFA 0, transparent 40%), radial-gradient(circle at 80% 80%, #FFFFFF 0, transparent 35%)",
          }}
          aria-hidden
        />
        <Logo className="relative text-brand-foreground [&_span]:text-brand-foreground" />

        {/*
          Marketing column: tagline, then social proof. `m-auto` on the inner block centres it
          when the panel is tall enough and lets it scroll (scrollbar hidden) rather than clip on
          a short viewport.
        */}
        <div className="relative flex min-h-0 flex-1 flex-col overflow-y-auto py-7 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden">
          <div className="m-auto flex w-full max-w-md flex-col gap-6">
            <div
              className="animate-fade-in [animation-fill-mode:backwards]"
              style={{ animationDelay: "0.05s" }}
            >
              <h1 className="text-3xl font-semibold leading-tight text-brand-foreground">
                Learn without limits.
              </h1>
              <p className="mt-2 text-sm text-brand-foreground/75">
                The learning platform universities and teams rely on.
              </p>
            </div>

            <AuthShowcase />
          </div>
        </div>

        <p className="relative text-sm text-brand-foreground/60">
          © {new Date().getFullYear()} {siteName}
          {platform?.supportEmail && <> · {platform.supportEmail}</>}
        </p>
      </div>

      {/* Form panel */}
      <div className="flex flex-col">
        <div className="flex items-center justify-between p-6">
          <div className="lg:hidden">
            <Logo />
          </div>
          <div className="ml-auto">
            <ThemeToggle />
          </div>
        </div>

        <div className="flex flex-1 items-center justify-center px-6 pb-12">
          <div className="w-full max-w-md animate-fade-in">
            <div className="mb-6">
              <h2 className="text-2xl font-semibold tracking-tight">{title}</h2>
              <p className="mt-1 text-sm text-muted-foreground">{subtitle}</p>
            </div>
            {children}
            {footer && <div className="mt-6 text-center text-sm text-muted-foreground">{footer}</div>}
          </div>
        </div>
      </div>
    </div>
  );
}
