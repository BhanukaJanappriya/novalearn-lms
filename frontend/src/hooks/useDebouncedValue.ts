import { useEffect, useState } from "react";

/**
 * Returns `value` after it has stopped changing for `delayMs`. Used to keep search
 * inputs responsive while limiting how often the API is queried.
 */
export function useDebouncedValue<T>(value: T, delayMs = 350): T {
  const [debounced, setDebounced] = useState(value);

  useEffect(() => {
    const timer = window.setTimeout(() => setDebounced(value), delayMs);
    return () => window.clearTimeout(timer);
  }, [value, delayMs]);

  return debounced;
}
