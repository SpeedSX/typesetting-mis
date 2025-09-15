import React from 'react';
import { useAppSelector } from '../hooks/redux';
import DashboardPage from '../pages/DashboardPage';
import AdminDashboardPage from '../pages/AdminDashboardPage';

const RoleBasedDashboard: React.FC = () => {
  const { user } = useAppSelector((state) => state.auth);

  // Check if user is admin
  const isAdmin = user?.roleName?.toLowerCase() === 'admin';

  if (isAdmin) {
    return <AdminDashboardPage />;
  }

  return <DashboardPage />;
};

export default RoleBasedDashboard;
