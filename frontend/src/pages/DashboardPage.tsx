import { Compass, GraduationCap, TrendingUp } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { LinkButton } from "@/components/ui/link-button";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { useAuth } from "@/context/AuthContext";
import { useMyEnrollments } from "@/features/enrollments/api/queries";

export function DashboardPage() {
  const { user } = useAuth();
  const { data: enrollments, isLoading } = useMyEnrollments();

  const active = (enrollments ?? []).filter((e) => e.status !== "Dropped");
  const completed = active.filter((e) => e.status === "Completed").length;
  const averageProgress = active.length
    ? Math.round(active.reduce((sum, e) => sum + e.progressPercent, 0) / active.length)
    : 0;

  return (
    <div className="min-h-screen">
      <LearnerHeader />

      <main className="mx-auto max-w-6xl px-6 py-10">
        <h1 className="text-2xl font-semibold tracking-tight">
          Welcome back{user ? `, ${user.fullName}` : ""} 👋
        </h1>
        <p className="mt-1 text-muted-foreground">Here&apos;s your learning space.</p>

        <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Your account</CardTitle>
              <CardDescription>Signed in as {user?.email}</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {user?.roles.map((role) => (
                  <span
                    key={role}
                    className="rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-medium text-primary"
                  >
                    {role}
                  </span>
                ))}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Enrolled courses</CardTitle>
              <CardDescription>
                {completed > 0 ? `${completed} completed so far` : "Everything you have joined"}
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {isLoading ? (
                <Skeleton className="h-9 w-16" />
              ) : (
                <div className="flex items-baseline gap-2">
                  <span className="text-3xl font-semibold tabular-nums">{active.length}</span>
                  <span className="text-sm text-muted-foreground">
                    course{active.length === 1 ? "" : "s"}
                  </span>
                </div>
              )}
              <LinkButton to="/my-courses" variant="outline" size="sm">
                <GraduationCap className="h-4 w-4" />
                My courses
              </LinkButton>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Average progress</CardTitle>
              <CardDescription>Across your active enrollments</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {isLoading ? (
                <Skeleton className="h-9 w-20" />
              ) : (
                <>
                  <div className="flex items-baseline gap-2">
                    <span className="text-3xl font-semibold tabular-nums">{averageProgress}%</span>
                    <TrendingUp className="h-4 w-4 text-success" aria-hidden />
                  </div>
                  <Progress value={averageProgress} label="Average" />
                </>
              )}
              <LinkButton to="/catalog" size="sm">
                <Compass className="h-4 w-4" />
                Browse catalog
              </LinkButton>
            </CardContent>
          </Card>
        </div>
      </main>
    </div>
  );
}
