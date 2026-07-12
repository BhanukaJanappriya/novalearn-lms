import { useState } from "react";
import { AnimatePresence } from "framer-motion";
import { BookOpen, Plus, TriangleAlert } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Modal } from "@/components/ui/modal";
import { Alert } from "@/components/ui/alert";
import { useAuth } from "@/context/AuthContext";
import { isAdmin, canManageCourses } from "@/lib/roles";
import { getApiErrorMessage } from "@/lib/apiError";
import { useCourses, useDeleteCourse } from "../api/queries";
import type { Course } from "../api/types";
import { CourseCard } from "../components/CourseCard";
import { CreateCourseDialog } from "../components/CreateCourseDialog";

export function CoursesPage() {
  const { user } = useAuth();
  const { data: courses, isLoading, isError, refetch } = useCourses();
  const deleteCourse = useDeleteCourse();
  const [createOpen, setCreateOpen] = useState(false);
  const [pendingDelete, setPendingDelete] = useState<Course | null>(null);

  const manager = canManageCourses(user);
  const admin = isAdmin(user);
  const canDelete = (course: Course) => admin || course.lecturerId === user?.id;

  const confirmDelete = () => {
    if (!pendingDelete) return;
    deleteCourse.mutate(pendingDelete.id, { onSuccess: () => setPendingDelete(null) });
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Courses</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {courses ? `${courses.length} course${courses.length === 1 ? "" : "s"} in the catalogue` : "Manage the course catalogue"}
          </p>
        </div>
        {manager && (
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" />
            New course
          </Button>
        )}
      </div>

      {isLoading && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-64 rounded-[18px]" />
          ))}
        </div>
      )}

      {isError && (
        <div className="flex min-h-[40vh] flex-col items-center justify-center text-center">
          <TriangleAlert className="h-10 w-10 text-destructive" />
          <p className="mt-3 text-sm font-medium">Couldn’t load courses</p>
          <Button className="mt-3" variant="outline" onClick={() => refetch()}>
            Retry
          </Button>
        </div>
      )}

      {courses && courses.length === 0 && (
        <div className="flex min-h-[40vh] flex-col items-center justify-center rounded-[18px] border border-dashed border-border text-center">
          <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10">
            <BookOpen className="h-7 w-7 text-primary" />
          </div>
          <p className="mt-4 text-base font-semibold">No courses yet</p>
          <p className="mt-1 max-w-sm text-sm text-muted-foreground">
            {manager ? "Create your first course to get started." : "Courses will appear here once they’re added."}
          </p>
          {manager && (
            <Button className="mt-4" onClick={() => setCreateOpen(true)}>
              <Plus className="h-4 w-4" />
              New course
            </Button>
          )}
        </div>
      )}

      {courses && courses.length > 0 && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          <AnimatePresence mode="popLayout">
            {courses.map((course) => (
              <CourseCard
                key={course.id}
                course={course}
                canDelete={canDelete(course)}
                onDelete={setPendingDelete}
              />
            ))}
          </AnimatePresence>
        </div>
      )}

      <CreateCourseDialog open={createOpen} onClose={() => setCreateOpen(false)} />

      <Modal
        open={pendingDelete !== null}
        onClose={() => setPendingDelete(null)}
        title="Delete course"
        description={pendingDelete ? `“${pendingDelete.title}” (${pendingDelete.code}) will be removed.` : undefined}
      >
        {deleteCourse.isError && <Alert className="mb-4">{getApiErrorMessage(deleteCourse.error)}</Alert>}
        <p className="text-sm text-muted-foreground">
          This action removes the course from the catalogue. You can only delete courses you own unless you are an administrator.
        </p>
        <div className="mt-5 flex justify-end gap-2">
          <Button variant="outline" onClick={() => setPendingDelete(null)}>
            Cancel
          </Button>
          <Button variant="destructive" isLoading={deleteCourse.isPending} onClick={confirmDelete}>
            Delete course
          </Button>
        </div>
      </Modal>
    </div>
  );
}
