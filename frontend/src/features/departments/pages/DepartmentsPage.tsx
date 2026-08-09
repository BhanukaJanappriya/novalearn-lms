import { useMemo, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { Boxes, BookOpen, Pencil, Plus, Trash2, TriangleAlert, UserRound } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Modal } from "@/components/ui/modal";
import { Skeleton } from "@/components/ui/skeleton";
import { useUsers } from "@/features/users/api/queries";
import { getApiErrorMessage } from "@/lib/apiError";
import { staggerContainer, staggerItem } from "@/lib/motion";
import { useDeleteDepartment, useDepartments, useSaveDepartment } from "../api/queries";
import type { Department, DepartmentInput } from "../api/types";
import { DepartmentFormDialog } from "../components/DepartmentFormDialog";

/** Admin view: the institution's academic departments. */
export function DepartmentsPage() {
  const { data: departments, isLoading, isError, error } = useDepartments();
  const saveDepartment = useSaveDepartment();
  const deleteDepartment = useDeleteDepartment();

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<Department | null>(null);
  const [pendingDelete, setPendingDelete] = useState<Department | null>(null);

  // Heads come from the account directory. A generous page size keeps it to one request, since
  // teaching staff are a small slice of the user base.
  const { data: staff } = useUsers({ page: 1, pageSize: 100 });
  const eligibleHeads = useMemo(
    () =>
      (staff?.items ?? []).filter((u) =>
        u.roles.some((r) => r === "Lecturer" || r === "Administrator" || r === "SuperAdministrator"),
      ),
    [staff],
  );

  const openCreate = () => {
    setEditing(null);
    setFormOpen(true);
  };

  const save = (input: DepartmentInput) =>
    saveDepartment.mutate({ id: editing?.id, input }, { onSuccess: () => setFormOpen(false) });

  const confirmDelete = () => {
    if (!pendingDelete) return;
    deleteDepartment.mutate(pendingDelete.id, { onSuccess: () => setPendingDelete(null) });
  };

  const totalCourses = (departments ?? []).reduce((sum, d) => sum + d.courseCount, 0);
  const activeCount = (departments ?? []).filter((d) => d.isActive).length;

  return (
    <PageTransition>
      <div className="space-y-6">
        <header className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
              <Boxes className="h-6 w-6 text-primary" aria-hidden />
              Departments
            </h1>
            <p className="mt-1 text-muted-foreground">
              {departments
                ? `${departments.length} department${departments.length === 1 ? "" : "s"} · ${activeCount} active · ${totalCourses} course${totalCourses === 1 ? "" : "s"} assigned`
                : "Group courses into academic faculties."}
            </p>
          </div>
          <Button size="sm" onClick={openCreate}>
            <Plus className="h-4 w-4" />
            New department
          </Button>
        </header>

        {isError && (
          <Alert variant="error">{getApiErrorMessage(error, "We could not load departments.")}</Alert>
        )}
        {deleteDepartment.isError && (
          <Alert variant="error">{getApiErrorMessage(deleteDepartment.error)}</Alert>
        )}

        {isLoading ? (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }).map((_, i) => (
              <Skeleton key={i} className="h-40 rounded-[18px]" />
            ))}
          </div>
        ) : departments && departments.length === 0 ? (
          <div className="rounded-[18px] border border-dashed border-border py-16 text-center">
            <p className="font-medium">No departments yet.</p>
            <p className="mt-1 text-sm text-muted-foreground">
              Create one to start grouping courses into faculties.
            </p>
            <Button className="mt-4" size="sm" onClick={openCreate}>
              <Plus className="h-4 w-4" />
              New department
            </Button>
          </div>
        ) : (
          <motion.div
            className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
            initial="hidden"
            animate="visible"
            variants={staggerContainer}
          >
            <AnimatePresence mode="popLayout">
              {departments?.map((department) => (
                <motion.div
                  key={department.id}
                  layout
                  variants={staggerItem}
                  exit={{ opacity: 0, scale: 0.96, transition: { duration: 0.18 } }}
                  whileHover={{ y: -4 }}
                  transition={{ type: "spring", stiffness: 320, damping: 24 }}
                  className="flex flex-col rounded-[18px] border border-border bg-card p-4 shadow-soft"
                >
                  <div className="flex items-start justify-between gap-2">
                    <div className="min-w-0">
                      <div className="flex flex-wrap items-center gap-2">
                        <Badge variant="default">{department.code}</Badge>
                        {!department.isActive && <Badge variant="neutral">Retired</Badge>}
                      </div>
                      <h2 className="mt-2 font-semibold leading-snug">{department.name}</h2>
                    </div>
                  </div>

                  {department.description && (
                    <p className="mt-2 line-clamp-2 text-sm text-muted-foreground">
                      {department.description}
                    </p>
                  )}

                  <div className="mt-3 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-muted-foreground">
                    <span className="inline-flex items-center gap-1.5">
                      <UserRound className="h-3.5 w-3.5" aria-hidden />
                      {department.headName ?? "No head assigned"}
                    </span>
                    <span className="inline-flex items-center gap-1.5">
                      <BookOpen className="h-3.5 w-3.5" aria-hidden />
                      {department.courseCount} course{department.courseCount === 1 ? "" : "s"}
                    </span>
                  </div>

                  <div className="mt-auto flex items-center justify-end gap-1 pt-4">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => {
                        setEditing(department);
                        setFormOpen(true);
                      }}
                    >
                      <Pencil className="h-3.5 w-3.5" />
                      Edit
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setPendingDelete(department)}
                      disabled={department.courseCount > 0}
                      title={
                        department.courseCount > 0
                          ? "Reassign its courses first, or retire it instead"
                          : undefined
                      }
                    >
                      <Trash2 className="h-3.5 w-3.5" />
                      Delete
                    </Button>
                  </div>
                </motion.div>
              ))}
            </AnimatePresence>
          </motion.div>
        )}

        <DepartmentFormDialog
          department={editing}
          eligibleHeads={eligibleHeads}
          open={formOpen}
          onClose={() => setFormOpen(false)}
          onSubmit={save}
          isSaving={saveDepartment.isPending}
          error={saveDepartment.error}
        />

        <Modal
          open={pendingDelete !== null}
          onClose={() => setPendingDelete(null)}
          title="Delete department"
          description={pendingDelete?.name}
        >
          <div className="space-y-4">
            <div className="flex items-start gap-3 rounded-xl bg-muted p-3 text-sm">
              <TriangleAlert className="mt-0.5 h-4 w-4 shrink-0 text-destructive" aria-hidden />
              <p className="text-muted-foreground">
                This department has no courses, so nothing else is affected. If you may want it
                back later, retire it instead by unticking Active.
              </p>
            </div>
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setPendingDelete(null)}>
                Cancel
              </Button>
              <Button
                variant="destructive"
                onClick={confirmDelete}
                isLoading={deleteDepartment.isPending}
              >
                Delete
              </Button>
            </div>
          </div>
        </Modal>
      </div>
    </PageTransition>
  );
}
