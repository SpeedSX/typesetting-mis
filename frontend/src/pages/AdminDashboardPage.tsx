import React, { useEffect } from 'react';
import {
  Container,
  Typography,
  Card,
  CardContent,
  Box,
  Paper,
  Button,
} from '@mui/material';
import {
  Business,
  People,
  Settings,
  BarChart,
  Add,
  Edit,
  Delete,
} from '@mui/icons-material';
import { useAppDispatch, useAppSelector } from '../hooks/redux';
import { fetchCompanies } from '../store/slices/companySlice';
import { fetchUserStats } from '../store/slices/userSlice';
import { useNavigate } from 'react-router-dom';

const AdminDashboardPage: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const { companies } = useAppSelector((state) => state.company);
  const { stats: userStats } = useAppSelector((state) => state.user);

  useEffect(() => {
    dispatch(fetchCompanies());
    dispatch(fetchUserStats());
  }, [dispatch]);

  const adminStats = [
    {
      title: 'Total Companies',
      value: companies.length,
      icon: <Business />,
      color: '#1976d2',
      action: 'Manage Companies',
    },
    {
      title: 'Total Users',
      value: userStats?.totalUsers || 'Loading...',
      icon: <People />,
      color: '#dc004e',
      action: 'Manage Users',
    },
    {
      title: 'System Health',
      value: 'Healthy',
      icon: <Settings />,
      color: '#2e7d32',
      action: 'System Settings',
    },
    {
      title: 'Reports',
      value: 'Available',
      icon: <BarChart />,
      color: '#ed6c02',
      action: 'View Reports',
    },
  ];

  const handleActionClick = (action: string) => {
    switch (action) {
      case 'Manage Companies':
        navigate('/companies');
        break;
      case 'Manage Users':
        navigate('/users');
        break;
      case 'Add Company':
        navigate('/companies');
        break;
      case 'System Settings':
        // TODO: Implement settings page
        console.log('Settings clicked');
        break;
      case 'View Reports':
        // TODO: Implement reports page
        console.log('Reports clicked');
        break;
      default:
        break;
    }
  };

  const quickActions = [
    { title: 'Add Company', icon: <Add />, color: '#1976d2' },
    { title: 'Manage Users', icon: <People />, color: '#dc004e' },
    { title: 'System Settings', icon: <Settings />, color: '#2e7d32' },
    { title: 'View Reports', icon: <BarChart />, color: '#ed6c02' },
  ];

  return (
    <Container maxWidth="lg">
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Admin Dashboard
        </Typography>
        <Typography variant="subtitle1" color="text.secondary">
          Welcome back, {user?.firstName}! Manage your system and users.
        </Typography>
      </Box>

      {/* Admin Stats */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h6" gutterBottom>
          System Overview
        </Typography>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: 3 }}>
          {adminStats.map((stat) => (
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
                <Button
                  size="small"
                  sx={{ mt: 1, textTransform: 'none' }}
                  color="primary"
                  onClick={() => handleActionClick(stat.action)}
                >
                  {stat.action}
                </Button>
              </CardContent>
            </Card>
          ))}
        </Box>
      </Box>

      {/* Quick Actions */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h6" gutterBottom>
          Quick Actions
        </Typography>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 2 }}>
          {quickActions.map((action) => (
            <Card 
              key={action.title} 
              sx={{ cursor: 'pointer', '&:hover': { boxShadow: 3 } }}
              onClick={() => handleActionClick(action.title)}
            >
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <Box
                    sx={{
                      p: 1,
                      borderRadius: 1,
                      backgroundColor: `${action.color}20`,
                      color: action.color,
                      mr: 2,
                    }}
                  >
                    {action.icon}
                  </Box>
                  <Typography variant="subtitle1">
                    {action.title}
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          ))}
        </Box>
      </Box>

      {/* Recent Companies */}
      <Box sx={{ mb: 4 }}>
        <Paper sx={{ p: 3 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6">
              Recent Companies
            </Typography>
            <Button variant="outlined" startIcon={<Add />}>
              Add Company
            </Button>
          </Box>
          {companies.length > 0 ? (
            <Box>
              {companies.slice(0, 5).map((company) => (
                <Box key={company.id} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 1, borderBottom: '1px solid #eee' }}>
                  <Box>
                    <Typography variant="subtitle1">{company.name}</Typography>
                    <Typography variant="body2" color="text.secondary">
                      {company.domain} • {company.subscriptionPlan}
                    </Typography>
                  </Box>
                  <Box>
                    <Button size="small" startIcon={<Edit />} sx={{ mr: 1 }}>
                      Edit
                    </Button>
                    <Button size="small" color="error" startIcon={<Delete />}>
                      Delete
                    </Button>
                  </Box>
                </Box>
              ))}
            </Box>
          ) : (
            <Typography variant="body2" color="text.secondary">
              No companies found.
            </Typography>
          )}
        </Paper>
      </Box>

      {/* System Status */}
      <Box>
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            System Status
          </Typography>
          <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Box sx={{ width: 8, height: 8, borderRadius: '50%', backgroundColor: '#2e7d32' }} />
              <Typography variant="body2">Database: Connected</Typography>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Box sx={{ width: 8, height: 8, borderRadius: '50%', backgroundColor: '#2e7d32' }} />
              <Typography variant="body2">API: Running</Typography>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Box sx={{ width: 8, height: 8, borderRadius: '50%', backgroundColor: '#2e7d32' }} />
              <Typography variant="body2">Frontend: Active</Typography>
            </Box>
          </Box>
        </Paper>
      </Box>
    </Container>
  );
};

export default AdminDashboardPage;
