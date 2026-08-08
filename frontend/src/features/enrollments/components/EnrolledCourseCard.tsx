import { motion } from "framer-motion";
import { BookOpen, CircleCheckBig, ClipboardList, ListChecks, LogOut, PlayCircle } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { CourseImage } from "@/components/ui/course-image";
import { LinkButton } from "@/components/ui/link-button";
import { Progress } from "@/components/ui/progress";
import { useSpotlight } from "@/hooks/useSpotlight";
import { staggerItem } from "@/lib/motion";
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
      variants={staggerItem}
      exit={{ opacity: 0, scale: 0.96, transition: { duration: 0.18 } }}
      whileHover={{ y: -4 }}
      transition={{ type: "spring", stiffness: 320, damping: 24 }}
      className="spotlight flex flex-col overflow-hidden rounded-[18px] border border-border bg-card shadow-soft"
    >
      <CourseImage
        code={enrollment.courseCode}
        category={enrollment.courseCategory}
        coverImageUrl={enrollment.courseCoverImageUrl}
        gradient={coverGradient(enrollment.courseCode)}
        width={640}
        height={280}
        className="h-32"
      >
        <span className="absolute left-3 top-3 rounded-md bg-black/40 px-2 py-0.5 text-xs font-semibold text-white backdrop-blur">
          {enrollment.courseCode}
        </span>
        <span className="absolute right-3 top-3">
          <Badge variant={statusVariant[enrollment.status]}>
            {isComplete && <CircleCheckBig className="h-3 w-3" />}
            {enrollment.status}
          </Badge>
        </span>
        <BookOpen className="absolute bottom-3 right-3 h-5 w-5 text-white/90" />
      </CourseImage>

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

        <div className="mt-auto flex flex-wrap items-center justify-between gap-2 pt-4">
          <div className="flex items-center gap-2">
            <LinkButton
              to={`/catalog?search=${encodeURIComponent(enrollment.courseCode)}`}
              size="sm"
              variant={isComplete ? "outline" : "default"}
            >
              <PlayCircle className="h-4 w-4" />
              {isComplete ? "Review" : "Continue"}
            </LinkButton>
            <LinkButton
              to={`/my-courses/${enrollment.courseId}/assignments`}
              size="sm"
              variant="outline"
            >
              <ClipboardList className="h-4 w-4" />
              Assignments
            </LinkButton>
            <LinkButton
              to={`/my-courses/${enrollment.courseId}/quizzes`}
              size="sm"
              variant="outline"
            >
              <ListChecks className="h-4 w-4" />
              Quizzes
            </LinkButton>
          </div>
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
