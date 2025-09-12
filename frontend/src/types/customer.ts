export interface Customer {
  id: string;
  name: string;
  email?: string;
  phone?: string;
  taxId?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCustomerRequest {
  name: string;
  email?: string;
  phone?: string;
  taxId?: string;
}

export interface UpdateCustomerRequest {
  name?: string;
  email?: string;
  phone?: string;
  taxId?: string;
}
