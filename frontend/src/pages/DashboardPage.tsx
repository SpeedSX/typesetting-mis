import React, { useEffect } from 'react';
import {
  Container,
  Typography,
  Card,
  CardContent,
  Box,
  Paper,
} from '@mui/material';
import {
  People,
  Business,
  Assignment,
  AttachMoney,
} from '@mui/icons-material';
import { useAppDispatch, useAppSelector } from '../hooks/redux';
import { fetchCustomers } from '../store/slices/customerSlice';
import { fetchCompanies } from '../store/slices/companySlice';

const DashboardPage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const { customers } = useAppSelector((state) => state.customer);
  const { companies } = useAppSelector((state) => state.company);

  useEffect(() => {
    dispatch(fetchCustomers());
    dispatch(fetchCompanies());
  }, [dispatch]);

  const stats = [
    {
      title: 'Customers',
      value: customers.length,
      icon: <People />,
      color: '#1976d2',
    },
    {
      title: 'Companies',
      value: companies.length,
      icon: <Business />,
      color: '#dc004e',
    },
    {
      title: 'Orders',
      value: 0,
      icon: <Assignment />,
      color: '#2e7d32',
    },
    {
      title: 'Revenue',
      value: '$0.00',
      icon: <AttachMoney />,
      color: '#ed6c02',
    },
  ];

  return (
    <Container maxWidth="lg">
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Welcome back, {user?.firstName}!
        </Typography>
        <Typography variant="subtitle1" color="text.secondary">
          {user?.companyName} • {user?.roleName}
        </Typography>
      </Box>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', md: 'repeat(4, 1fr)' }, gap: 3 }}>
        {stats.map((stat) => (
          <Card key={stat.title}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Box
                  sx={{
                    p: 1,
                    borderRadius: 1,
                    backgroundColor: `${stat.color}20`,
                    color: stat.color,
                    mr: 2,
                  }}
                >
                  {stat.icon}
                </Box>
                <Typography variant="h6" component="div">
                  {stat.title}
                </Typography>
              </Box>
              <Typography variant="h4" component="div" color="primary">
                {stat.value}
              </Typography>
            </CardContent>
          </Card>
        ))}
      </Box>

      <Box sx={{ mt: 4 }}>
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Recent Activity
          </Typography>
          <Typography variant="body2" color="text.secondary">
            No recent activity to display.
          </Typography>
        </Paper>
      </Box>
    </Container>
  );
};

export default DashboardPage;
