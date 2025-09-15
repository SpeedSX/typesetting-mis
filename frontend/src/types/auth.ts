export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  companyId: string; // We'll convert this to Guid in the API call
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  companyId: string;
  companyName: string;
  roleId: string;
  roleName: string;
  isActive: boolean;
  createdAt: string;
}
