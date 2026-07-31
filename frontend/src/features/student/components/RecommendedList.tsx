import { Link } from "react-router-dom";
import { ArrowRight, Users } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { coverGradient, levelVariant } from "@/features/enrollments/lib/courseVisuals";
import type { RecommendedCourse } from "../api/types";

interface RecommendedListProps {
  courses: RecommendedCourse[];
}

/** Published courses the learner has not joined, ranked by how many peers have. */
export function RecommendedList({ courses }: RecommendedListProps) {
  if (courses.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        You have joined everything on offer. More courses are on the way.
      </p>
    );
  }

  return (
    <ul className="space-y-2">
      {courses.map((course) => (
        <li key={course.courseId}>
          <Link
            to={`/catalog?search=${encodeURIComponent(course.code)}`}
            className="group flex items-center gap-3 rounded-xl border border-transparent p-2 transition-colors hover:border-border hover:bg-muted/50"
          >
            <span
              className="h-10 w-10 shrink-0 rounded-lg"
              style={{ backgroundImage: coverGradient(course.code) }}
              aria-hidden
            />
            <span className="min-w-0 flex-1">
              <span className="flex items-center gap-2">
                <span className="truncate text-sm font-medium">{course.title}</span>
                <Badge variant={levelVariant[course.level]}>{course.level}</Badge>
              </span>
              <span className="mt-0.5 flex items-center gap-3 text-xs text-muted-foreground">
                <span className="inline-flex items-center gap-1">
                  <Users className="h-3 w-3" aria-hidden />
                  {course.enrolledCount}
                </span>
                <span>{course.lessonCount} lessons</span>
                <span>{course.price === 0 ? "Free" : `$${course.price.toFixed(2)}`}</span>
              </span>
            </span>
            <ArrowRight
              className="h-4 w-4 shrink-0 text-muted-foreground transition-transform group-hover:translate-x-0.5"
              aria-hidden
            />
          </Link>
        </li>
      ))}
    </ul>
  );
}
