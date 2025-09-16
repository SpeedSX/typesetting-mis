export interface UserStats {
    totalUsers: number;
    activeUsers: number;
    inactiveUsers: number;
    usersByCompany: Array<{
      companyName: string;
      count: number;
    }>;
  }