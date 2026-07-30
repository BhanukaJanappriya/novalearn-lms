import { Search, X } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import type { CourseLevel } from "@/features/courses/api/types";

const selectClass =
  "h-10 rounded-md border border-input bg-card px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";

const levels: CourseLevel[] = ["Beginner", "Intermediate", "Advanced"];

interface CatalogFiltersProps {
  search: string;
  category: string;
  level: CourseLevel | "";
  categories: string[];
  onSearchChange: (value: string) => void;
  onCategoryChange: (value: string) => void;
  onLevelChange: (value: CourseLevel | "") => void;
  onReset: () => void;
}

/** Search box plus category and level selects for the catalogue. */
export function CatalogFilters({
  search,
  category,
  level,
  categories,
  onSearchChange,
  onCategoryChange,
  onLevelChange,
  onReset,
}: CatalogFiltersProps) {
  const hasFilters = search !== "" || category !== "" || level !== "";

  return (
    <div className="flex flex-wrap items-center gap-3">
      <div className="relative min-w-[220px] flex-1">
        <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          value={search}
          onChange={(event) => onSearchChange(event.target.value)}
          placeholder="Search by title or code…"
          aria-label="Search courses"
          className="pl-9"
        />
      </div>

      <select
        value={category}
        onChange={(event) => onCategoryChange(event.target.value)}
        aria-label="Filter by category"
        className={selectClass}
      >
        <option value="">All categories</option>
        {categories.map((name) => (
          <option key={name} value={name}>
            {name}
          </option>
        ))}
      </select>

      <select
        value={level}
        onChange={(event) => onLevelChange(event.target.value as CourseLevel | "")}
        aria-label="Filter by level"
        className={selectClass}
      >
        <option value="">All levels</option>
        {levels.map((name) => (
          <option key={name} value={name}>
            {name}
          </option>
        ))}
      </select>

      {hasFilters && (
        <Button variant="ghost" size="sm" onClick={onReset}>
          <X className="h-4 w-4" />
          Clear
        </Button>
      )}
    </div>
  );
}
