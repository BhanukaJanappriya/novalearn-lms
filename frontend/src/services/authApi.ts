import { apiClient, tokenStore } from "./apiClient";
import type {
  AuthenticationResponse,
  LoginPayload,
  RegisterPayload,
  RegisterResponse,
  UserSummary,
} from "@/types/auth";

export const authApi = {
  async register(payload: RegisterPayload): Promise<RegisterResponse> {
    const { data } = await apiClient.post<RegisterResponse>("/auth/register", payload);
    return data;
  },

  async login(payload: LoginPayload): Promise<AuthenticationResponse> {
    const { data } = await apiClient.post<AuthenticationResponse>("/auth/login", payload);
    tokenStore.set(data.accessToken);
    return data;
  },

  async refresh(): Promise<AuthenticationResponse | null> {
    try {
      const { data } = await apiClient.post<AuthenticationResponse>("/auth/refresh", {});
      tokenStore.set(data.accessToken);
      return data;
    } catch {
      tokenStore.set(null);
      return null;
    }
  },

  async me(): Promise<UserSummary> {
    const { data } = await apiClient.get<UserSummary>("/auth/me");
    return data;
  },

  async verifyEmail(userId: string, token: string): Promise<void> {
    await apiClient.post("/auth/verify-email", { userId, token });
  },

  async logout(): Promise<void> {
    await apiClient.post("/auth/logout", {});
    tokenStore.set(null);
  },
};
