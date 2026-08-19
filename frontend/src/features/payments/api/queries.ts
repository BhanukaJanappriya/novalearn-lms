import { useMutation } from "@tanstack/react-query";
import { apiClient } from "@/services/apiClient";

export interface CheckoutSessionResponse {
  checkoutUrl: string;
}

/**
 * Starts checkout for a priced course and returns Stripe's hosted payment URL.
 *
 * The caller navigates the whole page there with `window.location.href`, not a client-side
 * route: Checkout lives on Stripe's own domain, and enrolment is not created here at all — it is
 * created by the webhook once Stripe confirms payment, which is the only source of truth this
 * product trusts for "did the money actually move".
 */
export function useStartCheckout() {
  return useMutation({
    mutationFn: async (courseId: string) => {
      const { data } = await apiClient.post<CheckoutSessionResponse>("/payments/checkout-sessions", {
        courseId,
      });
      return data;
    },
  });
}
