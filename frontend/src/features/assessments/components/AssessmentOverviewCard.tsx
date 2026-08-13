import { motion } from "framer-motion";
import { ArrowRight, ClipboardList, FileText, ListChecks, Users } from "lucide-react";
import { Link } from "react-router-dom";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { staggerItem } from "@/lib/motion";
import type { AssessmentOverviewItem } from "../api/overviewQueries";
import { dueLabel, dueState, manageHref, markingHref, submissionPercent } from "../lib/overview";

interface AssessmentOverviewCardProps {
  item: AssessmentOverviewItem;
}

const dueVariant = {
  overdue: "destructive",
  soon: "warning",
  later: "neutral",
  none: "neutral",
} as const;

/** One piece of assessed work, as a card in the cross course list. */
export function AssessmentOverviewCard({ item }: AssessmentOverviewCardProps) {
  const KindIcon = item.kind === "Assignment" ? FileText : ListChecks;
  const state = dueState(item);
  const percent = submissionPercent(item);
  const needsMarking = item.awaitingMarkingCount > 0;

  return (
    <motion.article
      layout
      variants={staggerItem}
      exit={{ opacity: 0, scale: 0.96, transition: { duration: 0.18 } }}
      whileHover={{ y: -4 }}
      transition={{ type: "spring", stiffness: 320, damping: 24 }}
      className={`flex flex-col rounded-[18px] border bg-card p-4 shadow-soft ${
        needsMarking ? "border-primary/40" : "border-border"
      }`}
    >
      <div className="flex items-start justify-between gap-2">
        <div className="min-w-0">
          <p className="truncate text-xs text-muted-foreground">{item.courseTitle}</p>
          <h2 className="mt-0.5 flex items-center gap-1.5 font-semibold">
            <KindIcon className="h-4 w-4 shrink-0 text-primary" aria-hidden />
            <span className="truncate">{item.title}</span>
          </h2>
        </div>
        {item.status === "Draft" ? (
          <Badge variant="neutral">Draft</Badge>
        ) : (
          <Badge variant={dueVariant[state]}>{dueLabel(item)}</Badge>
        )}
      </div>

      <div className="mt-3 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-muted-foreground">
        <span className="inline-flex items-center gap-1.5">
          <Users className="h-3.5 w-3.5" aria-hidden />
          {item.submittedCount} of {item.enrolledCount} handed in
        </span>
        <span className="inline-flex items-center gap-1.5">
          <ClipboardList className="h-3.5 w-3.5" aria-hidden />
          {item.maxPoints} point{item.maxPoints === 1 ? "" : "s"}
          {item.kind === "Quiz" && ` · ${item.questionCount} question${item.questionCount === 1 ? "" : "s"}`}
        </span>
      </div>

      <div className="mt-2">
        {percent === null ? (
          <p className="text-xs text-muted-foreground">Nobody is enrolled on this course yet.</p>
        ) : (
          <Progress value={percent} label={`${item.title} submissions`} size="sm" />
        )}
      </div>

      <div className="mt-3 flex flex-wrap items-center gap-2 text-xs">
        {needsMarking ? (
          <Badge variant="warning">
            {item.awaitingMarkingCount} awaiting marking
          </Badge>
        ) : item.gradedCount > 0 ? (
          <Badge variant="success">All marked</Badge>
        ) : null}
        {item.averageScorePercent !== null && (
          <span className="text-muted-foreground">
            Average <span className="font-semibold text-foreground">{item.averageScorePercent}%</span>
          </span>
        )}
      </div>

      <div className="mt-auto flex items-center justify-between gap-2 pt-4">
        <Link
          to={manageHref(item)}
          className="text-xs font-medium text-muted-foreground hover:text-foreground"
        >
          {item.kind === "Assignment" ? "Open assignments" : "Edit quiz"}
        </Link>
        {needsMarking && (
          <Link
            to={markingHref(item)}
            className="inline-flex items-center gap-1 text-xs font-medium text-primary hover:underline"
          >
            Mark now
            <ArrowRight className="h-3.5 w-3.5" aria-hidden />
          </Link>
        )}
      </div>
    </motion.article>
  );
}
