import { Link, useParams } from "react-router-dom";
import { PageTransition } from "@/components/PageTransition";
import { ArrowLeft, TriangleAlert, Users } from "lucide-react";
import { Avatar } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { getApiErrorMessage } from "@/lib/apiError";
import { useCourseRoster } from "../api/queries";
import { avatarColor, statusVariant } from "../lib/courseVisuals";

const dateFormatter = new Intl.DateTimeFormat("en-US", {
  year: "numeric",
  month: "short",
  day: "numeric",
});

/** Students enrolled in a single course. Rendered inside the admin layout. */
export function CourseRosterPage() {
  const { courseId } = useParams<{ courseId: string }>();
  const { data: roster, isLoading, isError, error, refetch } = useCourseRoster(courseId);

  const title = roster?.[0]?.courseTitle;
  const code = roster?.[0]?.courseCode;
  const averageProgress = roster?.length
    ? Math.round(roster.reduce((sum, e) => sum + e.progressPercent, 0) / roster.length)
    : 0;

  return (
    <PageTransition>
      <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <Link
            to="/admin/courses"
            className="inline-flex items-center gap-1.5 text-sm text-muted-foreground transition-colors hover:text-foreground"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to courses
          </Link>
          <h1 className="mt-2 text-2xl font-semibold tracking-tight">
            {title ? `${title} students` : "Enrolled students"}
          </h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {roster
              ? `${roster.length} enrolled${code ? ` in ${code}` : ""} · ${averageProgress}% average progress`
              : "Students enrolled in this course."}
          </p>
        </div>
      </div>

      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-16 rounded-[18px]" />
          ))}
        </div>
      )}

      {isError && (
        <div className="flex min-h-[40vh] flex-col items-center justify-center rounded-[18px] border border-dashed border-border text-center">
          <TriangleAlert className="h-10 w-10 text-destructive" />
          <p className="mt-3 text-sm font-medium">{getApiErrorMessage(error, "Couldn’t load the roster")}</p>
          <Button className="mt-3" variant="outline" onClick={() => refetch()}>
            Retry
          </Button>
        </div>
      )}

      {roster && roster.length === 0 && (
        <div className="flex min-h-[40vh] flex-col items-center justify-center rounded-[18px] border border-dashed border-border text-center">
          <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10">
            <Users className="h-7 w-7 text-primary" />
          </div>
          <p className="mt-4 text-base font-semibold">No students yet</p>
          <p className="mt-1 max-w-sm text-sm text-muted-foreground">
            Students appear here as soon as they enroll from the catalog.
          </p>
        </div>
      )}

      {roster && roster.length > 0 && (
        <div className="overflow-hidden rounded-[18px] border border-border bg-card shadow-soft">
          <div className="overflow-x-auto">
            <table className="w-full min-w-[640px] text-sm">
              <thead>
                <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
                  <th className="px-4 py-3 font-medium">Student</th>
                  <th className="px-4 py-3 font-medium">Enrolled</th>
                  <th className="px-4 py-3 font-medium">Status</th>
                  <th className="px-4 py-3 font-medium">Progress</th>
                </tr>
              </thead>
              <tbody>
                {roster.map((enrollment) => (
                  <tr key={enrollment.id} className="border-b border-border last:border-0 hover:bg-muted/50">
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-3">
                        <Avatar
                          name={enrollment.studentName}
                          color={avatarColor(enrollment.studentName)}
                          size="sm"
                        />
                        <div className="min-w-0">
                          <p className="truncate font-medium">{enrollment.studentName}</p>
                          <p className="truncate text-xs text-muted-foreground">{enrollment.studentEmail}</p>
                        </div>
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 text-muted-foreground">
                      {dateFormatter.format(new Date(enrollment.enrolledAtUtc))}
                    </td>
                    <td className="px-4 py-3">
                      <Badge variant={statusVariant[enrollment.status]}>{enrollment.status}</Badge>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-3">
                        <Progress
                          value={enrollment.progressPercent}
                          label={enrollment.studentName}
                          size="sm"
                          className="max-w-[160px]"
                        />
                        <span className="w-10 shrink-0 text-xs font-semibold">
                          {enrollment.progressPercent}%
                        </span>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
    </PageTransition>
  );
}
