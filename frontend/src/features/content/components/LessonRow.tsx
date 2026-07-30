import { ArrowDown, ArrowUp, Pencil, Trash2 } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { IconAction } from "./IconAction";
import { formatDuration, lessonTypeIcon, lessonTypeLabel } from "./lessonMeta";
import type { Lesson } from "../api/types";

interface LessonRowProps {
  lesson: Lesson;
  canManage: boolean;
  isFirst: boolean;
  isLast: boolean;
  onEdit: (lesson: Lesson) => void;
  onDelete: (lesson: Lesson) => void;
  onMove: (lesson: Lesson, direction: -1 | 1) => void;
}

export function LessonRow({
  lesson,
  canManage,
  isFirst,
  isLast,
  onEdit,
  onDelete,
  onMove,
}: LessonRowProps) {
  const Icon = lessonTypeIcon[lesson.type];
  const duration = formatDuration(lesson.durationMinutes);

  return (
    <li className="flex items-center gap-3 rounded-xl px-3 py-2.5 transition-colors hover:bg-muted/60">
      <span
        className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary"
        title={lessonTypeLabel[lesson.type]}
      >
        <Icon className="h-4 w-4" aria-hidden />
      </span>

      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium">{lesson.title}</p>
        <p className="mt-0.5 flex items-center gap-2 text-xs text-muted-foreground">
          <span>{lessonTypeLabel[lesson.type]}</span>
          {duration && (
            <>
              <span aria-hidden>·</span>
              <span>{duration}</span>
            </>
          )}
        </p>
      </div>

      {lesson.isPreview && <Badge variant="success">Preview</Badge>}

      {canManage && (
        <div className="flex shrink-0 items-center gap-0.5">
          <IconAction
            icon={ArrowUp}
            label="Move lesson up"
            disabled={isFirst}
            onClick={() => onMove(lesson, -1)}
          />
          <IconAction
            icon={ArrowDown}
            label="Move lesson down"
            disabled={isLast}
            onClick={() => onMove(lesson, 1)}
          />
          <IconAction icon={Pencil} label="Edit lesson" onClick={() => onEdit(lesson)} />
          <IconAction
            icon={Trash2}
            label="Delete lesson"
            tone="destructive"
            onClick={() => onDelete(lesson)}
          />
        </div>
      )}
    </li>
  );
}
