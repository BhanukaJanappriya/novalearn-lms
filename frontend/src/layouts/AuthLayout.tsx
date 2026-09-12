import { useRef } from "react";
import { Logo } from "@/components/Logo";
import { ThemeToggle } from "@/components/ThemeToggle";
import { AUTH_HERO_URL } from "@/lib/imagery";
import { AuthFooter } from "@/features/auth/landing/AuthFooter";
import { FeatureHighlights } from "@/features/auth/landing/FeatureHighlights";
import { HowItWorks } from "@/features/auth/landing/HowItWorks";
import { MorphingBlobs } from "@/features/auth/landing/MorphingBlobs";
import { ScrollCue } from "@/features/auth/landing/ScrollCue";
import { ScrollReveal } from "@/features/auth/landing/ScrollReveal";
import { AuthShowcase } from "@/features/auth/testimonials/AuthShowcase";

interface AuthLayoutProps {
  title: string;
  subtitle: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
}

/**
 * Two-pane authentication shell: a long-scrolling brand/marketing panel (hero, social proof,
 * feature highlights, its own footer) alongside a focused, always-visible form card. The panel
 * scrolls independently of the page — the sign-in form never moves — so a visitor can explore the
 * marketing side at their own pace without it getting between them and signing in.
 */
export function AuthLayout({ title, subtitle, children, footer }: AuthLayoutProps) {
  const scrollRef = useRef<HTMLDivElement>(null);

  return (
    // lg:h-screen (on top of the min-h-screen every width gets) pins this row to exactly one
    // viewport at desktop widths, so the brand panel's own overflow-y-auto has a real height
    // budget to scroll *within* instead of just growing the whole row past the viewport — which
    // is what actually happens with only a min-height once that column's content runs long.
    <div className="grid min-h-screen lg:h-screen lg:grid-cols-2">
      {/*
        Brand panel. Marked as a primary-colored surface so the custom cursor (CursorField) swaps
        to a color that stays visible on it — see the "on-brand-surface" rule in index.css.
      */}
      <div
        data-cursor-surface="primary"
        className="relative hidden overflow-hidden bg-primary lg:flex lg:flex-col"
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
        <MorphingBlobs />

        {/* A small non-scrolling header, like a landing page's nav bar. */}
        <div className="relative flex items-center px-12 pt-10">
          <Logo tone="inverted" className="text-brand-foreground" />
        </div>

        {/*
          The long scrollable page: hero, social proof, feature highlights, footer. Scrollbar
          hidden for a cleaner look — ScrollCue tells a visitor there is more below the fold.
        */}
        <div
          ref={scrollRef}
          className="relative min-h-0 flex-1 overflow-y-auto px-12 pb-10 pt-8 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden"
        >
          <div className="mx-auto flex max-w-md flex-col gap-14">
            <ScrollReveal>
              <h1 className="text-3xl font-semibold leading-tight text-brand-foreground">
                Learn without limits.
              </h1>
              <p className="mt-2 text-sm text-brand-foreground/75">
                The learning platform universities and teams rely on.
              </p>
            </ScrollReveal>

            <ScrollReveal delay={80}>
              <HowItWorks />
            </ScrollReveal>

            <ScrollReveal delay={80}>
              <AuthShowcase />
            </ScrollReveal>

            <ScrollReveal delay={100}>
              <FeatureHighlights />
            </ScrollReveal>

            <ScrollReveal delay={120}>
              <AuthFooter />
            </ScrollReveal>
          </div>
        </div>

        <ScrollCue targetRef={scrollRef} />
      </div>

      {/*
        Form panel. Now that the brand panel's lg:h-screen hard-caps the row's height, this side
        needs its own overflow-y-auto too — the register form (name, email, password with its
        strength checklist, terms checkbox) can run taller than a short laptop screen.
      */}
      <div className="flex flex-col overflow-y-auto">
        <div className="flex items-center justify-between p-6">
          <div className="lg:hidden">
            <Logo />
          </div>
          <div className="ml-auto">
            <ThemeToggle />
          </div>
        </div>

        {/* m-auto rather than items-center/justify-center: centers when it fits, and — unlike
            centering on the scroll container itself — never leaves the top of an overflowing
            form inaccessible above the fold. Same trick as the brand panel's own scroll area. */}
        <div className="flex flex-1 px-6 pb-12">
          <div className="m-auto w-full max-w-md animate-fade-in">
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
