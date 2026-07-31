import { Skeleton } from "@/components/ui/skeleton";

/** Mirrors the real layout so the page does not jump when data lands. */
export function StudentDashboardSkeleton() {
  return (
    <div className="space-y-8">
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, index) => (
          <Skeleton key={index} className="h-[116px] rounded-[18px]" />
        ))}
      </div>

      <div>
        <Skeleton className="mb-4 h-6 w-44" />
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 3 }).map((_, index) => (
            <Skeleton key={index} className="h-[320px] rounded-[18px]" />
          ))}
        </div>
      </div>

      <div className="grid gap-4 lg:grid-cols-3">
        <Skeleton className="h-56 rounded-[18px] lg:col-span-2" />
        <Skeleton className="h-56 rounded-[18px]" />
      </div>
    </div>
  );
}
