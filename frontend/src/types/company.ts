export interface Company {
  id: string;
  name: string;
  domain: string;
  settings: CompanySettings;
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
  settings: CompanySettings;
  subscriptionPlan: string;
}

export interface UpdateCompanyRequest {
  name?: string;
  settings?: CompanySettings;
  subscriptionPlan?: string;
  isActive?: boolean;
}
