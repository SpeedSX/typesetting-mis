export interface Company {
  id: string;
  name: string;
  domain: string;
  settings: string; // JSON string as stored in backend
  subscriptionPlan: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CompanySettings {
  timezone: string;
  currency: string;
}

export interface CreateCompanyRequest {
  name: string;
  domain: string;
  settings: string; // JSON string to match backend expectation
  subscriptionPlan: string;
}

export interface UpdateCompanyRequest {
  name?: string;
  settings?: string; // JSON string to match backend expectation
  subscriptionPlan?: string;
  isActive?: boolean;
}
