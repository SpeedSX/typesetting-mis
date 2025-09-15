import React, { useEffect, useState } from 'react';
import {
  Container,
  Typography,
  Box,
  Button,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
} from '@mui/material';
import { Add, Edit, Delete, Share } from '@mui/icons-material';
import { useAppDispatch, useAppSelector } from '../hooks/redux';
import { fetchCompanies } from '../store/slices/companySlice';
import InvitationGenerator from '../components/InvitationGenerator';
import type { Company } from '../types/company';
import AddCompanyForm from '../components/AddCompanyForm';
import EditCompanyForm from '../components/EditCompanyForm';

const CompaniesPage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { companies, isLoading } = useAppSelector((state) => state.company);
  const [invitationDialog, setInvitationDialog] = useState<{ open: boolean; company: Company | null }>({
    open: false,
    company: null
  });
  const [addCompanyDialog, setAddCompanyDialog] = useState(false);
  const [editCompanyDialog, setEditCompanyDialog] = useState<{ open: boolean; company: Company | null }>({
    open: false,
    company: null
  });

  useEffect(() => {
    dispatch(fetchCompanies());
  }, [dispatch]);

  const handleOpenInvitation = (company: Company) => {
    setInvitationDialog({ open: true, company });
  };

  const handleCloseInvitation = () => {
    setInvitationDialog({ open: false, company: null });
  };

  const handleOpenAddCompany = () => {
    setAddCompanyDialog(true);
  };

  const handleCloseAddCompany = () => {
    setAddCompanyDialog(false);
  };

  const handleOpenEditCompany = (company: Company) => {
    setEditCompanyDialog({ open: true, company });
  };

  const handleCloseEditCompany = () => {
    setEditCompanyDialog({ open: false, company: null });
  };

  return (
    <Container maxWidth="lg">
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h4" component="h1">
          Companies
        </Typography>
        <Button variant="contained" startIcon={<Add />} onClick={handleOpenAddCompany}>
          Add Company
        </Button>
      </Box>

      {/* Companies Table */}
      <Paper>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Company Name</TableCell>
                <TableCell>Domain</TableCell>
                <TableCell>Subscription Plan</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Created</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {isLoading ? (
                <TableRow>
                  <TableCell colSpan={6} align="center">
                    <Typography>Loading...</Typography>
                  </TableCell>
                </TableRow>
              ) : companies.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} align="center">
                    <Typography variant="h6" color="text.secondary" gutterBottom>
                      No companies found
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Get started by adding your first company.
                    </Typography>
                  </TableCell>
                </TableRow>
              ) : (
                companies.map((company) => (
                  <TableRow key={company.id}>
                    <TableCell>
                      <Typography variant="subtitle2" fontWeight="medium">
                        {company.name}
                      </Typography>
                    </TableCell>
                    <TableCell>{company.domain}</TableCell>
                    <TableCell>
                      <Chip 
                        label={company.subscriptionPlan} 
                        size="small" 
                        color="primary" 
                        variant="outlined" 
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={company.isActive ? 'Active' : 'Inactive'}
                        color={company.isActive ? 'success' : 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      {new Date(company.createdAt).toLocaleDateString()}
                    </TableCell>
                    <TableCell>
                      <IconButton 
                        size="small" 
                        color="info" 
                        onClick={() => handleOpenInvitation(company)}
                        title="Generate Invitation Link"
                        aria-label="Generate Invitation Link"
                      >
                        <Share />
                      </IconButton>
                      <IconButton 
                        size="small" 
                        color="primary"
                        onClick={() => handleOpenEditCompany(company)}
                        title="Edit Company"
                        aria-label="Edit Company"
                      >
                        <Edit />
                      </IconButton>
                      <IconButton 
                        size="small" 
                        color="error"
                        title="Delete Company"
                        aria-label="Delete Company"
                      >
                        <Delete />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

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

      {/* Add Company Dialog */}
      <AddCompanyForm 
        open={addCompanyDialog} 
        onClose={handleCloseAddCompany} 
      />

      {/* Edit Company Dialog */}
      <EditCompanyForm 
        open={editCompanyDialog.open} 
        onClose={handleCloseEditCompany}
        company={editCompanyDialog.company}
      />
    </Container>
  );
};

export default CompaniesPage;
