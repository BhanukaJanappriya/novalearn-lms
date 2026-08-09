/** Mirrors the backend `DepartmentDto`. */
export interface Department {
  id: string;
  name: string;
  code: string;
  description: string | null;
  headId: string | null;
  headName: string | null;
  isActive: boolean;
  /** How many courses sit under this department. */
  courseCount: number;
  createdAtUtc: string;
}

export interface DepartmentInput {
  name: string;
  code: string;
  description: string | null;
  headId: string | null;
  isActive: boolean;
}
