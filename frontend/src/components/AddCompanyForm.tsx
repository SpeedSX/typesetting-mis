import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Box,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
} from '@mui/material';
import { useAppDispatch, useAppSelector } from '../hooks/redux';
import { createCompany, clearError } from '../store/slices/companySlice';
import type { CreateCompanyRequest, CompanySettings } from '../types/company';
import { TIMEZONES, CURRENCIES, SUBSCRIPTION_PLANS } from '../constants/org';

interface AddCompanyFormProps {
  open: boolean;
  onClose: () => void;
}

const AddCompanyForm: React.FC<AddCompanyFormProps> = ({ open, onClose }) => {
  const dispatch = useAppDispatch();
  const { isLoading, error } = useAppSelector((state) => state.company);

  const [formData, setFormData] = useState<CreateCompanyRequest>({
    name: '',
    domain: '',
    settings: JSON.stringify({
      timezone: 'UTC',
      currency: 'USD',
    }),
    subscriptionPlan: 'Basic',
  });

  const [settings, setSettings] = useState<CompanySettings>({
    timezone: 'UTC',
    currency: 'USD',
  });

  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});

  const handleInputChange = (field: string, value: string) => {
    if (field.startsWith('settings.')) {
      const settingField = field.split('.')[1];
      const newSettings = {
        ...settings,
        [settingField]: value,
      };
      setSettings(newSettings);
      setFormData(prev => ({
        ...prev,
        settings: JSON.stringify(newSettings),
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        [field]: value,
      }));
    }

    // Clear validation error for this field
    if (validationErrors[field]) {
      setValidationErrors(prev => ({
        ...prev,
        [field]: '',
      }));
    }
  };

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};

    if (!formData.name.trim()) {
      errors.name = 'Company name is required';
    }

    if (!formData.domain.trim()) {
      errors.domain = 'Domain is required';
    } else if (!/^(?=.{1,253}$)(?:[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,}$/.test(formData.domain)) {
      errors.domain = 'Enter a valid domain like example.com (no protocol, at least one dot)';
    }

    if (!settings.timezone) {
      errors['settings.timezone'] = 'Timezone is required';
    }

    if (!settings.currency) {
      errors['settings.currency'] = 'Currency is required';
    }

    if (!formData.subscriptionPlan) {
      errors.subscriptionPlan = 'Subscription plan is required';
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      await dispatch(createCompany(formData)).unwrap();
      handleClose();
    } catch (error) {
      // Error is handled by Redux and displayed via the error state
    }
  };

  const handleClose = () => {
    const defaultSettings = {
      timezone: 'UTC',
      currency: 'USD',
    };
    setFormData({
      name: '',
      domain: '',
      settings: JSON.stringify(defaultSettings),
      subscriptionPlan: 'Basic',
    });
    setSettings(defaultSettings);
    setValidationErrors({});
    dispatch(clearError()); // Clear any stale errors
    onClose();
  };


  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>Add New Company</DialogTitle>
      <form onSubmit={handleSubmit}>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField
              label="Company Name"
              value={formData.name}
              onChange={(e) => handleInputChange('name', e.target.value)}
              error={!!validationErrors.name}
              helperText={validationErrors.name}
              fullWidth
              required
            />

            <TextField
              label="Domain"
              value={formData.domain}
              onChange={(e) => handleInputChange('domain', e.target.value)}
              error={!!validationErrors.domain}
              helperText={validationErrors.domain || 'e.g., example.com'}
              fullWidth
              required
            />

            <FormControl fullWidth error={!!validationErrors['settings.timezone']}>
              <InputLabel>Timezone</InputLabel>
              <Select
                value={settings.timezone}
                onChange={(e) => handleInputChange('settings.timezone', e.target.value)}
                label="Timezone"
              >
                {TIMEZONES.map((tz) => (
                  <MenuItem key={tz} value={tz}>
                    {tz}
                  </MenuItem>
                ))}
              </Select>
              {validationErrors['settings.timezone'] && (
                <Box sx={{ color: 'error.main', fontSize: '0.75rem', mt: 0.5 }}>
                  {validationErrors['settings.timezone']}
                </Box>
              )}
            </FormControl>

            <FormControl fullWidth error={!!validationErrors['settings.currency']}>
              <InputLabel>Currency</InputLabel>
              <Select
                value={settings.currency}
                onChange={(e) => handleInputChange('settings.currency', e.target.value)}
                label="Currency"
              >
                {CURRENCIES.map((currency) => (
                  <MenuItem key={currency} value={currency}>
                    {currency}
                  </MenuItem>
                ))}
              </Select>
              {validationErrors['settings.currency'] && (
                <Box sx={{ color: 'error.main', fontSize: '0.75rem', mt: 0.5 }}>
                  {validationErrors['settings.currency']}
                </Box>
              )}
            </FormControl>

            <FormControl fullWidth error={!!validationErrors.subscriptionPlan}>
              <InputLabel>Subscription Plan</InputLabel>
              <Select
                value={formData.subscriptionPlan}
                onChange={(e) => handleInputChange('subscriptionPlan', e.target.value)}
                label="Subscription Plan"
              >
                {SUBSCRIPTION_PLANS.map((plan) => (
                  <MenuItem key={plan} value={plan}>
                    {plan}
                  </MenuItem>
                ))}
              </Select>
              {validationErrors.subscriptionPlan && (
                <Box sx={{ color: 'error.main', fontSize: '0.75rem', mt: 0.5 }}>
                  {validationErrors.subscriptionPlan}
                </Box>
              )}
            </FormControl>
          </Box>
        </DialogContent>

        <DialogActions>
          <Button onClick={handleClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isLoading}
          >
            {isLoading ? 'Creating...' : 'Create Company'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};

export default AddCompanyForm;
