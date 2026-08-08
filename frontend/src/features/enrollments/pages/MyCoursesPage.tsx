import { useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { Compass, GraduationCap, TriangleAlert } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { LinkButton } from "@/components/ui/link-button";
import { Modal } from "@/components/ui/modal";
import { Skeleton } from "@/components/ui/skeleton";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { getApiErrorMessage } from "@/lib/apiError";
import { staggerContainer } from "@/lib/motion";
import { useMyEnrollments, useUnenroll } from "../api/queries";
import type { Enrollment } from "../api/types";
import { EnrolledCourseCard } from "../components/EnrolledCourseCard";

export function MyCoursesPage() {
  const { data: enrollments, isLoading, isError, refetch } = useMyEnrollments();
  const unenroll = useUnenroll();
  const [pendingUnenroll, setPendingUnenroll] = useState<Enrollment | null>(null);

  const active = (enrollments ?? []).filter((e) => e.status !== "Dropped");
  const averageProgress = active.length
    ? Math.round(active.reduce((sum, e) => sum + e.progressPercent, 0) / active.length)
    : 0;

  const confirmUnenroll = () => {
    if (!pendingUnenroll) return;
    unenroll.mutate(pendingUnenroll.id, { onSuccess: () => setPendingUnenroll(null) });
  };

  return (
    <div className="min-h-screen">
      <LearnerHeader />

      <main className="mx-auto max-w-6xl space-y-6 px-6 py-10">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">My courses</h1>
            <p className="mt-1 text-sm text-muted-foreground">
              {enrollments
                ? `${active.length} enrolled · ${averageProgress}% average progress`
                : "Everything you are currently learning."}
            </p>
          </div>
          <LinkButton to="/catalog" variant="outline">
            <Compass className="h-4 w-4" />
            Browse catalog
          </LinkButton>
        </div>

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
            <p className="mt-3 text-sm font-medium">Couldn&apos;t load your courses</p>
            <Button className="mt-3" variant="outline" onClick={() => refetch()}>
              Retry
            </Button>
          </div>
        )}

        {enrollments && active.length === 0 && (
          <div className="flex min-h-[40vh] flex-col items-center justify-center rounded-[18px] border border-dashed border-border text-center">
            <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10">
              <GraduationCap className="h-7 w-7 text-primary" />
            </div>
            <p className="mt-4 text-base font-semibold">You haven&apos;t enrolled yet</p>
            <p className="mt-1 max-w-sm text-sm text-muted-foreground">
              Find something that interests you in the catalog and join in one click.
            </p>
            <LinkButton className="mt-4" to="/catalog">
              <Compass className="h-4 w-4" />
              Browse catalog
            </LinkButton>
          </div>
        )}

        {active.length > 0 && (
          <motion.div
            className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
            initial="hidden"
            animate="visible"
            variants={staggerContainer}
          >
            <AnimatePresence mode="popLayout">
              {active.map((enrollment) => (
                <EnrolledCourseCard
                  key={enrollment.id}
                  enrollment={enrollment}
                  onUnenroll={setPendingUnenroll}
                />
              ))}
            </AnimatePresence>
          </motion.div>
        )}
      </main>

      <Modal
        open={pendingUnenroll !== null}
        onClose={() => setPendingUnenroll(null)}
        title="Leave this course"
        description={
          pendingUnenroll
            ? `You will be unenrolled from “${pendingUnenroll.courseTitle}” (${pendingUnenroll.courseCode}).`
            : undefined
        }
      >
        {unenroll.isError && <Alert className="mb-4">{getApiErrorMessage(unenroll.error)}</Alert>}
        <p className="text-sm text-muted-foreground">
          Your progress will stop counting towards this course. You can enroll again later from the catalog.
        </p>
        <div className="mt-5 flex justify-end gap-2">
          <Button variant="outline" onClick={() => setPendingUnenroll(null)}>
            Cancel
          </Button>
          <Button variant="destructive" isLoading={unenroll.isPending} onClick={confirmUnenroll}>
            Unenroll
          </Button>
        </div>
      </Modal>
    </div>
  );
}
