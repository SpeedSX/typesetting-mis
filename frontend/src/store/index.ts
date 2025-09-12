import { configureStore } from '@reduxjs/toolkit';
import authSlice from './slices/authSlice';
import companySlice from './slices/companySlice';
import customerSlice from './slices/customerSlice';

export const store = configureStore({
  reducer: {
    auth: authSlice,
    company: companySlice,
    customer: customerSlice,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
