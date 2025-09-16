export type ISODateString = string; // ISO-8601 timestamp from backend (UTC)

export interface Invitation {
  id: string;
  token: string;
  companyId: string;
  companyName: string;
  expiresAt: ISODateString;
  isUsed: boolean;
  usedAt: ISODateString | null;
  usedByUserId: string | null;
  usedByEmail: string | null;
}

export interface CreateInvitationRequest {
  companyId: string;
  expirationHours?: number;
}

export interface ValidateInvitationRequest {
  token: string;
}
