import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/services/apiClient";
import type { TrendMetric } from "@/features/analytics/api/queries";

export type PaymentStatus =
  | "Pending"
  | "Succeeded"
  | "Failed"
  | "Expired"
  | "PartiallyRefunded"
  | "Refunded";

export interface RevenuePoint {
  date: string;
  revenue: number;
  transactions: number;
}

export interface CourseRevenueRow {
  courseId: string;
  courseTitle: string;
  transactions: number;
  grossRevenue: number;
  refundedAmount: number;
  netRevenue: number;
}

export interface FinanceHeadline {
  netRevenue: TrendMetric;
  transactions: TrendMetric;
  refundedAmount: TrendMetric;
  pendingCheckouts: number;
}

export interface FinanceOverview {
  window: { fromUtc: string; toUtc: string; days: number; granularity: "Day" | "Week" | "Month" };
  headline: FinanceHeadline;
  series: RevenuePoint[];
  courses: CourseRevenueRow[];
  currency: string;
}

export interface Transaction {
  id: string;
  studentId: string;
  studentName: string;
  studentEmail: string;
  courseId: string;
  courseTitle: string;
  amount: number;
  currency: string;
  status: PaymentStatus;
  refundedAmount: number | null;
  createdAtUtc: string;
  paidAtUtc: string | null;
  refundedAtUtc: string | null;
  failureReason: string | null;
  canRefund: boolean;
  refundableAmount: number;
}

export interface TransactionPage {
  items: Transaction[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface TransactionFilters {
  status?: PaymentStatus;
  search?: string;
  page: number;
  pageSize: number;
}

export const financeKeys = {
  all: ["finance"] as const,
  overview: (days: number) => [...financeKeys.all, "overview", days] as const,
  transactions: (filters: TransactionFilters) =>
    [...financeKeys.all, "transactions", filters] as const,
};

export function useFinanceOverview(days: number) {
  return useQuery({
    queryKey: financeKeys.overview(days),
    queryFn: async () => {
      const { data } = await apiClient.get<FinanceOverview>("/admin/finance/overview", {
        params: { days },
      });
      return data;
    },
    staleTime: 30_000,
  });
}

export function useTransactions(filters: TransactionFilters) {
  return useQuery({
    queryKey: financeKeys.transactions(filters),
    queryFn: async () => {
      const { data } = await apiClient.get<TransactionPage>("/admin/finance/transactions", {
        params: {
          status: filters.status || undefined,
          search: filters.search || undefined,
          page: filters.page,
          pageSize: filters.pageSize,
        },
      });
      return data;
    },
    staleTime: 15_000,
  });
}

export function useRefundPayment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ paymentId, amount }: { paymentId: string; amount?: number }) => {
      const { data } = await apiClient.post<Transaction>(
        `/admin/finance/transactions/${paymentId}/refund`,
        { amount: amount ?? null },
      );
      return data;
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: financeKeys.all }),
  });
}
