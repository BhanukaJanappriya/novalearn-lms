import { cn } from "@/lib/utils";

interface LogoProps {
  className?: string;
  showWord?: boolean;
  /**
   * "default" splits the wordmark into a dark "Nova" and a purple "Learn", for light chrome.
   * "inverted" drops that split and lets the whole word inherit `currentColor` instead, for a
   * dark or brand-purple surface where purple-on-purple would make "Learn" disappear.
   */
  tone?: "default" | "inverted";
}

/** NovaLearn wordmark + glyph. The glyph mirrors public/logo.svg. */
export function Logo({ className, showWord = true, tone = "default" }: LogoProps) {
  return (
    <div className={cn("flex items-center gap-2.5", className)}>
      <svg viewBox="0 0 512 512" className="h-8 w-8" role="img" aria-label="NovaLearn logo">
        <defs>
          <linearGradient id="nova-logo-grad" x1="96" y1="96" x2="416" y2="416" gradientUnits="userSpaceOnUse">
            <stop stopColor="#A78BFA" />
            <stop offset="1" stopColor="#8B5CF6" />
          </linearGradient>
        </defs>
        <rect x="48" y="48" width="416" height="416" rx="96" fill="url(#nova-logo-grad)" />
        <path
          d="M170 356V156L342 356V156"
          stroke="#FFFFFF"
          strokeWidth="40"
          strokeLinecap="round"
          strokeLinejoin="round"
          fill="none"
        />
        <circle cx="342" cy="156" r="22" fill="#FFFFFF" />
      </svg>
      {showWord && (
        <span className="text-lg font-semibold tracking-tight">
          Nova
          <span className={tone === "inverted" ? undefined : "text-primary"}>Learn</span>
        </span>
      )}
    </div>
  );
}
