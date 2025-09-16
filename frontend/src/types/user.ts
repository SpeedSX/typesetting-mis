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