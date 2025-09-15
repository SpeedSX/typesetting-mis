import { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Provider } from 'react-redux';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { store } from './store';
import { useAppDispatch, useAppSelector } from './hooks/redux';
import { getCurrentUser } from './store/slices/authSlice';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import CustomersPage from './pages/CustomersPage';
import CompaniesPage from './pages/CompaniesPage';
import UsersPage from './pages/UsersPage';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import RoleBasedDashboard from './components/RoleBasedDashboard';

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

function AppContent() {
  const dispatch = useAppDispatch();
  const { isAuthenticated, isCheckingAuth } = useAppSelector((state) => state.auth);

  useEffect(() => {
    // Check if user is logged in on app start
    const token = localStorage.getItem('authToken');
    if (token && !isAuthenticated) {
      dispatch(getCurrentUser());
    }
  }, [dispatch, isAuthenticated]);

  // The ProtectedRoute component will handle the loading state for auth checking

  return (
    <Router>
      <Routes>
        <Route path="/login" element={!isAuthenticated && !isCheckingAuth ? <LoginPage /> : <Navigate to="/" />} />
        <Route path="/register" element={!isAuthenticated && !isCheckingAuth ? <RegisterPage /> : <Navigate to="/" />} />
        <Route
          path="/*"
          element={
            <ProtectedRoute>
              <Layout>
                <Routes>
                  <Route path="/" element={<RoleBasedDashboard />} />
                  <Route path="/customers" element={<CustomersPage />} />
                  <Route path="/companies" element={<CompaniesPage />} />
                  <Route path="/users" element={<UsersPage />} />
                </Routes>
              </Layout>
            </ProtectedRoute>
          }
        />
      </Routes>
    </Router>
  );
}

function App() {
  return (
    <Provider store={store}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <AppContent />
      </ThemeProvider>
    </Provider>
  );
}

export default App;