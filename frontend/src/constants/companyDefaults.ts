import type { CompanySettings } from "../types/company";

export const DEFAULT_COMPANY_SETTINGS = { 
    timezone: 'UTC',
    currency: 'USD' 
}  as const satisfies Readonly<CompanySettings>;