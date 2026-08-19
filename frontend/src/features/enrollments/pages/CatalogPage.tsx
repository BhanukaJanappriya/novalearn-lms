import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { AnimatePresence, motion } from "framer-motion";
import { Compass, TriangleAlert } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { getApiErrorMessage } from "@/lib/apiError";
import { staggerContainer } from "@/lib/motion";
import { formatNumber } from "@/lib/format";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { useCourses } from "@/features/courses/api/queries";
import type { CourseLevel } from "@/features/courses/api/types";
import { useCourseCatalog, useEnrollInCourse } from "../api/queries";
import type { CatalogCourse, CatalogFilters as Filters } from "../api/types";
import { CatalogCard } from "../components/CatalogCard";
import { CatalogFilters } from "../components/CatalogFilters";
import { PaginationControls } from "@/components/ui/pagination";
import { useStartCheckout } from "@/features/payments/api/queries";

const PAGE_SIZE = 12;

export function CatalogPage() {
  // Deep links such as /catalog?search=CS101 land straight on a single course.
  const [searchParams] = useSearchParams();
  const [search, setSearch] = useState(() => searchParams.get("search") ?? "");
  const [category, setCategory] = useState("");
  const [level, setLevel] = useState<CourseLevel | "">("");
  const [page, setPage] = useState(1);

  const debouncedSearch = useDebouncedValue(search);

  // Any filter change puts the user back on the first page.
  useEffect(() => {
    setPage(1);
  }, [debouncedSearch, category, level]);

  const filters: Filters = useMemo(
    () => ({ search: debouncedSearch, category, level, page, pageSize: PAGE_SIZE }),
    [debouncedSearch, category, level, page],
  );

  const { data, isLoading, isError, isFetching, refetch } = useCourseCatalog(filters);
  const enroll = useEnrollInCourse();
  const checkout = useStartCheckout();

  // Category options come from the shared course list so they stay stable while paging.
  const { data: allCourses } = useCourses();
  const categories = useMemo(
    () =>
      [...new Set((allCourses ?? []).filter((c) => c.status === "Published").map((c) => c.category))].sort(
        (a, b) => a.localeCompare(b),
      ),
    [allCourses],
  );

  const resetFilters = () => {
    setSearch("");
    setCategory("");
    setLevel("");
  };

  // A priced course starts checkout and leaves the page entirely for Stripe's hosted payment
  // page; only a free course is enrolled directly here. The redirect itself never counts as
  // success — that is decided by the webhook, long after this function returns.
  const onEnroll = (course: CatalogCourse) => {
    if (course.price > 0) {
      checkout.mutate(course.id, {
        onSuccess: (session) => {
          window.location.href = session.checkoutUrl;
        },
      });
      return;
    }

    enroll.mutate(course.id);
  };

  const courses = data?.items ?? [];
  const isEmpty = !isLoading && courses.length === 0;

  return (
    <div className="min-h-screen">
      <LearnerHeader />

      <main className="mx-auto max-w-6xl space-y-6 px-6 py-10">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Course catalog</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {data
              ? `${formatNumber(data.totalCount)} published course${data.totalCount === 1 ? "" : "s"} available`
              : "Browse published courses and enroll in one click."}
          </p>
        </div>

        <CatalogFilters
          search={search}
          category={category}
          level={level}
          categories={categories}
          onSearchChange={setSearch}
          onCategoryChange={setCategory}
          onLevelChange={setLevel}
          onReset={resetFilters}
        />

        {enroll.isError && <Alert>{getApiErrorMessage(enroll.error)}</Alert>}
        {checkout.isError && (
          <Alert>{getApiErrorMessage(checkout.error, "We could not start checkout.")}</Alert>
        )}

        {isLoading && (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }).map((_, i) => (
              <Skeleton key={i} className="h-64 rounded-[18px]" />
            ))}
          </div>
        )}

        {isError && (
          <div className="flex min-h-[40vh] flex-col items-center justify-center text-center">
            <TriangleAlert className="h-10 w-10 text-destructive" />
            <p className="mt-3 text-sm font-medium">Couldn&apos;t load the catalog</p>
            <Button className="mt-3" variant="outline" onClick={() => refetch()}>
              Retry
            </Button>
          </div>
        )}

        {!isError && isEmpty && (
          <div className="flex min-h-[40vh] flex-col items-center justify-center rounded-[18px] border border-dashed border-border text-center">
            <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10">
              <Compass className="h-7 w-7 text-primary" />
            </div>
            <p className="mt-4 text-base font-semibold">No courses match your filters</p>
            <p className="mt-1 max-w-sm text-sm text-muted-foreground">
              Try a different search term, or clear the filters to see everything on offer.
            </p>
            <Button className="mt-4" variant="outline" onClick={resetFilters}>
              Clear filters
            </Button>
          </div>
        )}

        {courses.length > 0 && (
          <motion.div
            className={`grid gap-4 transition-opacity sm:grid-cols-2 lg:grid-cols-3 ${
              isFetching ? "opacity-60" : "opacity-100"
            }`}
            initial="hidden"
            animate="visible"
            variants={staggerContainer}
          >
            <AnimatePresence mode="popLayout">
              {courses.map((course) => (
                <CatalogCard
                  key={course.id}
                  course={course}
                  isEnrolling={
                    (enroll.isPending && enroll.variables === course.id)
                    || (checkout.isPending && checkout.variables === course.id)
                  }
                  onEnroll={onEnroll}
                />
              ))}
            </AnimatePresence>
          </motion.div>
        )}

        {data && (
          <PaginationControls
            page={data.page}
            totalPages={data.totalPages}
            totalCount={data.totalCount}
            onPageChange={setPage}
            noun="course"
          />
        )}
      </main>
    </div>
  );
}
