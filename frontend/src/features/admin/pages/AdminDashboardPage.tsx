import { useState } from "react";
import { motion } from "framer-motion";
import { Download, RefreshCw, TriangleAlert } from "lucide-react";
import { Button } from "@/components/ui/button";
import { exportCsv } from "@/lib/exportCsv";
import { useAuth } from "@/context/AuthContext";
import { useAdminDashboard } from "../api/queries";
import { ExecutiveSummary } from "../components/ExecutiveSummary";
import { KpiCard } from "../components/KpiCard";
import { DashboardSkeleton } from "../components/DashboardSkeleton";
import { ChartCard, RangeChips } from "../components/charts/ChartCard";
import { TrendAreaChart } from "../components/charts/TrendAreaChart";
import { RoleDonutChart } from "../components/charts/RoleDonutChart";
import { WeeklyActivityChart } from "../components/charts/WeeklyActivityChart";
import { PopularCoursesChart } from "../components/charts/PopularCoursesChart";
import { ActivityFeed } from "../components/ActivityFeed";
import { PendingApprovals } from "../components/PendingApprovals";
import { SystemHealth } from "../components/SystemHealth";
import { SecurityPanel } from "../components/SecurityPanel";
import { AiInsightsPanel } from "../components/AiInsightsPanel";
import { QuickActions } from "../components/QuickActions";

const RANGES = ["7D", "30D", "90D", "1Y"] as const;

const stagger = {
  hidden: { opacity: 0, y: 12 },
  show: { opacity: 1, y: 0 },
};

export function AdminDashboardPage() {
  const { user } = useAuth();
  const { data, isLoading, isError, refetch, isFetching } = useAdminDashboard();
  const [range, setRange] = useState<(typeof RANGES)[number]>("1Y");

  if (isLoading) return <DashboardSkeleton />;

  if (isError || !data) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center text-center">
        <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-destructive/10">
          <TriangleAlert className="h-7 w-7 text-destructive" />
        </div>
        <h2 className="mt-4 text-lg font-semibold">Couldn’t load the dashboard</h2>
        <p className="mt-1 max-w-sm text-sm text-muted-foreground">
          The admin service didn’t respond. Check your connection and try again.
        </p>
        <Button className="mt-4" onClick={() => refetch()}>
          <RefreshCw className="h-4 w-4" />
          Retry
        </Button>
      </div>
    );
  }

  const adminName = user?.fullName?.split(" ")[0] ?? "Administrator";

  return (
    <div className="space-y-6">
      <ExecutiveSummary data={data.summary} adminName={adminName} />

      {/* KPIs */}
      <section>
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-sm font-semibold uppercase tracking-wider text-muted-foreground">Key metrics</h2>
          <div className="flex items-center gap-2">
            <span className="inline-flex items-center gap-1.5 text-xs text-muted-foreground">
              <span className="relative flex h-2 w-2">
                <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-success opacity-75" />
                <span className="relative inline-flex h-2 w-2 rounded-full bg-success" />
              </span>
              Live
            </span>
            <Button variant="outline" size="sm" onClick={() => refetch()} isLoading={isFetching}>
              <RefreshCw className="h-3.5 w-3.5" />
              Refresh
            </Button>
          </div>
        </div>
        <motion.div
          initial="hidden"
          animate="show"
          transition={{ staggerChildren: 0.03 }}
          className="grid grid-cols-2 gap-4 md:grid-cols-3 xl:grid-cols-4"
        >
          {data.kpis.map((metric) => (
            <motion.div key={metric.id} variants={stagger}>
              <KpiCard metric={metric} />
            </motion.div>
          ))}
        </motion.div>
      </section>

      {/* Analytics row 1 */}
      <section className="grid gap-4 lg:grid-cols-3">
        <ChartCard
          title="Enrollment trend"
          subtitle="New enrolments vs. previous year"
          className="lg:col-span-2"
          actions={
            <>
              <RangeChips options={RANGES} value={range} onChange={setRange} />
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8"
                aria-label="Export CSV"
                title="Export CSV"
                onClick={() =>
                  exportCsv(
                    "enrollment-trend.csv",
                    data.enrollmentTrend.map((p) => ({
                      period: p.label,
                      thisYear: p.value,
                      previousYear: p.compare ?? 0,
                    })),
                  )
                }
              >
                <Download className="h-4 w-4" />
              </Button>
            </>
          }
        >
          <TrendAreaChart data={data.enrollmentTrend} />
        </ChartCard>

        <ChartCard title="Users by role" subtitle="Distribution across the platform">
          <RoleDonutChart data={data.roleDistribution} />
        </ChartCard>
      </section>

      {/* Analytics row 2 */}
      <section className="grid gap-4 lg:grid-cols-3">
        <ChartCard title="Weekly activity" subtitle="Active users per day" className="lg:col-span-2">
          <WeeklyActivityChart data={data.weeklyActivity} />
        </ChartCard>
        <ChartCard title="Popular courses" subtitle="By active enrolments">
          <PopularCoursesChart data={data.popularCourses} />
        </ChartCard>
      </section>

      {/* Quick actions */}
      <ChartCard title="Quick actions" subtitle="Common administrative tasks">
        <QuickActions />
      </ChartCard>

      {/* Operational row */}
      <section className="grid gap-4 xl:grid-cols-3">
        <ChartCard title="Recent activity" subtitle="Latest events across the platform">
          <ActivityFeed items={data.activity} />
        </ChartCard>
        <ChartCard title="Pending approvals" subtitle={`${data.approvals.length} awaiting review`}>
          <PendingApprovals items={data.approvals} />
        </ChartCard>
        <ChartCard title="AI insights" subtitle="Recommendations for your attention">
          <AiInsightsPanel items={data.insights} />
        </ChartCard>
      </section>

      {/* Infra row */}
      <section className="grid gap-4 xl:grid-cols-2">
        <ChartCard title="System health" subtitle="Services & resource utilisation">
          <SystemHealth data={data.health} />
        </ChartCard>
        <ChartCard title="Security center" subtitle="Posture & recent security events">
          <SecurityPanel data={data.security} />
        </ChartCard>
      </section>
    </div>
  );
}
