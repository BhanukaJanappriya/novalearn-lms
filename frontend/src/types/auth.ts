/** Shapes mirrored from the backend authentication contracts. */

export interface UserSummary {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
}

export interface AuthenticationResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
  tokenType: string;
  user: UserSummary;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  requiresEmailVerification: boolean;
}

export interface RegisterPayload {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface LoginPayload {
  email: string;
  password: string;
}

/** RFC 7807 problem details returned by the API on failure. */
export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}
