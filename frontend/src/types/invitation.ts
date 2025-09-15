export interface Invitation {
  token: string;
  companyName: string;
  expiresAt: string;
}

export interface CreateInvitationRequest {
  companyId: string;
  expirationHours?: number;
}

export interface ValidateInvitationRequest {
  token: string;
}
