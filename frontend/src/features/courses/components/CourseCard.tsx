import { Link } from "react-router-dom";
import { motion } from "framer-motion";
import { BookOpen, ClipboardList, Layers, ListChecks, Pencil, Trash2, User, Users } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { CourseImage } from "@/components/ui/course-image";
import { staggerItem } from "@/lib/motion";
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
  return (
    <motion.div
      layout
      variants={staggerItem}
      exit={{ opacity: 0, scale: 0.96, transition: { duration: 0.18 } }}
      whileHover={{ y: -4 }}
      transition={{ type: "spring", stiffness: 320, damping: 24 }}
      className="flex flex-col overflow-hidden rounded-[18px] border border-border bg-card shadow-soft"
    >
      <CourseImage
        code={course.code}
        category={course.category}
        coverImageUrl={course.coverImageUrl}
        gradient={coverGradient(course.code)}
        width={640}
        height={280}
        className="h-32"
      >
        <span className="absolute left-3 top-3 rounded-md bg-black/40 px-2 py-0.5 text-xs font-semibold text-white backdrop-blur">
          {course.code}
        </span>
        <span className="absolute right-3 top-3">
          <Badge variant={statusVariant[course.status]}>{course.status}</Badge>
        </span>
        <BookOpen className="absolute bottom-3 right-3 h-5 w-5 text-white/90" />
      </CourseImage>

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

        <div className="mt-4 flex flex-wrap items-center justify-between gap-2 border-t border-border pt-3">
          <span className="text-sm font-semibold">
            {course.price === 0 ? "Free" : `$${course.price.toFixed(2)}`}
          </span>
          {canManage && (
            <div className="flex items-center gap-1">
              {/* Content and roster are visible to the same people who may manage the course. */}
              <Link
                to={`/admin/courses/${course.id}/content`}
                className="inline-flex items-center gap-1.5 rounded-lg px-2 py-1 text-xs font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
              >
                <Layers className="h-3.5 w-3.5" />
                Content
              </Link>
              <Link
                to={`/admin/courses/${course.id}/assignments`}
                className="inline-flex items-center gap-1.5 rounded-lg px-2 py-1 text-xs font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
              >
                <ClipboardList className="h-3.5 w-3.5" />
                Work
              </Link>
              <Link
                to={`/admin/courses/${course.id}/quizzes`}
                className="inline-flex items-center gap-1.5 rounded-lg px-2 py-1 text-xs font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
              >
                <ListChecks className="h-3.5 w-3.5" />
                Quizzes
              </Link>
              <Link
                to={`/admin/courses/${course.id}/students`}
                className="inline-flex items-center gap-1.5 rounded-lg px-2 py-1 text-xs font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
              >
                <Users className="h-3.5 w-3.5" />
                Students
              </Link>
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
