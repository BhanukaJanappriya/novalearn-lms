import axios, {
  AxiosError,
  type AxiosRequestConfig,
  type InternalAxiosRequestConfig,
} from "axios";
import { env } from "@/config/env";

/**
 * Access token lives in memory only (never localStorage) to reduce XSS exposure.
 * The refresh token is an httpOnly cookie the browser sends automatically.
 */
let accessToken: string | null = null;

export const tokenStore = {
  get: () => accessToken,
  set: (token: string | null) => {
    accessToken = token;
  },
};

export const apiClient = axios.create({
  baseURL: env.apiBaseUrl,
  withCredentials: true,
  headers: { "Content-Type": "application/json" },
});

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});

// Single in-flight refresh shared by all queued 401s.
let refreshPromise: Promise<string | null> | null = null;

async function requestRefresh(): Promise<string | null> {
  try {
    const { data } = await axios.post<{ accessToken: string }>(
      `${env.apiBaseUrl}/auth/refresh`,
      {},
      { withCredentials: true },
    );
    tokenStore.set(data.accessToken);
    return data.accessToken;
  } catch {
    tokenStore.set(null);
    return null;
  }
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as (AxiosRequestConfig & { _retry?: boolean }) | undefined;
    const url = original?.url ?? "";
    const isAuthRoute = url.includes("/auth/login") || url.includes("/auth/refresh");

    if (error.response?.status === 401 && original && !original._retry && !isAuthRoute) {
      original._retry = true;
      refreshPromise ??= requestRefresh().finally(() => {
        refreshPromise = null;
      });

      const newToken = await refreshPromise;
      if (newToken) {
        // The request interceptor re-injects the refreshed token on retry.
        return apiClient(original);
      }
    }

    return Promise.reject(error);
  },
);
