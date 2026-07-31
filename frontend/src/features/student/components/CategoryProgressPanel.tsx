import { Progress } from "@/components/ui/progress";
import type { CategoryProgress } from "../api/types";

interface CategoryProgressPanelProps {
  categories: CategoryProgress[];
}

/** Average progress per subject area, most-studied first. */
export function CategoryProgressPanel({ categories }: CategoryProgressPanelProps) {
  if (categories.length === 0) {
    return <p className="text-sm text-muted-foreground">Enrol in a course to see your subjects here.</p>;
  }

  return (
    <ul className="space-y-4">
      {categories.map((category) => (
        <li key={category.label}>
          <div className="mb-1.5 flex items-baseline justify-between gap-3 text-sm">
            <span className="truncate font-medium">{category.label}</span>
            <span className="shrink-0 text-xs text-muted-foreground">
              {category.courseCount} course{category.courseCount === 1 ? "" : "s"} ·{" "}
              <span className="font-semibold tabular-nums text-foreground">
                {category.averageProgressPercent}%
              </span>
            </span>
          </div>
          <Progress value={category.averageProgressPercent} label={category.label} size="sm" />
        </li>
      ))}
    </ul>
  );
}
