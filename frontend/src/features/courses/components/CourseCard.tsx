import { motion } from "framer-motion";
import { BookOpen, Pencil, Trash2, User } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { useSpotlight } from "@/hooks/useSpotlight";
import type { Course, CourseLevel, CourseStatus } from "../api/types";

const levelVariant: Record<CourseLevel, "success" | "warning" | "destructive"> = {
  Beginner: "success",
  Intermediate: "warning",
  Advanced: "destructive",
};

const statusVariant: Record<CourseStatus, "success" | "neutral"> = {
  Published: "success",
  Draft: "neutral",
};

/** Deterministic brand-ish gradient from the course code, for the cover strip. */
function coverGradient(seed: string): string {
  const palettes = [
    ["#8B5CF6", "#A78BFA"],
    ["#2a78d6", "#5598e7"],
    ["#1baf7a", "#3fd39c"],
    ["#eda100", "#f5c451"],
    ["#4a3aa7", "#7a6ad4"],
    ["#e34948", "#f07a79"],
  ];
  const hash = [...seed].reduce((acc, ch) => acc + ch.charCodeAt(0), 0);
  const [from, to] = palettes[hash % palettes.length];
  return `linear-gradient(135deg, ${from}, ${to})`;
}

interface CourseCardProps {
  course: Course;
  /** Whether the current user may edit/delete this course. */
  canManage: boolean;
  onEdit: (course: Course) => void;
  onDelete: (course: Course) => void;
}

export function CourseCard({ course, canManage, onEdit, onDelete }: CourseCardProps) {
  const { ref, onMouseMove } = useSpotlight<HTMLDivElement>();

  return (
    <motion.div
      ref={ref}
      onMouseMove={onMouseMove}
      layout
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, scale: 0.96 }}
      whileHover={{ y: -4 }}
      transition={{ type: "spring", stiffness: 320, damping: 24 }}
      className="spotlight flex flex-col overflow-hidden rounded-[18px] border border-border bg-card shadow-soft"
    >
      <div className="relative h-24" style={{ backgroundImage: coverGradient(course.code) }}>
        {course.coverImageUrl && (
          <img src={course.coverImageUrl} alt="" className="absolute inset-0 h-full w-full object-cover" />
        )}
        <div className="absolute inset-0 bg-gradient-to-t from-black/25 to-transparent" />
        <span className="absolute left-3 top-3 rounded-md bg-black/30 px-2 py-0.5 text-xs font-semibold text-white backdrop-blur">
          {course.code}
        </span>
        <span className="absolute right-3 top-3">
          <Badge variant={statusVariant[course.status]}>{course.status}</Badge>
        </span>
        <BookOpen className="absolute bottom-3 right-3 h-5 w-5 text-white/80" />
      </div>

      <div className="relative z-[2] flex flex-1 flex-col p-4">
        <div className="mb-2 flex items-center gap-2">
          <Badge variant="neutral">{course.category}</Badge>
          <Badge variant={levelVariant[course.level]}>{course.level}</Badge>
        </div>
        <h3 className="text-base font-semibold leading-snug">{course.title}</h3>
        {course.description && (
          <p className="mt-1 line-clamp-2 text-sm text-muted-foreground">{course.description}</p>
        )}

        <div className="mt-3 flex items-center gap-1.5 text-xs text-muted-foreground">
          <User className="h-3.5 w-3.5" />
          {course.lecturerName}
        </div>

        <div className="mt-4 flex items-center justify-between border-t border-border pt-3">
          <span className="text-sm font-semibold">
            {course.price === 0 ? "Free" : `$${course.price.toFixed(2)}`}
          </span>
          {canManage && (
            <div className="flex items-center gap-1">
              <button
                type="button"
                onClick={() => onEdit(course)}
                className="inline-flex items-center gap-1.5 rounded-lg px-2 py-1 text-xs font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
              >
                <Pencil className="h-3.5 w-3.5" />
                Edit
              </button>
              <button
                type="button"
                onClick={() => onDelete(course)}
                className="inline-flex items-center gap-1.5 rounded-lg px-2 py-1 text-xs font-medium text-destructive transition-colors hover:bg-destructive/10"
              >
                <Trash2 className="h-3.5 w-3.5" />
                Delete
              </button>
            </div>
          )}
        </div>
      </div>
    </motion.div>
  );
}
