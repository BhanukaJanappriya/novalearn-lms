import { forwardRef } from "react";
import { cn } from "@/lib/utils";

/**
 * A native checkbox, styled to match the form controls. Left as an uncontrolled `<input>` so it
 * drops straight into react-hook-form's `register()` the same way {@link FormField} does.
 */
export const Checkbox = forwardRef<HTMLInputElement, React.InputHTMLAttributes<HTMLInputElement>>(
  ({ className, ...props }, ref) => (
    <input
      ref={ref}
      type="checkbox"
      className={cn(
        "h-4 w-4 shrink-0 rounded border border-input bg-transparent accent-[hsl(var(--primary))]",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background",
        "disabled:cursor-not-allowed disabled:opacity-50",
        className,
      )}
      {...props}
    />
  ),
);
Checkbox.displayName = "Checkbox";
