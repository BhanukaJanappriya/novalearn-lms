import { Link, type LinkProps } from "react-router-dom";
import type { VariantProps } from "class-variance-authority";
import { buttonVariants } from "./button";
import { cn } from "@/lib/utils";

export interface LinkButtonProps extends LinkProps, VariantProps<typeof buttonVariants> {}

/**
 * A router link styled as a button. Keeps proper navigation semantics (a real anchor)
 * instead of nesting an anchor inside a button element.
 */
export function LinkButton({ className, variant, size, ...props }: LinkButtonProps) {
  return <Link className={cn(buttonVariants({ variant, size, className }))} {...props} />;
}
