export type UsersByCompanyItem = Readonly<{
  companyName: string;
  count: number;
}>;

export interface UserStats {
    totalUsers: number;
    activeUsers: number;
    inactiveUsers: number;
    usersByCompany: UsersByCompanyItem[];
  }

  export interface AdminUserListItem {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    isActive: boolean;
    lastLogin: string | null;
    companyName: string;
    roleName: string;
  }