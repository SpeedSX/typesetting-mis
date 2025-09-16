export interface Invitation {
  id: string;
  token: string;
  companyId: string;
  companyName: string;
  expiresAt: string;
  isUsed: boolean;
  usedAt: string | null;
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
