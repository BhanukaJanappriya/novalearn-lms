import { motion } from "framer-motion";
import { BookOpen, CircleCheckBig, LogOut, PlayCircle } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { LinkButton } from "@/components/ui/link-button";
import { Progress } from "@/components/ui/progress";
import { useSpotlight } from "@/hooks/useSpotlight";
import { coverGradient, levelVariant, statusVariant } from "../lib/courseVisuals";
import type { Enrollment } from "../api/types";

interface EnrolledCourseCardProps {
  enrollment: Enrollment;
  onUnenroll: (enrollment: Enrollment) => void;
}

/** One enrolled course with its progress, status and the continue / leave actions. */
export function EnrolledCourseCard({ enrollment, onUnenroll }: EnrolledCourseCardProps) {
  const { ref, onMouseMove } = useSpotlight<HTMLDivElement>();
  const isComplete = enrollment.status === "Completed";

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
      <div className="relative h-24" style={{ backgroundImage: coverGradient(enrollment.courseCode) }}>
        {enrollment.courseCoverImageUrl && (
          <img
            src={enrollment.courseCoverImageUrl}
            alt=""
            className="absolute inset-0 h-full w-full object-cover"
          />
        )}
        <div className="absolute inset-0 bg-gradient-to-t from-black/25 to-transparent" />
        <span className="absolute left-3 top-3 rounded-md bg-black/30 px-2 py-0.5 text-xs font-semibold text-white backdrop-blur">
          {enrollment.courseCode}
        </span>
        <span className="absolute right-3 top-3">
          <Badge variant={statusVariant[enrollment.status]}>
            {isComplete && <CircleCheckBig className="h-3 w-3" />}
            {enrollment.status}
          </Badge>
        </span>
        <BookOpen className="absolute bottom-3 right-3 h-5 w-5 text-white/80" />
      </div>

      <div className="relative z-[2] flex flex-1 flex-col p-4">
        <div className="mb-2 flex flex-wrap items-center gap-2">
          {enrollment.courseCategory && <Badge variant="neutral">{enrollment.courseCategory}</Badge>}
          {enrollment.courseLevel && (
            <Badge variant={levelVariant[enrollment.courseLevel] ?? "neutral"}>{enrollment.courseLevel}</Badge>
          )}
        </div>
        <h3 className="text-base font-semibold leading-snug">{enrollment.courseTitle}</h3>

        <div className="mt-4 space-y-1.5">
          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <span>Progress</span>
            <span className="font-semibold text-foreground">{enrollment.progressPercent}%</span>
          </div>
          <Progress value={enrollment.progressPercent} label={enrollment.courseTitle} />
        </div>

        <div className="mt-auto flex items-center justify-between gap-2 pt-4">
          <LinkButton
            to={`/catalog?search=${encodeURIComponent(enrollment.courseCode)}`}
            size="sm"
            variant={isComplete ? "outline" : "default"}
          >
            <PlayCircle className="h-4 w-4" />
            {isComplete ? "Review" : "Continue"}
          </LinkButton>
          <button
            type="button"
            onClick={() => onUnenroll(enrollment)}
            className="inline-flex items-center gap-1.5 rounded-lg px-2 py-1 text-xs font-medium text-destructive transition-colors hover:bg-destructive/10"
          >
            <LogOut className="h-3.5 w-3.5" />
            Unenroll
          </button>
        </div>
      </div>
    </motion.div>
  );
}
