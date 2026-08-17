import { useState } from "react";
import { Award, BarChart3, GraduationCap, TrendingUp, Users } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { getApiErrorMessage } from "@/lib/apiError";
import { ChartCard, RangeChips } from "@/features/admin/components/charts/ChartCard";
import { useAnalytics } from "../api/queries";
import { EnrolmentTrendChart, ScoreDistributionChart } from "../components/AnalyticsCharts";
import { CoursePerformanceTable } from "../components/CoursePerformanceTable";
import { TrendMetricCard } from "../components/TrendMetricCard";

const RANGES = ["7d", "30d", "90d", "1y"] as const;
type Range = (typeof RANGES)[number];

const rangeDays: Record<Range, number> = { "7d": 7, "30d": 30, "90d": 90, "1y": 365 };

/**
 * Platform analytics.
 *
 * Deliberately not a second dashboard. The dashboard answers what the platform looks like right
 * now; this answers how it is going, which is why every headline figure carries a comparison
 * against the previous window of the same length and the tables are about learning rather than
 * about accounts.
 */
export function AnalyticsPage() {
  const [range, setRange] = useState<Range>("30d");
  const { data, isLoading, isError, error } = useAnalytics(rangeDays[range]);

  return (
    <PageTransition>
      <div className="space-y-6">
        <header className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
              <BarChart3 className="h-6 w-6 text-primary" aria-hidden />
              Analytics
            </h1>
            <p className="mt-1 text-muted-foreground">
              How teaching and learning are going, compared with the period before.
            </p>
          </div>
          <RangeChips options={RANGES} value={range} onChange={setRange} />
        </header>

        {isError && (
          <Alert variant="error">
            {getApiErrorMessage(error, "We could not load analytics.")}
          </Alert>
        )}

        {isLoading && (
          <>
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
              {Array.from({ length: 4 }).map((_, index) => (
                <Skeleton key={index} className="h-24 rounded-[18px]" />
              ))}
            </div>
            <Skeleton className="h-80 rounded-[18px]" />
          </>
        )}

        {data && (
          <>
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
              <TrendMetricCard
                label="New enrolments"
                metric={data.headline.enrolments}
                icon={GraduationCap}
              />
              <TrendMetricCard
                label="Completions"
                metric={data.headline.completions}
                icon={Award}
              />
              <TrendMetricCard
                label="Active learners"
                metric={data.headline.activeLearners}
                icon={Users}
              />
              <TrendMetricCard
                label="Average mark"
                metric={data.headline.averageScorePercent}
                icon={TrendingUp}
                suffix="%"
              />
            </div>

            <ChartCard
              title="Enrolments and completions"
              subtitle={`Bucketed by ${data.window.granularity.toLowerCase()} across the last ${data.window.days} days`}
            >
              <EnrolmentTrendChart data={data.series} granularity={data.window.granularity} />
            </ChartCard>

            <div className="grid gap-4 lg:grid-cols-3">
              <ChartCard
                title="Marks awarded"
                subtitle="Assignments and quizzes, this period"
                className="lg:col-span-2"
              >
                <ScoreDistributionChart data={data.scoreDistribution} />
              </ChartCard>

              <ChartCard title="By department" subtitle="Completion across all time">
                {data.departments.length === 0 ? (
                  <p className="py-8 text-center text-sm text-muted-foreground">
                    No courses yet.
                  </p>
                ) : (
                  <ul className="space-y-3">
                    {data.departments.map((department) => (
                      <li key={department.name}>
                        <div className="flex items-baseline justify-between gap-2 text-sm">
                          <span className="truncate font-medium">{department.name}</span>
                          <span className="shrink-0 text-xs tabular-nums text-muted-foreground">
                            {department.completionRatePercent}%
                          </span>
                        </div>
                        <Progress
                          value={department.completionRatePercent}
                          label={`${department.name} completion`}
                          size="sm"
                          className="mt-1"
                        />
                        <p className="mt-1 text-xs text-muted-foreground">
                          {department.courses} course{department.courses === 1 ? "" : "s"} ·{" "}
                          {department.enrolled} enrolled · {department.completed} completed
                        </p>
                      </li>
                    ))}
                  </ul>
                )}
              </ChartCard>
            </div>

            <ChartCard
              title="Course performance"
              subtitle="Lifetime figures, busiest first. At risk means active, enrolled over a fortnight, under a quarter done."
            >
              <CoursePerformanceTable rows={data.courses} />
            </ChartCard>
          </>
        )}
      </div>
    </PageTransition>
  );
}
