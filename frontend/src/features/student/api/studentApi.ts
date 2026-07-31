import { apiClient } from "@/services/apiClient";
import type { StudentDashboard } from "./types";

export const studentApi = {
  async dashboard(): Promise<StudentDashboard> {
    const { data } = await apiClient.get<StudentDashboard>("/student/dashboard");
    return data;
  },
};
