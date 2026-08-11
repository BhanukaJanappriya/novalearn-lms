import { useState } from "react";
import { GraduationCap, Search, ShieldCheck, Users } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { LinkButton } from "@/components/ui/link-button";
import { Skeleton } from "@/components/ui/skeleton";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { getApiErrorMessage } from "@/lib/apiError";
import { useDirectory, type DirectoryAudience } from "../api/queries";
import { DirectoryGrid } from "../components/DirectoryGrid";

interface DirectoryPageProps {
  audience: DirectoryAudience;
}

const copy = {
  students: {
    title: "Students",
    icon: GraduationCap,
    blurb: "Everyone learning on the platform, with how far they have got.",
    empty: "No students match that search.",
  },
  lecturers: {
    title: "Lecturers",
    icon: Users,
    blurb: "Teaching staff, with what they run and who they teach.",
    empty: "No teaching staff match that search.",
  },
} as const;

/**
 * The people directory. Read only: it answers who is registered and how they are doing, and
 * carries nothing that could be used to act on an account. Anyone who needs to change a role or
 * suspend somebody goes to Users, which is linked from here.
 */
export function DirectoryPage({ audience }: DirectoryPageProps) {
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search, 300);
  const { data: people, isLoading, isError, error } = useDirectory(audience, debouncedSearch);

  const { title, icon: Icon, blurb, empty } = copy[audience];

  return (
    <PageTransition>
      <div className="space-y-6">
        <header className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
              <Icon className="h-6 w-6 text-primary" aria-hidden />
              {title}
            </h1>
            <p className="mt-1 text-muted-foreground">{blurb}</p>
          </div>
          <LinkButton to="/admin/users" variant="outline" size="sm">
            <ShieldCheck className="h-4 w-4" />
            Manage accounts
          </LinkButton>
        </header>

        <div className="flex flex-wrap items-center gap-3">
          <div className="relative min-w-[240px] flex-1">
            <Search
              className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
              aria-hidden
            />
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder={`Search ${title.toLowerCase()} by name or email`}
              aria-label={`Search ${title.toLowerCase()}`}
              className="pl-9"
            />
          </div>
          {people && (
            <Badge variant="neutral">
              {people.length} {people.length === 1 ? "person" : "people"}
            </Badge>
          )}
        </div>

        {isError && (
          <Alert variant="error">
            {getApiErrorMessage(error, `We could not load ${title.toLowerCase()}.`)}
          </Alert>
        )}

        {isLoading ? (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }).map((_, index) => (
              <Skeleton key={index} className="h-52 rounded-[18px]" />
            ))}
          </div>
        ) : people && people.length === 0 ? (
          <div className="rounded-[18px] border border-dashed border-border py-16 text-center">
            <p className="font-medium">{empty}</p>
            <p className="mt-1 text-sm text-muted-foreground">
              Try a different name, or clear the search.
            </p>
          </div>
        ) : people ? (
          <>
            <DirectoryGrid people={people} />
            <p className="text-xs text-muted-foreground">
              This is a read-only view. It shows profile and progress information only, never
              account security details or individual marks.
            </p>
          </>
        ) : null}
      </div>
    </PageTransition>
  );
}
