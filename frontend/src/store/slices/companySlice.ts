import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import { apiService } from '../../services/api';
import type { Company, CreateCompanyRequest, UpdateCompanyRequest } from '../../types/company';

interface CompanyState {
  companies: Company[];
  currentCompany: Company | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: CompanyState = {
  companies: [],
  currentCompany: null,
  isLoading: false,
  error: null,
};

// Async thunks
export const fetchCompanies = createAsyncThunk(
  'company/fetchCompanies',
  async (_, { rejectWithValue }) => {
    try {
      const companies = await apiService.getCompanies();
      return companies;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch companies');
    }
  }
);

export const fetchCompany = createAsyncThunk(
  'company/fetchCompany',
  async (id: string, { rejectWithValue }) => {
    try {
      const company = await apiService.getCompany(id);
      return company;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch company');
    }
  }
);

export const createCompany = createAsyncThunk(
  'company/createCompany',
  async (companyData: CreateCompanyRequest, { rejectWithValue }) => {
    try {
      const company = await apiService.createCompany(companyData);
      return company;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to create company');
    }
  }
);

export const updateCompany = createAsyncThunk(
  'company/updateCompany',
  async ({ id, companyData }: { id: string; companyData: UpdateCompanyRequest }, { rejectWithValue }) => {
    try {
      await apiService.updateCompany(id, companyData);
      return { id, companyData };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to update company');
    }
  }
);

export const deleteCompany = createAsyncThunk(
  'company/deleteCompany',
  async (id: string, { rejectWithValue }) => {
    try {
      await apiService.deleteCompany(id);
      return id;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to delete company');
    }
  }
);

const companySlice = createSlice({
  name: 'company',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    setCurrentCompany: (state, action: PayloadAction<Company>) => {
      state.currentCompany = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch companies
      .addCase(fetchCompanies.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchCompanies.fulfilled, (state, action: PayloadAction<Company[]>) => {
        state.isLoading = false;
        state.companies = action.payload;
        state.error = null;
      })
      .addCase(fetchCompanies.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Fetch company
      .addCase(fetchCompany.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchCompany.fulfilled, (state, action: PayloadAction<Company>) => {
        state.isLoading = false;
        state.currentCompany = action.payload;
        state.error = null;
      })
      .addCase(fetchCompany.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Create company
      .addCase(createCompany.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(createCompany.fulfilled, (state, action: PayloadAction<Company>) => {
        state.isLoading = false;
        state.companies.push(action.payload);
        state.error = null;
      })
      .addCase(createCompany.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Update company
      .addCase(updateCompany.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(updateCompany.fulfilled, (state, action) => {
        state.isLoading = false;
        const index = state.companies.findIndex(company => company.id === action.payload.id);
        if (index !== -1) {
          state.companies[index] = { ...state.companies[index], ...action.payload.companyData };
        }
        if (state.currentCompany?.id === action.payload.id) {
          state.currentCompany = { ...state.currentCompany, ...action.payload.companyData };
        }
        state.error = null;
      })
      .addCase(updateCompany.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Delete company
      .addCase(deleteCompany.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(deleteCompany.fulfilled, (state, action) => {
        state.isLoading = false;
        state.companies = state.companies.filter(company => company.id !== action.payload);
        if (state.currentCompany?.id === action.payload) {
          state.currentCompany = null;
        }
        state.error = null;
      })
      .addCase(deleteCompany.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const { clearError, setCurrentCompany } = companySlice.actions;
export default companySlice.reducer;
