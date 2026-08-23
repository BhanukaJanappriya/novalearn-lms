import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Download, FileText, Play } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { getApiErrorMessage } from "@/lib/apiError";
import { ChartCard } from "@/features/admin/components/charts/ChartCard";
import { statusFilters as paymentStatuses, statusLabel as paymentStatusLabel } from "@/features/finance/lib/finance";
import {
  allCategories as ticketCategories,
  allPriorities as ticketPriorities,
  allStatuses as ticketStatuses,
  categoryLabel as ticketCategoryLabel,
  statusLabel as ticketStatusLabel,
} from "@/features/support/lib/support";
import { useAssignableRoles } from "@/features/users/api/queries";
import {
  reportsKeys,
  useCoursePerformanceReport,
  useEnrollmentsReport,
  useRecentReportRuns,
  useRevenueReport,
  useSupportTicketsReport,
  useUsersReport,
} from "../api/queries";
import type {
  EnrollmentStatus,
  PaymentStatus,
  ReportType,
  TicketCategory,
  TicketPriority,
  TicketStatus,
} from "../api/types";
import { RecentReportRunsPanel } from "../components/RecentReportRunsPanel";
import { ReportResults } from "../components/ReportResults";
import {
  coursePerformanceColumns,
  enrollmentColumns,
  revenueColumns,
  supportTicketsColumns,
  usersColumns,
} from "../lib/columns";
import { downloadCsv, toCsv } from "../lib/csv";

const PAGE_SIZE = 15;

const reportTypes: { value: ReportType; label: string }[] = [
  { value: "Enrollments", label: "Enrollments" },
  { value: "Revenue", label: "Revenue" },
  { value: "CoursePerformance", label: "Course performance" },
  { value: "Users", label: "Users" },
  { value: "SupportTickets", label: "Support tickets" },
];

const enrollmentStatuses: EnrollmentStatus[] = ["Active", "Completed", "Dropped"];

function toIsoOrUndefined(date: string, endOfDay = false): string | undefined {
  if (!date) return undefined;
  return new Date(`${date}T${endOfDay ? "23:59:59" : "00:00:00"}`).toISOString();
}

/**
 * The reporting centre: five exportable views over the platform's core operational data, each
 * generated on demand rather than kept live — a report is something staff explicitly asks for,
 * and every run is logged to the panel on the right. Distinct from Analytics and Finance, which
 * are live dashboards: this is a generation-and-export tool, not another chart.
 */
