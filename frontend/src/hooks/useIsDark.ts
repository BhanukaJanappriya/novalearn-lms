import { useEffect, useState } from "react";

/**
 * Tracks whether the `dark` class is present on <html>. Charts need concrete
 * colour values (not CSS vars), so they read this to pick the right theme ramp.
 * Kept in sync via a MutationObserver so toggling theme repaints charts live.
 */
export function useIsDark(): boolean {
  const [isDark, setIsDark] = useState(
    () => typeof document !== "undefined" && document.documentElement.classList.contains("dark"),
  );

  useEffect(() => {
    const root = document.documentElement;
    const observer = new MutationObserver(() => {
      setIsDark(root.classList.contains("dark"));
    });
    observer.observe(root, { attributes: true, attributeFilter: ["class"] });
    return () => observer.disconnect();
  }, []);

  return isDark;
}
