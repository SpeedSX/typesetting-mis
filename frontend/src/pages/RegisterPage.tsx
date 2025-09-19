import React, { useState, useEffect } from 'react';
import {
  Container,
  Paper,
  TextField,
  Button,
  Typography,
  Box,
  Alert,
  Link,
  CircularProgress,
} from '@mui/material';
import { useNavigate, Link as RouterLink, useSearchParams } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks/redux';
import { register, clearError } from '../store/slices/authSlice';
import { apiService } from '../services/api';
import type { RegisterRequest } from '../types/auth';
import type { Invitation } from '../types/invitation';

const RegisterPage: React.FC = () => {
  const [formData, setFormData] = useState<RegisterRequest>({
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    invitationToken: '',
  });
  const [errors, setErrors] = useState<Partial<RegisterRequest>>({});
  const [invitation, setInvitation] = useState<Invitation | null>(null);
  const [isValidatingInvitation, setIsValidatingInvitation] = useState(false);
  const [invitationError, setInvitationError] = useState<string | null>(null);

  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { isLoading, error, isAuthenticated } = useAppSelector((state) => state.auth);
  const [searchParams] = useSearchParams();
  const invitationTokenFromUrl = searchParams.get('invite');

  useEffect(() => {
    if (isAuthenticated) {
      navigate('/');
    }
  }, [isAuthenticated, navigate]);

  useEffect(() => {
    const validateInvitation = async () => {
      if (invitationTokenFromUrl) {
        setIsValidatingInvitation(true);
        setInvitationError(null);
        
        try {
          const invitationData = await apiService.validateInvitation({
            token: invitationTokenFromUrl
          });
          setInvitation(invitationData);
          setFormData(prev => ({
            ...prev,
            invitationToken: invitationTokenFromUrl
          }));
        } catch (err: any) {
          setInvitationError(err.response?.data?.message || 'Invalid or expired invitation');
        } finally {
          setIsValidatingInvitation(false);
        }
      }
    };
  
    validateInvitation();
  
    // Move cleanup to the outer useEffect return
    return () => {
      dispatch(clearError());
    };
  }, [dispatch, invitationTokenFromUrl]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    // Clear error when user starts typing
    if (errors[name as keyof RegisterRequest]) {
      setErrors((prev) => ({
        ...prev,
        [name]: undefined,
      }));
    }
  };


  const validateForm = (): boolean => {
    const newErrors: Partial<RegisterRequest> = {};

    if (!formData.email) {
      newErrors.email = 'Email is required';
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.email = 'Email is invalid';
    }

    if (!formData.password) {
      newErrors.password = 'Password is required';
    } else if (formData.password.length < 6) {
      newErrors.password = 'Password must be at least 6 characters';
    }

    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your password';
    } else if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }

    if (!formData.firstName) {
      newErrors.firstName = 'First name is required';
    }

    if (!formData.lastName) {
      newErrors.lastName = 'Last name is required';
    }

    if (!formData.invitationToken) {
      newErrors.invitationToken = 'Valid invitation is required';
    }
        
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    dispatch(register(formData));
  };

  return (
    <Container component="main" maxWidth="sm">
      <Box
        sx={{
          marginTop: 8,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <Paper elevation={3} sx={{ padding: 4, width: '100%' }}>
          <Typography component="h1" variant="h4" align="center" gutterBottom>
            TypesettingMIS
          </Typography>
          <Typography component="h2" variant="h5" align="center" gutterBottom>
            Sign Up
          </Typography>

          {isValidatingInvitation && (
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 2 }}>
              <CircularProgress size={20} sx={{ mr: 1 }} />
              <Typography variant="body2">Validating invitation...</Typography>
            </Box>
          )}

          {invitationError && (
             <Alert severity="error" sx={{ mb: 2 }}>
               {invitationError}
             </Alert>
           )}

          {!invitation && !isValidatingInvitation && !invitationError && (
            <Alert severity="warning" sx={{ mb: 2 }}>
              A valid invitation link is required to sign up.
            </Alert>
          )}

          {invitation && (
            <Alert severity="success" sx={{ mb: 2 }}>
              You're invited to join <strong>{invitation.companyName}</strong>!
              <br />
              <Typography variant="caption" color="text.secondary">
                Invitation expires: {new Date(invitation.expiresAt).toLocaleString()}
              </Typography>
            </Alert>
          )}
          
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <Box 
            component="form" 
            onSubmit={handleSubmit} 
            sx={{ 
              mt: 1,
              opacity: isValidatingInvitation ? 0.6 : 1
            }}
          >
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                margin="normal"
                required
                fullWidth
                id="firstName"
                label="First Name"
                name="firstName"
                autoComplete="given-name"
                value={formData.firstName}
                onChange={handleChange}
                error={!!errors.firstName}
                helperText={errors.firstName}
              />
              <TextField
                margin="normal"
                required
                fullWidth
                id="lastName"
                label="Last Name"
                name="lastName"
                autoComplete="family-name"
                value={formData.lastName}
                onChange={handleChange}
                error={!!errors.lastName}
                helperText={errors.lastName}
              />
            </Box>
            <TextField
              margin="normal"
              required
              fullWidth
              id="email"
              label="Email Address"
              name="email"
              autoComplete="email"
              value={formData.email}
              onChange={handleChange}
              error={!!errors.email}
              helperText={errors.email}
            />
            <TextField
              margin="normal"
              required
              fullWidth
              name="password"
              label="Password"
              type="password"
              id="password"
              autoComplete="new-password"
              value={formData.password}
              onChange={handleChange}
              error={!!errors.password}
              helperText={errors.password}
            />
            <TextField
              margin="normal"
              required
              fullWidth
              name="confirmPassword"
              label="Confirm Password"
              type="password"
              id="confirmPassword"
              autoComplete="new-password"
              value={formData.confirmPassword}
              onChange={handleChange}
              error={!!errors.confirmPassword}
              helperText={errors.confirmPassword}
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
              disabled={isLoading || isValidatingInvitation || !!invitationError || !invitation}
            >
              {isLoading ? 'Creating Account...' : 'Sign Up'}
            </Button>
            <Box textAlign="center">
              <Link component={RouterLink} to="/login" variant="body2">
                Already have an account? Sign In
              </Link>
            </Box>
          </Box>
        </Paper>
      </Box>
    </Container>
  );
};

export default RegisterPage;
