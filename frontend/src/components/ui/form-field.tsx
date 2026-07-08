import { forwardRef } from "react";
import { Input, type InputProps } from "./input";
import { Label } from "./label";

interface FormFieldProps extends InputProps {
  label: string;
  error?: string;
}

/** Label + input + inline validation error, wired for accessibility. */
export const FormField = forwardRef<HTMLInputElement, FormFieldProps>(
  ({ label, error, id, name, ...props }, ref) => {
    const fieldId = id ?? name;
    const errorId = error ? `${fieldId}-error` : undefined;

    return (
      <div className="space-y-1.5">
        <Label htmlFor={fieldId}>{label}</Label>
        <Input
          ref={ref}
          id={fieldId}
          name={name}
          aria-invalid={error ? true : undefined}
          aria-describedby={errorId}
          {...props}
        />
        {error && (
          <p id={errorId} className="text-xs font-medium text-destructive">
            {error}
          </p>
        )}
      </div>
    );
  },
);
FormField.displayName = "FormField";
