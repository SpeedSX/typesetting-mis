import axios, { type AxiosInstance, type AxiosResponse } from 'axios';
import type { AuthResponse, LoginRequest, RegisterRequest } from '../types/auth';
import type { Company, CreateCompanyRequest, UpdateCompanyRequest } from '../types/company';
import type { Customer, CreateCustomerRequest, UpdateCustomerRequest } from '../types/customer';

class ApiService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: 'http://localhost:5030/api',
      headers: {
        'Content-Type': 'application/json',
      },
      withCredentials: true, // Enable cookies for httpOnly refresh tokens
    });

    // Add request interceptor to include auth token
    this.api.interceptors.request.use((config) => {
      const token = localStorage.getItem('authToken');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Add response interceptor for error handling
    this.api.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Token expired or invalid, redirect to login
          localStorage.removeItem('authToken');
          localStorage.removeItem('user');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // Auth endpoints
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/login', credentials);
    return response.data;
  }

  async register(userData: RegisterRequest): Promise<AuthResponse> {
    // Convert companyId string to GUID format for the backend
    const registerData = {
      ...userData,
      companyId: userData.companyId // The backend expects a GUID string
    };
    const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/register', registerData);
    return response.data;
  }

  async getCurrentUser(): Promise<any> {
    const response: AxiosResponse<any> = await this.api.get('/auth/me');
    return response.data;
  }


  async logout(): Promise<void> {
    // Refresh token will be read from httpOnly cookie on the backend
    await this.api.post('/auth/logout');
  }

  async refreshToken(): Promise<AuthResponse> {
    // Refresh token will be read from httpOnly cookie on the backend
    const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/refresh');
    return response.data;
  }

  // Admin User endpoints
  async getUsers(): Promise<any[]> {
    const response: AxiosResponse<any[]> = await this.api.get('/admin/users');
    return response.data;
  }

  async getUserStats(): Promise<any> {
    const response: AxiosResponse<any> = await this.api.get('/admin/users/stats');
    return response.data;
  }

  // Health check
  async healthCheck(): Promise<any> {
    const response: AxiosResponse<any> = await this.api.get('/health');
    return response.data;
  }

  // Admin Company endpoints
  async getCompanies(): Promise<Company[]> {
    const response: AxiosResponse<Company[]> = await this.api.get('/admin/companies');
    return response.data;
  }

  async getCompany(id: string): Promise<Company> {
    const response: AxiosResponse<Company> = await this.api.get(`/admin/companies/${id}`);
    return response.data;
  }

  async createCompany(company: CreateCompanyRequest): Promise<Company> {
    const response: AxiosResponse<Company> = await this.api.post('/admin/companies', company);
    return response.data;
  }

  async updateCompany(id: string, company: UpdateCompanyRequest): Promise<void> {
    await this.api.put(`/admin/companies/${id}`, company);
  }

  async deleteCompany(id: string): Promise<void> {
    await this.api.delete(`/admin/companies/${id}`);
  }


  // User Customer endpoints
  async getCustomers(): Promise<Customer[]> {
    const response: AxiosResponse<Customer[]> = await this.api.get('/user/customers');
    return response.data;
  }

  async getCustomer(id: string): Promise<Customer> {
    const response: AxiosResponse<Customer> = await this.api.get(`/user/customers/${id}`);
    return response.data;
  }

  async createCustomer(customer: CreateCustomerRequest): Promise<Customer> {
    const response: AxiosResponse<Customer> = await this.api.post('/user/customers', customer);
    return response.data;
  }

  async updateCustomer(id: string, customer: UpdateCustomerRequest): Promise<void> {
    await this.api.put(`/user/customers/${id}`, customer);
  }

  async deleteCustomer(id: string): Promise<void> {
    await this.api.delete(`/user/customers/${id}`);
  }
}

export const apiService = new ApiService();
