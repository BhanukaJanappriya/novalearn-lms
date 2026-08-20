import { BrowserRouter } from "react-router-dom";
import { MotionConfig } from "framer-motion";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { CursorField } from "@/components/CursorField";
import { DocumentTitle } from "@/components/DocumentTitle";
import { MaintenanceBanner } from "@/components/MaintenanceBanner";
import { AuthProvider } from "@/context/AuthProvider";
import { NotificationsProvider } from "@/features/notifications/NotificationsProvider";
import { AppRoutes } from "@/routes/AppRoutes";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: 1, refetchOnWindowFocus: false },
    mutations: { retry: 0 },
  },
});

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      {/*
        One switch for the whole product: with reducedMotion="user", every framer-motion
        animation collapses to an instant state change when the operating system asks for
        reduced motion. Doing it here means no individual component has to remember.
      */}
      <MotionConfig reducedMotion="user">
        {/* Mounted once, outside the router, so it survives route changes rather than
            remounting (and losing its trailing position) on every navigation. */}
        <CursorField />
        <DocumentTitle />
        <BrowserRouter>
          <MaintenanceBanner />
          <AuthProvider>
            <NotificationsProvider>
              <AppRoutes />
            </NotificationsProvider>
          </AuthProvider>
        </BrowserRouter>
      </MotionConfig>
    </QueryClientProvider>
  );
}
