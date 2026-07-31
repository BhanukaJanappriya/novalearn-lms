import { AnimatePresence } from "framer-motion";
import {
  Award,
  BookOpen,
  Clock,
  Compass,
  GraduationCap,
  Sparkles,
  TrendingUp,
} from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { LinkButton } from "@/components/ui/link-button";
import { useAuth } from "@/context/AuthContext";
import { useUpdateProgress } from "@/features/enrollments/api/queries";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { getApiErrorMessage } from "@/lib/apiError";
import { useStudentDashboard } from "../api/queries";
import { ActivityChart } from "../components/ActivityChart";
import { CategoryProgressPanel } from "../components/CategoryProgressPanel";
import { ContinueLearningCard } from "../components/ContinueLearningCard";
import { RecommendedList } from "../components/RecommendedList";
import { StatTile } from "../components/StatTile";
import { StudentDashboardSkeleton } from "../components/StudentDashboardSkeleton";
import { formatDuration, progressHeadline } from "../lib/learning";

export function StudentDashboardPage() {
  const { user } = useAuth();
  const { data, isLoading, isError, error } = useStudentDashboard();
  const updateProgress = useUpdateProgress();

  const handleProgressChange = (enrollmentId: string, progressPercent: number) =>
    updateProgress.mutate({ enrollmentId, progressPercent });

  return (
    <div className="min-h-screen">
      <LearnerHeader />

      <main className="mx-auto max-w-6xl px-6 py-10">
        <header className="mb-8">
          <h1 className="text-2xl font-semibold tracking-tight">
            Welcome back{user ? `, ${user.fullName}` : ""} 👋
          </h1>
          <p className="mt-1 text-muted-foreground">
            {data
              ? progressHeadline(data.summary.activeCourses, data.summary.averageProgressPercent)
              : "Here is your learning space."}
          </p>
        </header>

        {isError && (
          <Alert variant="error" className="mb-6">
            {getApiErrorMessage(error, "We could not load your dashboard.")}
          </Alert>
        )}

        {updateProgress.isError && (
          <Alert variant="error" className="mb-6">
            {getApiErrorMessage(updateProgress.error, "We could not save your progress.")}
          </Alert>
        )}

        {isLoading && <StudentDashboardSkeleton />}

        {data && (
          <div className="space-y-8">
            <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <StatTile
                label="Courses in progress"
                value={String(data.summary.activeCourses)}
                hint={
                  data.summary.coursesNearlyDone > 0
                    ? `${data.summary.coursesNearlyDone} nearly finished`
                    : undefined
                }
                icon={GraduationCap}
              />
              <StatTile
                label="Average progress"
                value={`${data.summary.averageProgressPercent}%`}
                hint="Across every course you have joined"
                icon={TrendingUp}
                accent="text-success"
              />
              <StatTile
                label="Completed"
                value={String(data.summary.completedCourses)}
                hint={data.summary.completedCourses > 0 ? "Well done" : "Nothing finished yet"}
                icon={Award}
                accent="text-[hsl(var(--warning))]"
              />
              <StatTile
                label="Learning material"
                value={formatDuration(data.summary.learningMinutes)}
                hint={`${data.summary.lessonsAvailable} lessons available`}
                icon={Clock}
              />
            </section>

            <section>
              <div className="mb-4 flex items-center justify-between gap-3">
                <h2 className="text-lg font-semibold tracking-tight">Continue learning</h2>
                <LinkButton to="/my-courses" variant="ghost" size="sm">
                  View all
                </LinkButton>
              </div>

              {data.continueLearning.length === 0 ? (
                <Card>
                  <CardContent className="flex flex-col items-center gap-3 py-10 text-center">
                    <span className="rounded-2xl bg-muted p-3 text-primary">
                      <Compass className="h-6 w-6" aria-hidden />
                    </span>
                    <div>
                      <p className="font-medium">
                        {data.summary.completedCourses > 0
                          ? "You have finished everything you started."
                          : "You have not joined a course yet."}
                      </p>
                      <p className="mt-1 text-sm text-muted-foreground">
                        Browse the catalog to find your next one.
                      </p>
                    </div>
                    <LinkButton to="/catalog" size="sm">
                      <Compass className="h-4 w-4" />
                      Browse catalog
                    </LinkButton>
                  </CardContent>
                </Card>
              ) : (
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                  <AnimatePresence mode="popLayout">
                    {data.continueLearning.map((course) => (
                      <ContinueLearningCard
                        key={course.enrollmentId}
                        course={course}
                        onProgressChange={handleProgressChange}
                        isSaving={updateProgress.isPending}
                      />
                    ))}
                  </AnimatePresence>
                </div>
              )}
            </section>

            <section className="grid gap-4 lg:grid-cols-3">
              <Card className="lg:col-span-2">
                <CardHeader>
                  <CardTitle className="text-base">Your subjects</CardTitle>
                  <CardDescription>Average progress in each area you are studying</CardDescription>
                </CardHeader>
                <CardContent>
                  <CategoryProgressPanel categories={data.categoryProgress} />
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Enrolment activity</CardTitle>
                  <CardDescription>Courses joined per month</CardDescription>
                </CardHeader>
                <CardContent>
                  <ActivityChart points={data.enrollmentActivity} />
                </CardContent>
              </Card>
            </section>

            <section className="grid gap-4 lg:grid-cols-3">
              <Card className="lg:col-span-2">
                <CardHeader>
                  <CardTitle className="flex items-center gap-2 text-base">
                    <Sparkles className="h-4 w-4 text-primary" aria-hidden />
                    Suggested for you
                  </CardTitle>
                  <CardDescription>Popular courses you have not joined yet</CardDescription>
                </CardHeader>
                <CardContent>
                  <RecommendedList courses={data.recommended} />
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Completed</CardTitle>
                  <CardDescription>
                    {data.completed.length > 0
                      ? `${data.completed.length} course${data.completed.length === 1 ? "" : "s"} finished`
                      : "Finish a course to see it here"}
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  {data.completed.length === 0 ? (
                    <p className="text-sm text-muted-foreground">
                      Nothing yet. Your first completion will show up here.
                    </p>
                  ) : (
                    <ul className="space-y-3">
                      {data.completed.map((course) => (
                        <li key={course.enrollmentId} className="flex items-center gap-3">
                          <span className="rounded-lg bg-success/10 p-2 text-success">
                            <BookOpen className="h-4 w-4" aria-hidden />
                          </span>
                          <span className="min-w-0 flex-1">
                            <span className="block truncate text-sm font-medium">{course.title}</span>
                            <span className="block text-xs text-muted-foreground">{course.code}</span>
                          </span>
                        </li>
                      ))}
                    </ul>
                  )}
                </CardContent>
              </Card>
            </section>
          </div>
        )}
      </main>
    </div>
  );
}
