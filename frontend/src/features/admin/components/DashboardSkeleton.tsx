import { Skeleton } from "@/components/ui/skeleton";

/** Full-page loading state that mirrors the dashboard's real layout. */
export function DashboardSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-36 w-full rounded-[18px]" />

      <div className="grid grid-cols-2 gap-4 md:grid-cols-3 xl:grid-cols-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <div key={i} className="rounded-[18px] border border-border bg-card p-5 shadow-soft">
            <div className="flex justify-between">
              <Skeleton className="h-11 w-11 rounded-2xl" />
              <Skeleton className="h-5 w-12 rounded-full" />
            </div>
            <Skeleton className="mt-4 h-7 w-24" />
            <Skeleton className="mt-2 h-4 w-20" />
            <Skeleton className="mt-4 h-8 w-full" />
          </div>
        ))}
      </div>

      <div className="grid gap-4 lg:grid-cols-3">
        <Skeleton className="h-80 rounded-[18px] lg:col-span-2" />
        <Skeleton className="h-80 rounded-[18px]" />
      </div>
    </div>
  );
}
