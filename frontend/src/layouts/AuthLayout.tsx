import { motion } from "framer-motion";
import { Logo } from "@/components/Logo";
import { ThemeToggle } from "@/components/ThemeToggle";
import { AUTH_HERO_URL } from "@/lib/imagery";

interface AuthLayoutProps {
  title: string;
  subtitle: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
}

/** Two-pane authentication shell: brand/marketing panel + focused form card. */
export function AuthLayout({ title, subtitle, children, footer }: AuthLayoutProps) {
  return (
    <div className="grid min-h-screen lg:grid-cols-2">
      {/* Brand panel */}
      <div className="relative hidden overflow-hidden bg-primary lg:flex lg:flex-col lg:justify-between lg:p-12">
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
        <Logo className="relative text-primary-foreground [&_span]:text-primary-foreground" />
        <motion.div
          className="relative max-w-md"
          initial={{ opacity: 0, y: 14 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, delay: 0.1, ease: [0.22, 1, 0.36, 1] }}
        >
          <h1 className="text-3xl font-semibold leading-tight text-primary-foreground">
            Learn without limits.
          </h1>
          <p className="mt-3 text-primary-foreground/80">
            A modern learning platform for universities and teams — courses, assessments and
            insights, in one elegant place.
          </p>
        </motion.div>
        <p className="relative text-sm text-primary-foreground/60">
          © {new Date().getFullYear()} NovaLearn
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
          <motion.div
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.25, ease: "easeOut" }}
            className="w-full max-w-md"
          >
            <div className="mb-6">
              <h2 className="text-2xl font-semibold tracking-tight">{title}</h2>
              <p className="mt-1 text-sm text-muted-foreground">{subtitle}</p>
            </div>
            {children}
            {footer && <div className="mt-6 text-center text-sm text-muted-foreground">{footer}</div>}
          </motion.div>
        </div>
      </div>
    </div>
  );
}
