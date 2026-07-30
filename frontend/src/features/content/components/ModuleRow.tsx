import { AnimatePresence, motion } from "framer-motion";
import { ArrowDown, ArrowUp, ChevronRight, Pencil, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { IconAction } from "./IconAction";
import { LessonRow } from "./LessonRow";
import { formatDuration, moduleDuration } from "./lessonMeta";
import type { CourseModule, Lesson } from "../api/types";

interface ModuleRowProps {
  module: CourseModule;
  position: number;
  canManage: boolean;
  expanded: boolean;
  isFirst: boolean;
  isLast: boolean;
  onToggle: () => void;
  onEdit: (module: CourseModule) => void;
  onDelete: (module: CourseModule) => void;
  onMove: (module: CourseModule, direction: -1 | 1) => void;
  onAddLesson: (module: CourseModule) => void;
  onEditLesson: (module: CourseModule, lesson: Lesson) => void;
  onDeleteLesson: (lesson: Lesson) => void;
  onMoveLesson: (module: CourseModule, lesson: Lesson, direction: -1 | 1) => void;
}

export function ModuleRow({
  module,
  position,
  canManage,
  expanded,
  isFirst,
  isLast,
  onToggle,
  onEdit,
  onDelete,
  onMove,
  onAddLesson,
  onEditLesson,
  onDeleteLesson,
  onMoveLesson,
}: ModuleRowProps) {
  const lessonCount = module.lessons.length;
  const duration = formatDuration(moduleDuration(module));

  return (
    <motion.li
      layout
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, scale: 0.98 }}
      transition={{ type: "spring", stiffness: 320, damping: 26 }}
      className="overflow-hidden rounded-[18px] border border-border bg-card shadow-soft"
    >
      <div className="flex items-center gap-3 p-4">
        <button
          type="button"
          onClick={onToggle}
          aria-expanded={expanded}
          aria-label={expanded ? `Collapse ${module.title}` : `Expand ${module.title}`}
          className="flex min-w-0 flex-1 items-center gap-3 text-left"
        >
          <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-muted text-sm font-semibold text-muted-foreground">
            {position}
          </span>
          <ChevronRight
            className={cn(
              "h-4 w-4 shrink-0 text-muted-foreground transition-transform",
              expanded && "rotate-90",
            )}
            aria-hidden
          />
          <span className="min-w-0 flex-1">
            <span className="block truncate text-sm font-semibold">{module.title}</span>
            <span className="mt-0.5 flex items-center gap-2 text-xs text-muted-foreground">
              <span>
                {lessonCount} {lessonCount === 1 ? "lesson" : "lessons"}
              </span>
              {duration && (
                <>
                  <span aria-hidden>·</span>
                  <span>{duration}</span>
                </>
              )}
            </span>
          </span>
        </button>

        {canManage && (
          <div className="flex shrink-0 items-center gap-0.5">
            <IconAction
              icon={ArrowUp}
              label="Move module up"
              disabled={isFirst}
              onClick={() => onMove(module, -1)}
            />
            <IconAction
              icon={ArrowDown}
              label="Move module down"
              disabled={isLast}
              onClick={() => onMove(module, 1)}
            />
            <IconAction icon={Pencil} label="Edit module" onClick={() => onEdit(module)} />
            <IconAction
              icon={Trash2}
              label="Delete module"
              tone="destructive"
              onClick={() => onDelete(module)}
            />
          </div>
        )}
      </div>

      <AnimatePresence initial={false}>
        {expanded && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: "auto", opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            transition={{ duration: 0.2 }}
            className="overflow-hidden"
          >
            <div className="border-t border-border px-4 pb-4 pt-3">
              {module.description && (
                <p className="mb-3 text-sm text-muted-foreground">{module.description}</p>
              )}

              {lessonCount === 0 ? (
                <p className="rounded-xl border border-dashed border-border px-3 py-6 text-center text-sm text-muted-foreground">
                  No lessons in this module yet.
                </p>
              ) : (
                <ul className="space-y-0.5">
                  {module.lessons.map((lesson, index) => (
                    <LessonRow
                      key={lesson.id}
                      lesson={lesson}
                      canManage={canManage}
                      isFirst={index === 0}
                      isLast={index === lessonCount - 1}
                      onEdit={(target) => onEditLesson(module, target)}
                      onDelete={onDeleteLesson}
                      onMove={(target, direction) => onMoveLesson(module, target, direction)}
                    />
                  ))}
                </ul>
              )}

              {canManage && (
                <Button
                  variant="outline"
                  size="sm"
                  className="mt-3"
                  onClick={() => onAddLesson(module)}
                >
                  <Plus className="h-4 w-4" />
                  Add lesson
                </Button>
              )}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </motion.li>
  );
}
