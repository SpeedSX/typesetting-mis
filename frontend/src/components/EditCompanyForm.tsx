import React, { useState, useEffect } from 'react';
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
import { updateCompany, clearError } from '../store/slices/companySlice';
import type { Company, UpdateCompanyRequest, CompanySettings } from '../types/company';
import { TIMEZONES, CURRENCIES, SUBSCRIPTION_PLANS } from '../constants/org';
import { DEFAULT_COMPANY_SETTINGS } from '../constants/companyDefaults';

const initialForm: Omit<UpdateCompanyRequest, 'settings'> = {
  name: '',
  subscriptionPlan: '',
  isActive: true,
};

const initialSettings: CompanySettings = { ...DEFAULT_COMPANY_SETTINGS };

interface EditCompanyFormProps {
  open: boolean;
  onClose: () => void;
  company: Company | null;
}

const EditCompanyForm: React.FC<EditCompanyFormProps> = ({ open, onClose, company }) => {
  const dispatch = useAppDispatch();
  const { isLoading, error } = useAppSelector((state) => state.company);

  const [formData, setFormData] = useState<Omit<UpdateCompanyRequest, 'settings'>>(initialForm);
  const [settings, setSettings] = useState<CompanySettings>(initialSettings);

  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});

  // Initialize form data when dialog opens and company changes
  useEffect(() => {
    if (open && company) {
      setFormData({
        name: company.name,
        subscriptionPlan: company.subscriptionPlan,
        isActive: company.isActive,
      });

      // Parse settings JSON string
      try {
        const parsedSettings = company.settings ? JSON.parse(company.settings) : initialSettings;
        setSettings(parsedSettings);
      } catch (error) {
        console.error('Error parsing settings:', error);
        setSettings({ ...initialSettings });
      }
    }
  }, [open, company]);

  const handleInputChange = (field: string, value: string | boolean) => {
    if (field.startsWith('settings.')) {
      const settingField = field.split('.')[1];
      const newSettings = {
        ...settings,
        [settingField]: value,
      };
      setSettings(newSettings);
      // keep only `settings` state; stringify on submit
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

    if (!formData.name?.trim()) {
      errors.name = 'Company name is required';
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

    if (!company || !validateForm()) {
      return;
    }

    try {
      const payload: UpdateCompanyRequest = { ...formData, settings: JSON.stringify(settings) };
      await dispatch(updateCompany({ id: company.id, companyData: payload })).unwrap();
      handleClose();
    } catch (error) {
      // Error is handled by Redux and displayed via the error state
    }
  };

  const handleClose = () => {
    setFormData({ ...initialForm });
    setSettings(() => ({ ...initialSettings }));
    setValidationErrors({});
    dispatch(clearError()); // Clear any stale errors
    onClose();
  };


  if (!company) {
    return null;
  }

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>Edit Company</DialogTitle>
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
              value={formData.name || ''}
              onChange={(e) => handleInputChange('name', e.target.value)}
              error={!!validationErrors.name}
              helperText={validationErrors.name}
              fullWidth
              required
            />

            <TextField
              label="Domain"
              value={company.domain}
              disabled
              fullWidth
              helperText="Domain cannot be changed"
            />

            <FormControl fullWidth error={!!validationErrors['settings.timezone']}>
              <InputLabel>Timezone</InputLabel>
              <Select
                value={settings.timezone}
                onChange={(e) => handleInputChange('settings.timezone', e.target.value)}
                label="Timezone"
                aria-label="Company timezone"
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
                aria-label="Company currency"
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
                value={formData.subscriptionPlan || ''}
                onChange={(e) => handleInputChange('subscriptionPlan', e.target.value)}
                label="Subscription Plan"
                aria-label="Company subscription plan"
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

            <FormControl fullWidth>
              <InputLabel>Status</InputLabel>
              <Select
                value={formData.isActive ? 'active' : 'inactive'}
                onChange={(e) => handleInputChange('isActive', e.target.value === 'active')}
                label="Status"
                aria-label="Company status"
              >
                <MenuItem value="active">Active</MenuItem>
                <MenuItem value="inactive">Inactive</MenuItem>
              </Select>
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
            {isLoading ? 'Updating...' : 'Update Company'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};

export default EditCompanyForm;
