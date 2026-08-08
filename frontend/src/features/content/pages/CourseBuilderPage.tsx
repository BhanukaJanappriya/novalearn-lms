import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import { AnimatePresence } from "framer-motion";
import { ArrowLeft, Layers, Plus, TriangleAlert } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Modal } from "@/components/ui/modal";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { useAuth } from "@/context/AuthContext";
import { isAdmin } from "@/lib/roles";
import { getApiErrorMessage } from "@/lib/apiError";
import {
  useCourseContent,
  useDeleteLesson,
  useDeleteModule,
  useReorderLessons,
  useReorderModules,
} from "../api/queries";
import type { CourseModule, Lesson } from "../api/types";
import { ModuleRow } from "../components/ModuleRow";
import { ModuleFormDialog } from "../components/ModuleFormDialog";
import { LessonFormDialog } from "../components/LessonFormDialog";
import { countLessons, formatDuration } from "../components/lessonMeta";

/** Lecturer/admin course builder: the modules of a course and the lessons inside them. */
export function CourseBuilderPage() {
  const { courseId = "" } = useParams<{ courseId: string }>();
  const { user } = useAuth();
  const { data: content, isLoading, isError, refetch } = useCourseContent(courseId);

  const reorderModules = useReorderModules(courseId);
  const reorderLessons = useReorderLessons(courseId);
  const deleteModule = useDeleteModule(courseId);
  const deleteLesson = useDeleteLesson(courseId);

  // Modules start expanded; this tracks the ones the user has folded away.
  const [collapsed, setCollapsed] = useState<string[]>([]);
  const [moduleFormOpen, setModuleFormOpen] = useState(false);
  const [editingModule, setEditingModule] = useState<CourseModule | null>(null);
  const [lessonForm, setLessonForm] = useState<{ moduleId: string; lesson: Lesson | null } | null>(
    null,
  );
  const [pendingModule, setPendingModule] = useState<CourseModule | null>(null);
  const [pendingLesson, setPendingLesson] = useState<Lesson | null>(null);

  const canManage = Boolean(content) && (isAdmin(user) || content?.lecturerId === user?.id);
  const modules = content?.modules ?? [];

  const toggle = (moduleId: string) =>
    setCollapsed((current) =>
      current.includes(moduleId)
        ? current.filter((id) => id !== moduleId)
        : [...current, moduleId],
    );

  /** Swaps a module with its neighbour and sends the whole new order to the server. */
  const moveModule = (module: CourseModule, direction: -1 | 1) => {
    const ids = modules.map((m) => m.id);
    const index = ids.indexOf(module.id);
    const target = index + direction;
    if (index < 0 || target < 0 || target >= ids.length) return;

    [ids[index], ids[target]] = [ids[target], ids[index]];
    reorderModules.mutate(ids);
  };

  const moveLesson = (module: CourseModule, lesson: Lesson, direction: -1 | 1) => {
    const ids = module.lessons.map((l) => l.id);
    const index = ids.indexOf(lesson.id);
    const target = index + direction;
    if (index < 0 || target < 0 || target >= ids.length) return;

    [ids[index], ids[target]] = [ids[target], ids[index]];
    reorderLessons.mutate({ moduleId: module.id, ids });
  };

  const openCreateModule = () => {
    setEditingModule(null);
    setModuleFormOpen(true);
  };

  const openEditModule = (module: CourseModule) => {
    setEditingModule(module);
    setModuleFormOpen(true);
  };

  const reorderError = reorderModules.error ?? reorderLessons.error;
  const totalLessons = countLessons(modules);
  const totalDuration = formatDuration(
    modules.reduce(
      (sum, module) =>
        sum + module.lessons.reduce((inner, lesson) => inner + (lesson.durationMinutes ?? 0), 0),
      0,
    ),
  );

  return (
    <PageTransition>
      <div className="space-y-6">
      <div>
        <Link
          to="/admin/courses"
          className="inline-flex items-center gap-1.5 text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to courses
        </Link>

        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <div className="min-w-0">
            <div className="flex flex-wrap items-center gap-2">
              <h1 className="text-2xl font-semibold tracking-tight">
                {content?.courseTitle ?? "Course content"}
              </h1>
              {content && <Badge variant="neutral">{content.courseCode}</Badge>}
              {content && (
                <Badge variant={content.courseStatus === "Published" ? "success" : "neutral"}>
                  {content.courseStatus}
                </Badge>
              )}
            </div>
            <p className="mt-1 text-sm text-muted-foreground">
              {content
                ? `${modules.length} module${modules.length === 1 ? "" : "s"} · ${totalLessons} lesson${
                    totalLessons === 1 ? "" : "s"
                  }${totalDuration ? ` · ${totalDuration}` : ""}`
                : "Build the outline of this course"}
            </p>
          </div>

          {canManage && (
            <Button onClick={openCreateModule}>
              <Plus className="h-4 w-4" />
              Add module
            </Button>
          )}
        </div>
      </div>

      {(reorderError || deleteModule.isError || deleteLesson.isError) && (
        <Alert>
          {getApiErrorMessage(reorderError ?? deleteModule.error ?? deleteLesson.error)}
        </Alert>
      )}

      {isLoading && (
        <div className="space-y-3">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-20 rounded-[18px]" />
          ))}
        </div>
      )}

      {isError && (
        <div className="flex min-h-[40vh] flex-col items-center justify-center text-center">
          <TriangleAlert className="h-10 w-10 text-destructive" />
          <p className="mt-3 text-sm font-medium">Couldn’t load this course’s content</p>
          <Button className="mt-3" variant="outline" onClick={() => refetch()}>
            Retry
          </Button>
        </div>
      )}

      {content && modules.length === 0 && (
        <div className="flex min-h-[40vh] flex-col items-center justify-center rounded-[18px] border border-dashed border-border text-center">
          <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10">
            <Layers className="h-7 w-7 text-primary" />
          </div>
          <p className="mt-4 text-base font-semibold">No modules yet, add your first one</p>
          <p className="mt-1 max-w-sm text-sm text-muted-foreground">
            {canManage
              ? "Modules group lessons into chapters your students work through in order."
              : "The lecturer hasn’t published an outline for this course yet."}
          </p>
          {canManage && (
            <Button className="mt-4" onClick={openCreateModule}>
              <Plus className="h-4 w-4" />
              Add module
            </Button>
          )}
        </div>
      )}

      {modules.length > 0 && (
        <ul className="space-y-3">
          <AnimatePresence mode="popLayout">
            {modules.map((module, index) => (
              <ModuleRow
                key={module.id}
                module={module}
                position={index + 1}
                canManage={canManage}
                expanded={!collapsed.includes(module.id)}
                isFirst={index === 0}
                isLast={index === modules.length - 1}
                onToggle={() => toggle(module.id)}
                onEdit={openEditModule}
                onDelete={setPendingModule}
                onMove={moveModule}
                onAddLesson={(target) => setLessonForm({ moduleId: target.id, lesson: null })}
                onEditLesson={(target, lesson) =>
                  setLessonForm({ moduleId: target.id, lesson })
                }
                onDeleteLesson={setPendingLesson}
                onMoveLesson={moveLesson}
              />
            ))}
          </AnimatePresence>
        </ul>
      )}

      <ModuleFormDialog
        open={moduleFormOpen}
        courseId={courseId}
        module={editingModule}
        onClose={() => setModuleFormOpen(false)}
      />

      {lessonForm && (
        <LessonFormDialog
          open
          courseId={courseId}
          moduleId={lessonForm.moduleId}
          lesson={lessonForm.lesson}
          onClose={() => setLessonForm(null)}
        />
      )}

      <Modal
        open={pendingModule !== null}
        onClose={() => setPendingModule(null)}
        title="Delete module"
        description={pendingModule ? `“${pendingModule.title}” will be removed.` : undefined}
      >
        {deleteModule.isError && (
          <Alert className="mb-4">{getApiErrorMessage(deleteModule.error)}</Alert>
        )}
        <p className="text-sm text-muted-foreground">
          Deleting a module also removes the{" "}
          {pendingModule?.lessons.length ?? 0}{" "}
          {pendingModule?.lessons.length === 1 ? "lesson" : "lessons"} inside it.
        </p>
        <div className="mt-5 flex justify-end gap-2">
          <Button variant="outline" onClick={() => setPendingModule(null)}>
            Cancel
          </Button>
          <Button
            variant="destructive"
            isLoading={deleteModule.isPending}
            onClick={() =>
              pendingModule &&
              deleteModule.mutate(pendingModule.id, { onSuccess: () => setPendingModule(null) })
            }
          >
            Delete module
          </Button>
        </div>
      </Modal>

      <Modal
        open={pendingLesson !== null}
        onClose={() => setPendingLesson(null)}
        title="Delete lesson"
        description={pendingLesson ? `“${pendingLesson.title}” will be removed.` : undefined}
      >
        {deleteLesson.isError && (
          <Alert className="mb-4">{getApiErrorMessage(deleteLesson.error)}</Alert>
        )}
        <p className="text-sm text-muted-foreground">
          This removes the lesson from the module. You can add it again later.
        </p>
        <div className="mt-5 flex justify-end gap-2">
          <Button variant="outline" onClick={() => setPendingLesson(null)}>
            Cancel
          </Button>
          <Button
            variant="destructive"
            isLoading={deleteLesson.isPending}
            onClick={() =>
              pendingLesson &&
              deleteLesson.mutate(pendingLesson.id, { onSuccess: () => setPendingLesson(null) })
            }
          >
            Delete lesson
          </Button>
        </div>
      </Modal>
    </div>
    </PageTransition>
  );
}