export function ReportsPage() {
  const queryClient = useQueryClient();
  const [reportType, setReportType] = useState<ReportType>("Enrollments");
  const [page, setPage] = useState(1);

  const [enrollmentStatus, setEnrollmentStatus] = useState<EnrollmentStatus | "">("");
  const [enrollmentFrom, setEnrollmentFrom] = useState("");
  const [enrollmentTo, setEnrollmentTo] = useState("");

  const [revenueStatus, setRevenueStatus] = useState<PaymentStatus | "">("");
  const [revenueFrom, setRevenueFrom] = useState("");
  const [revenueTo, setRevenueTo] = useState("");

  const [usersSearch, setUsersSearch] = useState("");
  const [usersRole, setUsersRole] = useState("");
  const [usersActive, setUsersActive] = useState<"" | "true" | "false">("");
  const [usersVerified, setUsersVerified] = useState<"" | "true" | "false">("");

  const [ticketStatus, setTicketStatus] = useState<TicketStatus | "">("");
  const [ticketCategory, setTicketCategory] = useState<TicketCategory | "">("");
  const [ticketPriority, setTicketPriority] = useState<TicketPriority | "">("");

  const { data: roles } = useAssignableRoles();

  const enrollmentsQuery = useEnrollmentsReport(
    {
      status: enrollmentStatus || undefined,
      fromUtc: toIsoOrUndefined(enrollmentFrom),
      toUtc: toIsoOrUndefined(enrollmentTo, true),
    },
    false,
  );
  const revenueQuery = useRevenueReport(
    {
      status: revenueStatus || undefined,
      fromUtc: toIsoOrUndefined(revenueFrom),
      toUtc: toIsoOrUndefined(revenueTo, true),
    },
    false,
  );
  const coursePerformanceQuery = useCoursePerformanceReport(false);
  const usersQuery = useUsersReport(
    {
      search: usersSearch || undefined,
      role: usersRole || undefined,
      isActive: usersActive === "" ? undefined : usersActive === "true",
      emailConfirmed: usersVerified === "" ? undefined : usersVerified === "true",
    },
    false,
  );
  const ticketsQuery = useSupportTicketsReport(
    { status: ticketStatus || undefined, category: ticketCategory || undefined, priority: ticketPriority || undefined },
    false,
  );

  const recentRuns = useRecentReportRuns();

  const changeReportType = (next: ReportType) => {
    setReportType(next);
    setPage(1);
  };

  const handleGenerate = async () => {
    setPage(1);

    switch (reportType) {
      case "Enrollments":
        await enrollmentsQuery.refetch();
        break;
      case "Revenue":
        await revenueQuery.refetch();
        break;
      case "CoursePerformance":
        await coursePerformanceQuery.refetch();
        break;
      case "Users":
        await usersQuery.refetch();
        break;
      case "SupportTickets":
        await ticketsQuery.refetch();
        break;
    }

    queryClient.invalidateQueries({ queryKey: reportsKeys.recentRuns });
  };

  const handleExport = () => {
    const date = new Date().toISOString().slice(0, 10);
    const filename = (name: string) => `${name}-report-${date}.csv`;

    switch (reportType) {
      case "Enrollments":
        if (enrollmentsQuery.data) downloadCsv(filename("enrollments"), toCsv(enrollmentsQuery.data, enrollmentColumns));
        break;
      case "Revenue":
        if (revenueQuery.data) downloadCsv(filename("revenue"), toCsv(revenueQuery.data, revenueColumns));
        break;
      case "CoursePerformance":
        if (coursePerformanceQuery.data)
          downloadCsv(filename("course-performance"), toCsv(coursePerformanceQuery.data, coursePerformanceColumns));
        break;
      case "Users":
        if (usersQuery.data) downloadCsv(filename("users"), toCsv(usersQuery.data, usersColumns));
        break;
      case "SupportTickets":
        if (ticketsQuery.data)
          downloadCsv(filename("support-tickets"), toCsv(ticketsQuery.data, supportTicketsColumns));
        break;
    }
  };

  const activeQuery = {
    Enrollments: enrollmentsQuery,
    Revenue: revenueQuery,
    CoursePerformance: coursePerformanceQuery,
    Users: usersQuery,
    SupportTickets: ticketsQuery,
  }[reportType];

  const canExport = Boolean(activeQuery.data && activeQuery.data.length > 0);

  return (
    <PageTransition>
      <div className="grid gap-6 lg:grid-cols-[1fr_320px]">
        <div className="space-y-6">
          <header>
            <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
              <FileText className="h-6 w-6 text-primary" aria-hidden />
              Reports
            </h1>
            <p className="mt-1 text-muted-foreground">
              Generate and export a report over any of the platform's core operational data.
            </p>
          </header>

          <div className="flex flex-wrap gap-1.5">
            {reportTypes.map((option) => (
              <button
                key={option.value}
                type="button"
                onClick={() => changeReportType(option.value)}
                aria-pressed={reportType === option.value}
                className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
                  reportType === option.value
                    ? "bg-primary/10 text-primary"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground"
                }`}
              >
                {option.label}
              </button>
            ))}
          </div>

          {activeQuery.isError && (
            <Alert variant="error">{getApiErrorMessage(activeQuery.error, "We could not generate that report.")}</Alert>
          )}

          <ChartCard
            title="Filters"
            actions={
              <div className="flex items-center gap-2">
                <Button variant="outline" size="sm" onClick={handleExport} disabled={!canExport}>
                  <Download className="h-4 w-4" />
                  Export CSV
                </Button>
                <Button size="sm" onClick={handleGenerate} isLoading={activeQuery.isFetching}>
                  <Play className="h-4 w-4" />
                  Generate report
                </Button>
              </div>
            }
          >
            {reportType === "Enrollments" && (
              <div className="flex flex-wrap items-end gap-3">
                <div className="space-y-1.5">
                  <Label htmlFor="enrollment-status">Status</Label>
                  <select
                    id="enrollment-status"
                    value={enrollmentStatus}
                    onChange={(e) => setEnrollmentStatus(e.target.value as EnrollmentStatus | "")}
                    className="h-9 rounded-lg border border-border bg-background px-2 text-sm"
                  >
                    <option value="">Any status</option>
                    {enrollmentStatuses.map((status) => (
                      <option key={status} value={status}>
                        {status}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="enrollment-from">Enrolled from</Label>
                  <Input
                    id="enrollment-from"
                    type="date"
                    value={enrollmentFrom}
                    onChange={(e) => setEnrollmentFrom(e.target.value)}
                    className="h-9"
                  />
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="enrollment-to">Enrolled to</Label>
                  <Input
                    id="enrollment-to"
                    type="date"
                    value={enrollmentTo}
                    onChange={(e) => setEnrollmentTo(e.target.value)}
                    className="h-9"
                  />
                </div>
              </div>
            )}

            {reportType === "Revenue" && (
              <div className="flex flex-wrap items-end gap-3">
                <div className="space-y-1.5">
                  <Label htmlFor="revenue-status">Status</Label>
                  <select
                    id="revenue-status"
                    value={revenueStatus}
                    onChange={(e) => setRevenueStatus(e.target.value as PaymentStatus | "")}
                    className="h-9 rounded-lg border border-border bg-background px-2 text-sm"
                  >
                    <option value="">Any status</option>
                    {paymentStatuses.map((status) => (
                      <option key={status} value={status}>
                        {paymentStatusLabel[status]}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="revenue-from">Paid from</Label>
                  <Input
                    id="revenue-from"
                    type="date"
                    value={revenueFrom}
                    onChange={(e) => setRevenueFrom(e.target.value)}
                    className="h-9"
                  />
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="revenue-to">Paid to</Label>
                  <Input
                    id="revenue-to"
                    type="date"
                    value={revenueTo}
                    onChange={(e) => setRevenueTo(e.target.value)}
                    className="h-9"
                  />
                </div>
              </div>
            )}

            {reportType === "CoursePerformance" && (
              <p className="text-sm text-muted-foreground">
                Lifetime figures for every course — no filters needed, the same numbers Analytics shows.
              </p>
            )}

            {reportType === "Users" && (
              <div className="flex flex-wrap items-end gap-3">
                <div className="min-w-[200px] flex-1 space-y-1.5">
                  <Label htmlFor="users-search">Search</Label>
                  <Input
                    id="users-search"
                    value={usersSearch}
                    onChange={(e) => setUsersSearch(e.target.value)}
                    placeholder="Name or email"
                    className="h-9"
                  />
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="users-role">Role</Label>
                  <select
                    id="users-role"
                    value={usersRole}
                    onChange={(e) => setUsersRole(e.target.value)}
                    className="h-9 rounded-lg border border-border bg-background px-2 text-sm"
                  >
                    <option value="">Any role</option>
                    {(roles ?? []).map((role) => (
                      <option key={role} value={role}>
                        {role}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="users-active">Active</Label>
                  <select
                    id="users-active"
                    value={usersActive}
                    onChange={(e) => setUsersActive(e.target.value as "" | "true" | "false")}
                    className="h-9 rounded-lg border border-border bg-background px-2 text-sm"
                  >
                    <option value="">Any</option>
                    <option value="true">Active</option>
                    <option value="false">Deactivated</option>
                  </select>
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="users-verified">Verified</Label>
                  <select
                    id="users-verified"
                    value={usersVerified}
                    onChange={(e) => setUsersVerified(e.target.value as "" | "true" | "false")}
                    className="h-9 rounded-lg border border-border bg-background px-2 text-sm"
                  >
                    <option value="">Any</option>
                    <option value="true">Verified</option>
                    <option value="false">Unverified</option>
                  </select>
                </div>
              </div>
            )}

            {reportType === "SupportTickets" && (
              <div className="flex flex-wrap items-end gap-3">
                <div className="space-y-1.5">
                  <Label htmlFor="ticket-status">Status</Label>
                  <select
                    id="ticket-status"
                    value={ticketStatus}
                    onChange={(e) => setTicketStatus(e.target.value as TicketStatus | "")}
                    className="h-9 rounded-lg border border-border bg-background px-2 text-sm"
                  >
                    <option value="">Any status</option>
                    {ticketStatuses.map((status) => (
                      <option key={status} value={status}>
                        {ticketStatusLabel[status]}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="ticket-category">Category</Label>
                  <select
                    id="ticket-category"
                    value={ticketCategory}
                    onChange={(e) => setTicketCategory(e.target.value as TicketCategory | "")}
                    className="h-9 rounded-lg border border-border bg-background px-2 text-sm"
                  >
                    <option value="">Any category</option>
                    {ticketCategories.map((category) => (
                      <option key={category} value={category}>
                        {ticketCategoryLabel[category]}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="ticket-priority">Priority</Label>
                  <select
                    id="ticket-priority"
                    value={ticketPriority}
                    onChange={(e) => setTicketPriority(e.target.value as TicketPriority | "")}
                    className="h-9 rounded-lg border border-border bg-background px-2 text-sm"
                  >
                    <option value="">Any priority</option>
                    {ticketPriorities.map((priority) => (
                      <option key={priority} value={priority}>
                        {priority}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
            )}
          </ChartCard>

          <ChartCard title="Results" subtitle={reportTypes.find((r) => r.value === reportType)?.label}>
            {reportType === "Enrollments" && (
              <ReportResults
                query={enrollmentsQuery}
                columns={enrollmentColumns}
                rowKey={(row) => row.enrollmentId}
                emptyMessage="No enrolments match these filters."
                page={page}
                pageSize={PAGE_SIZE}
                onPageChange={setPage}
              />
            )}
            {reportType === "Revenue" && (
              <ReportResults
                query={revenueQuery}
                columns={revenueColumns}
                rowKey={(row) => row.id}
                emptyMessage="No transactions match these filters."
                page={page}
                pageSize={PAGE_SIZE}
                onPageChange={setPage}
              />
            )}
            {reportType === "CoursePerformance" && (
              <ReportResults
                query={coursePerformanceQuery}
                columns={coursePerformanceColumns}
                rowKey={(row) => row.courseId}
                emptyMessage="No courses to show yet."
                page={page}
                pageSize={PAGE_SIZE}
                onPageChange={setPage}
              />
            )}
            {reportType === "Users" && (
              <ReportResults
                query={usersQuery}
                columns={usersColumns}
                rowKey={(row) => row.id}
                emptyMessage="No accounts match these filters."
                page={page}
                pageSize={PAGE_SIZE}
                onPageChange={setPage}
              />
            )}
            {reportType === "SupportTickets" && (
              <ReportResults
                query={ticketsQuery}
                columns={supportTicketsColumns}
                rowKey={(row) => row.id}
                emptyMessage="No tickets match these filters."
                page={page}
                pageSize={PAGE_SIZE}
                onPageChange={setPage}
              />
            )}
          </ChartCard>
        </div>

        <RecentReportRunsPanel runs={recentRuns.data} isLoading={recentRuns.isLoading} />
      </div>
    </PageTransition>
  );
}
