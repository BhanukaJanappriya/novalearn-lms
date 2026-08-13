import { useMemo, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { AlertTriangle, ClipboardCheck, ClipboardList, FileText, Search } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { getApiErrorMessage } from "@/lib/apiError";
import { staggerContainer } from "@/lib/motion";
import { useAssessmentOverview } from "../api/overviewQueries";
import { AssessmentOverviewCard } from "../components/AssessmentOverviewCard";
import { filterItems, type OverviewFilter } from "../lib/overview";

interface AssessmentsPageProps {
  /**
   * Locks the page to one kind of work. The Assignments entry in the sidebar is the same page
   * with the tabs already narrowed, rather than a second implementation of the same list.
   */
  only?: "assignments" | "quizzes";
}

const allTabs: { id: OverviewFilter; label: string }[] = [
  { id: "marking", label: "Needs marking" },
  { id: "all", label: "Everything" },
  { id: "assignments", label: "Assignments" },
  { id: "quizzes", label: "Quizzes" },
  { id: "drafts", label: "Drafts" },
];

/**
 * The assessment hub: every assignment and quiz across the courses the caller is responsible for,
 * in one list, with what is waiting on a person surfaced first.
 *
 * Nothing is authored or marked here. Each row links into the per course screens that already own
 * those jobs, so there is one place that writes to an assignment and one place that writes to a
 * quiz, no matter how you arrived.
 */
export function AssessmentsPage({ only }: AssessmentsPageProps) {
  const { data, isLoading, isError, error } = useAssessmentOverview();
  const [filter, setFilter] = useState<OverviewFilter>("marking");
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search, 250);

  const tabs = only
    ? allTabs.filter((tab) => tab.id === "marking" || tab.id === "all" || tab.id === "drafts")
    : allTabs;

  const scoped = useMemo(() => {
    const items = data?.items ?? [];
    if (!only) return items;
    return items.filter((item) =>
      only === "assignments" ? item.kind === "Assignment" : item.kind === "Quiz",
    );
  }, [data, only]);

  const visible = useMemo(
    () => filterItems(scoped, filter, debouncedSearch),
    [scoped, filter, debouncedSearch],
  );

  const summary = data?.summary;
  const title = only === "assignments" ? "Assignments" : "Assessments";
  const awaiting = only ? scoped.reduce((sum, i) => sum + i.awaitingMarkingCount, 0) : summary?.awaitingMarking ?? 0;

  const stats = [
    { label: "Awaiting marking", value: awaiting, icon: ClipboardCheck, tint: "text-primary" },
    { label: "Due within a week", value: summary?.dueSoon ?? 0, icon: ClipboardList, tint: "text-warning" },
    { label: "Overdue", value: summary?.overdue ?? 0, icon: AlertTriangle, tint: "text-destructive" },
    { label: "Drafts", value: summary?.drafts ?? 0, icon: FileText, tint: "text-muted-foreground" },
  ];

  return (
    <PageTransition>
      <div className="space-y-6">
        <header>
          <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
            <ClipboardList className="h-6 w-6 text-primary" aria-hidden />
            {title}
          </h1>
          <p className="mt-1 text-muted-foreground">
            {awaiting > 0
              ? `${awaiting} piece${awaiting === 1 ? "" : "s"} of work waiting to be marked.`
              : "Everything handed in has been marked."}
          </p>
        </header>

        {isError && (
          <Alert variant="error">
            {getApiErrorMessage(error, "We could not load your assessments.")}
          </Alert>
        )}

        {!only && (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {stats.map(({ label, value, icon: Icon, tint }) => (
              <div key={label} className="rounded-[18px] border border-border bg-card p-4 shadow-soft">
                <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
                  <Icon className={`h-3.5 w-3.5 ${tint}`} aria-hidden />
                  {label}
                </p>
                <p className="mt-1 text-2xl font-semibold tabular-nums">
                  {isLoading ? "—" : value}
                </p>
              </div>
            ))}
          </div>
        )}

        <div className="flex flex-wrap items-center gap-3">
          <div className="flex flex-wrap gap-1.5">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                type="button"
                onClick={() => setFilter(tab.id)}
                aria-pressed={filter === tab.id}
                className={`rounded-lg px-3 py-1.5 text-xs font-medium transition-colors ${
                  filter === tab.id
                    ? "bg-primary/10 text-primary"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground"
                }`}
              >
                {tab.label}
              </button>
            ))}
          </div>

          <div className="relative min-w-[220px] flex-1">
            <Search
              className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
              aria-hidden
            />
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search by title or course"
              aria-label="Search assessments"
              className="pl-9"
            />
          </div>

          {data && <Badge variant="neutral">{visible.length} shown</Badge>}
        </div>

        {isLoading ? (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }).map((_, index) => (
              <Skeleton key={index} className="h-48 rounded-[18px]" />
            ))}
          </div>
        ) : visible.length === 0 ? (
          <div className="rounded-[18px] border border-dashed border-border py-16 text-center">
            <p className="font-medium">
              {filter === "marking"
                ? "Nothing is waiting to be marked."
                : "No assessments match that search."}
            </p>
            <p className="mt-1 text-sm text-muted-foreground">
              {filter === "marking"
                ? "Switch to Everything to see the full list."
                : "Try a different title, or clear the search."}
            </p>
          </div>
        ) : (
          <motion.div
            className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
            initial="hidden"
            animate="visible"
            variants={staggerContainer}
          >
            <AnimatePresence mode="popLayout">
              {visible.map((item) => (
                <AssessmentOverviewCard key={`${item.kind}-${item.id}`} item={item} />
              ))}
            </AnimatePresence>
          </motion.div>
        )}
      </div>
    </PageTransition>
  );
}
