import { AnimatePresence, motion } from "framer-motion";
import { BookOpen, GraduationCap, Layers, Users } from "lucide-react";
import { Avatar } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { timeAgo } from "@/lib/format";
import { staggerContainer, staggerItem } from "@/lib/motion";
import { avatarColor, roleVariant, sortRoles } from "@/features/users/lib/userVisuals";
import type { DirectoryEntry } from "../api/queries";

interface DirectoryGridProps {
  people: DirectoryEntry[];
}

/** Read-only people cards, shared by the students and lecturers directories. */
export function DirectoryGrid({ people }: DirectoryGridProps) {
  return (
    <motion.div
      className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
      initial="hidden"
      animate="visible"
      variants={staggerContainer}
    >
      <AnimatePresence mode="popLayout">
        {people.map((person) => (
          <motion.article
            key={person.id}
            layout
            variants={staggerItem}
            exit={{ opacity: 0, scale: 0.96, transition: { duration: 0.18 } }}
            whileHover={{ y: -4 }}
            transition={{ type: "spring", stiffness: 320, damping: 24 }}
            className="flex flex-col rounded-[18px] border border-border bg-card p-4 shadow-soft"
          >
            <div className="flex items-start gap-3">
              <Avatar
                name={person.fullName}
                src={person.avatarUrl}
                color={avatarColor(person.email)}
                size="lg"
              />
              <div className="min-w-0 flex-1">
                <div className="flex flex-wrap items-center gap-1.5">
                  <h2 className="truncate font-semibold">{person.fullName}</h2>
                  {!person.isActive && <Badge variant="neutral">Inactive</Badge>}
                </div>
                <p className="truncate text-xs text-muted-foreground">{person.email}</p>
                <div className="mt-1.5 flex flex-wrap gap-1">
                  {sortRoles(person.roles).map((role) => (
                    <Badge key={role} variant={roleVariant(role)}>
                      {role}
                    </Badge>
                  ))}
                </div>
              </div>
            </div>

            {person.learner && (
              <div className="mt-4 space-y-2">
                <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-muted-foreground">
                  <span className="inline-flex items-center gap-1.5">
                    <GraduationCap className="h-3.5 w-3.5" aria-hidden />
                    {person.learner.enrolledCourses} enrolled
                  </span>
                  <span className="inline-flex items-center gap-1.5">
                    <BookOpen className="h-3.5 w-3.5" aria-hidden />
                    {person.learner.completedCourses} completed
                  </span>
                </div>
                <div>
                  <div className="mb-1 flex items-center justify-between text-xs text-muted-foreground">
                    <span>Average progress</span>
                    <span className="font-semibold tabular-nums text-foreground">
                      {person.learner.averageProgressPercent}%
                    </span>
                  </div>
                  <Progress
                    value={person.learner.averageProgressPercent}
                    label={`${person.fullName} average progress`}
                    size="sm"
                  />
                </div>
              </div>
            )}

            {person.teacher && (
              <div className="mt-4 space-y-2">
                <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-muted-foreground">
                  <span className="inline-flex items-center gap-1.5">
                    <BookOpen className="h-3.5 w-3.5" aria-hidden />
                    {person.teacher.coursesOwned} course
                    {person.teacher.coursesOwned === 1 ? "" : "s"}
                    {person.teacher.publishedCourses > 0 &&
                      ` · ${person.teacher.publishedCourses} published`}
                  </span>
                  <span className="inline-flex items-center gap-1.5">
                    <Users className="h-3.5 w-3.5" aria-hidden />
                    {person.teacher.learnersTaught} learner
                    {person.teacher.learnersTaught === 1 ? "" : "s"}
                  </span>
                </div>
                {person.teacher.departmentsHeaded.length > 0 && (
                  <p className="inline-flex items-center gap-1.5 text-xs text-primary">
                    <Layers className="h-3.5 w-3.5" aria-hidden />
                    Heads {person.teacher.departmentsHeaded.join(", ")}
                  </p>
                )}
              </div>
            )}

            <p className="mt-auto pt-4 text-[11px] text-muted-foreground">
              Joined {new Date(person.joinedAtUtc).toLocaleDateString(undefined, {
                year: "numeric",
                month: "short",
              })}
              {person.lastActiveAtUtc
                ? ` · last seen ${timeAgo(person.lastActiveAtUtc)}`
                : " · never signed in"}
            </p>
          </motion.article>
        ))}
      </AnimatePresence>
    </motion.div>
  );
}
