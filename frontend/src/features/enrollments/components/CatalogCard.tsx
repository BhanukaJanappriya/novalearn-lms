import { motion } from "framer-motion";
import { BookOpen, Check, Plus, User, Users } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useSpotlight } from "@/hooks/useSpotlight";
import { formatNumber } from "@/lib/format";
import { coverGradient, levelVariant } from "../lib/courseVisuals";
import type { CatalogCourse } from "../api/types";

interface CatalogCardProps {
  course: CatalogCourse;
  isEnrolling: boolean;
  onEnroll: (course: CatalogCourse) => void;
}

/** A single catalogue tile with the join action and the caller's enrolment state. */
export function CatalogCard({ course, isEnrolling, onEnroll }: CatalogCardProps) {
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
        {course.isEnrolled && (
          <span className="absolute right-3 top-3">
            <Badge variant="success">
              <Check className="h-3 w-3" />
              Enrolled
            </Badge>
          </span>
        )}
        <BookOpen className="absolute bottom-3 right-3 h-5 w-5 text-white/80" />
      </div>

      <div className="relative z-[2] flex flex-1 flex-col p-4">
        <div className="mb-2 flex flex-wrap items-center gap-2">
          <Badge variant="neutral">{course.category}</Badge>
          <Badge variant={levelVariant[course.level] ?? "neutral"}>{course.level}</Badge>
        </div>
        <h3 className="text-base font-semibold leading-snug">{course.title}</h3>
        {course.description && (
          <p className="mt-1 line-clamp-2 text-sm text-muted-foreground">{course.description}</p>
        )}

        <div className="mt-3 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-muted-foreground">
          <span className="inline-flex items-center gap-1.5">
            <User className="h-3.5 w-3.5" />
            {course.lecturerName}
          </span>
          <span className="inline-flex items-center gap-1.5">
            <Users className="h-3.5 w-3.5" />
            {formatNumber(course.enrolledCount)} enrolled
          </span>
        </div>

        <div className="mt-4 flex items-center justify-between gap-3 border-t border-border pt-3">
          <span className="text-sm font-semibold">
            {course.price === 0 ? "Free" : `$${course.price.toFixed(2)}`}
          </span>
          {course.isEnrolled ? (
            <span className="inline-flex items-center gap-1.5 text-sm font-medium text-success">
              <Check className="h-4 w-4" />
              Enrolled
            </span>
          ) : (
            <Button size="sm" isLoading={isEnrolling} onClick={() => onEnroll(course)}>
              {!isEnrolling && <Plus className="h-4 w-4" />}
              {isEnrolling ? "Enrolling" : "Enroll"}
            </Button>
          )}
        </div>
      </div>
    </motion.div>
  );
}
