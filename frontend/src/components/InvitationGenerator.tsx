import React, { useState } from 'react';
import {
  Box,
  Button,
  TextField,
  Typography,
  Paper,
  Alert,
  IconButton,
  InputAdornment,
  Divider,
  CircularProgress,
} from '@mui/material';
import { ContentCopy, Share, Link as LinkIcon } from '@mui/icons-material';
import { apiService } from '../services/api';
import type { Invitation } from '../types/invitation';

interface InvitationGeneratorProps {
  companyId: string;
  companyName: string;
}

const InvitationGenerator: React.FC<InvitationGeneratorProps> = ({ companyId, companyName }) => {
  const [copied, setCopied] = useState(false);
  const [customMessage, setCustomMessage] = useState('');
  const [invitation, setInvitation] = useState<Invitation | null>(null);
  const [isGenerating, setIsGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const generateInvitation = async () => {
    setIsGenerating(true);
    setError(null);
    
    try {
      const newInvitation = await apiService.createInvitation({
        companyId,
        expirationHours: 24 // 24 hours default
      });
      setInvitation(newInvitation);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setError(msg || 'Failed to generate invitation');
    } finally {
      setIsGenerating(false);
    }
  };

  const baseUrl = window.location.origin;
  const invitationUrl = invitation 
    ? `${baseUrl}/register?invite=${encodeURIComponent(invitation.token)}`
    : `${baseUrl}/register`;
  
  const fullInvitationText = customMessage 
    ? `${customMessage}\n\nRegistration Link: ${invitationUrl}`
    : `You're invited to join ${companyName}!\n\nClick here to register: ${invitationUrl}`;

  const handleCopyUrl = async () => {
    try {
      await navigator.clipboard.writeText(invitationUrl);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      console.error('Failed to copy: ', err);
    }
  };

  const handleCopyFullText = async () => {
    try {
      await navigator.clipboard.writeText(fullInvitationText);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      console.error('Failed to copy: ', err);
    }
  };

  const handleShare = async () => {
    if (navigator.share) {
      try {
        await navigator.share({
          title: `Join ${companyName}`,
          text: customMessage || `You're invited to join ${companyName}!`,
          url: invitationUrl,
        });
      } catch (err) {
        console.error('Error sharing: ', err);
      }
    } else {
      // Fallback to copying
      handleCopyFullText();
    }
  };

  return (
    <Paper sx={{ p: 3, mb: 3 }}>
      <Typography variant="h6" gutterBottom>
        Generate Invitation Link
      </Typography>
      
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Create a secure invitation link for users to register and join <strong>{companyName}</strong>
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {!invitation && (
        <Box sx={{ mb: 2 }}>
          <Button
            variant="contained"
            onClick={generateInvitation}
            disabled={isGenerating}
            startIcon={isGenerating ? <CircularProgress size={20} /> : <LinkIcon />}
          >
            {isGenerating ? 'Generating...' : 'Generate Secure Invitation'}
          </Button>
        </Box>
      )}

      {invitation && (
        <>
          <Alert severity="success" sx={{ mb: 2 }}>
            Invitation generated successfully! Expires: {new Date(invitation.expiresAt).toLocaleString()}
          </Alert>

          <TextField
            fullWidth
            multiline
            rows={3}
            label="Custom Message (Optional)"
            value={customMessage}
            onChange={(e) => setCustomMessage(e.target.value)}
            placeholder="Add a custom message for the invitation..."
            sx={{ mb: 2 }}
          />

          <TextField
            fullWidth
            label="Invitation URL"
            value={invitationUrl}
            InputProps={{
              readOnly: true,
          endAdornment: (
            <InputAdornment position="end">
              <IconButton onClick={handleCopyUrl} edge="end" aria-label="Copy invitation URL">
                <ContentCopy />
              </IconButton>
            </InputAdornment>
          ),
        }}
        sx={{ mb: 2 }}
      />

      <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
        <Button
          variant="contained"
          startIcon={<ContentCopy />}
          onClick={handleCopyFullText}
          disabled={copied}
        >
          {copied ? 'Copied!' : 'Copy Full Text'}
        </Button>
        
        <Button
          variant="outlined"
          startIcon={<Share />}
          onClick={handleShare}
        >
          Share
        </Button>
      </Box>

      {copied && (
        <Alert severity="success" sx={{ mt: 2 }}>
          Invitation copied to clipboard!
        </Alert>
      )}

      <Divider sx={{ my: 2 }} />

      <Typography variant="body2" color="text.secondary">
        <strong>Preview:</strong>
      </Typography>
      <Paper 
        variant="outlined" 
        sx={{ 
          p: 2, 
          mt: 1, 
          bgcolor: 'grey.50',
          whiteSpace: 'pre-wrap',
          fontFamily: 'monospace',
          fontSize: '0.875rem'
        }}
      >
        {fullInvitationText}
      </Paper>
        </>
      )}
    </Paper>
  );
};

export default InvitationGenerator;
