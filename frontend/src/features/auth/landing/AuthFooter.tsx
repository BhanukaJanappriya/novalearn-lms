import { Link } from "react-router-dom";
import { usePublicSettings } from "@/features/settings/api/queries";

/** Closing section of the brand panel's scroll — the actual footer of this long-scrolling page. */
export function AuthFooter() {
  const { data: platform } = usePublicSettings();
  const siteName = platform?.siteName ?? "NovaLearn";

  return (
    <footer className="space-y-3 border-t border-brand-foreground/15 pt-6 text-xs text-brand-foreground/60">
      <div className="flex flex-wrap items-center gap-x-4 gap-y-1.5">
        <Link
          to="/terms"
          target="_blank"
          rel="noreferrer"
          className="transition-colors hover:text-brand-foreground hover:underline"
        >
          Terms of Service
        </Link>
        <Link
          to="/privacy"
          target="_blank"
          rel="noreferrer"
          className="transition-colors hover:text-brand-foreground hover:underline"
        >
          Privacy Policy
        </Link>
        {platform?.supportEmail && (
          <a
            href={`mailto:${platform.supportEmail}`}
            className="transition-colors hover:text-brand-foreground hover:underline"
          >
            {platform.supportEmail}
          </a>
        )}
      </div>
      <p>
        © {new Date().getFullYear()} {siteName}. All rights reserved.
      </p>
    </footer>
  );
}
