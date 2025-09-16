export interface UserStats {
    totalUsers: number;
    activeUsers: number;
    inactiveUsers: number;
    usersByCompany: {
      companyName: string;
      count: number;
    }[];
  }