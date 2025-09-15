import React, { useEffect, useState } from 'react';
import {
  Container,
  Typography,
  Box,
  Button,
  Card,
  CardContent,
  Grid,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
} from '@mui/material';
import { Add, Edit, Delete, Share } from '@mui/icons-material';
import { useAppDispatch, useAppSelector } from '../hooks/redux';
import { fetchCompanies } from '../store/slices/companySlice';
import InvitationGenerator from '../components/InvitationGenerator';

const CompaniesPage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { companies, isLoading } = useAppSelector((state) => state.company);
  const [invitationDialog, setInvitationDialog] = useState<{ open: boolean; company: any }>({
    open: false,
    company: null
  });

  useEffect(() => {
    dispatch(fetchCompanies());
  }, [dispatch]);

  const handleOpenInvitation = (company: any) => {
    setInvitationDialog({ open: true, company });
  };

  const handleCloseInvitation = () => {
    setInvitationDialog({ open: false, company: null });
  };

  return (
    <Container maxWidth="lg">
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h4" component="h1">
          Companies
        </Typography>
        <Button variant="contained" startIcon={<Add />}>
          Add Company
        </Button>
      </Box>

      {isLoading ? (
        <Typography>Loading...</Typography>
      ) : (
        <Grid container spacing={3}>
          {companies.map((company) => (
            <Grid item xs={12} sm={6} md={4} key={company.id}>
              <Card>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                    <Typography variant="h6" component="div">
                      {company.name}
                    </Typography>
                    <Box>
                      <IconButton 
                        size="small" 
                        color="info" 
                        onClick={() => handleOpenInvitation(company)}
                        title="Generate Invitation Link"
                      >
                        <Share />
                      </IconButton>
                      <IconButton size="small" color="primary">
                        <Edit />
                      </IconButton>
                      <IconButton size="small" color="error">
                        <Delete />
                      </IconButton>
                    </Box>
                  </Box>
                  
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    {company.domain}
                  </Typography>
                  
                  <Box sx={{ display: 'flex', gap: 1, mb: 2 }}>
                    <Chip 
                      label={company.subscriptionPlan} 
                      size="small" 
                      color="primary" 
                      variant="outlined" 
                    />
                    <Chip 
                      label={company.isActive ? 'Active' : 'Inactive'} 
                      size="small" 
                      color={company.isActive ? 'success' : 'default'} 
                    />
                  </Box>
                  
                  <Typography variant="body2" color="text.secondary">
                    Created: {new Date(company.createdAt).toLocaleDateString()}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      {companies.length === 0 && !isLoading && (
        <Box sx={{ textAlign: 'center', py: 4 }}>
          <Typography variant="h6" color="text.secondary" gutterBottom>
            No companies found
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Get started by adding your first company.
          </Typography>
        </Box>
      )}

      {/* Invitation Dialog */}
      <Dialog 
        open={invitationDialog.open} 
        onClose={handleCloseInvitation}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          Generate Invitation Link
        </DialogTitle>
        <DialogContent>
          {invitationDialog.company && (
            <InvitationGenerator 
              companyId={invitationDialog.company.id}
              companyName={invitationDialog.company.name}
            />
          )}
        </DialogContent>
      </Dialog>
    </Container>
  );
};

export default CompaniesPage;
