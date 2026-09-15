import { ApiKeyMetadata } from '@/features/identity/api/apiKeys'
import {
  formatDate,
  formatOwner,
} from '@/features/identity/components/apiKeys/apiKeyUtils'
import { Box, Button, Chip, Paper, Stack, Typography } from '@mui/material'

interface ApiKeyListProps {
  apiKeys: ApiKeyMetadata[]
  isGlobalAdmin: boolean
  canRevokeApiKeys: boolean
  onRevoke: (apiKey: ApiKeyMetadata) => void
}

const ApiKeyList = ({
  apiKeys,
  isGlobalAdmin,
  canRevokeApiKeys,
  onRevoke,
}: ApiKeyListProps) => {
  return (
    <Paper
      variant="outlined"
      role="list"
      aria-label="API keys"
      sx={{
        borderRadius: 1,
        overflow: 'hidden',
      }}
    >
      {apiKeys.map((key) => (
        <Box
          key={key.id}
          role="listitem"
          sx={{
            p: 2,
            display: 'grid',
            gridTemplateColumns: {
              xs: '1fr',
              lg: canRevokeApiKeys
                ? 'minmax(180px, 1fr) minmax(220px, 1fr) minmax(220px, 1.2fr) auto'
                : 'minmax(180px, 1fr) minmax(220px, 1fr) minmax(220px, 1.2fr)',
            },
            gap: { xs: 1.5, lg: 2 },
            alignItems: 'start',
            borderBottom: 1,
            borderColor: 'divider',
            '&:last-child': {
              borderBottom: 0,
            },
          }}
        >
          <Box>
            <Box
              sx={{
                display: 'flex',
                alignItems: 'flex-start',
                gap: 1,
                flexWrap: 'wrap',
              }}
            >
              <Typography variant="subtitle1" fontWeight={600}>
                {key.name}
              </Typography>
              <Chip
                size="small"
                label={key.isRevoked ? 'Revoked' : 'Active'}
                color={key.isRevoked ? 'default' : 'success'}
              />
            </Box>
            {isGlobalAdmin && (
              <Typography variant="body2" color="text.secondary">
                {formatOwner(key)}
              </Typography>
            )}
          </Box>

          <Stack direction="row" gap={3} flexWrap="wrap">
            <Box>
              <Typography variant="caption" color="text.secondary">
                Created
              </Typography>
              <Typography variant="body2">{formatDate(key.createdAt)}</Typography>
            </Box>
            <Box>
              <Typography variant="caption" color="text.secondary">
                Expires
              </Typography>
              <Typography variant="body2">{formatDate(key.expiresAt)}</Typography>
            </Box>
          </Stack>

          <Box>
            <Typography variant="caption" color="text.secondary">
              Claims
            </Typography>
            <Stack direction="row" gap={0.75} flexWrap="wrap" mt={0.5}>
              {key.claims.length > 0 ? (
                key.claims.map((claim) => (
                  <Chip key={claim} size="small" label={claim} />
                ))
              ) : (
                <Typography variant="body2">None</Typography>
              )}
            </Stack>
          </Box>

          {canRevokeApiKeys && (
            <Box
              sx={{
                justifySelf: { xs: 'start', lg: 'end' },
                alignSelf: 'center',
              }}
            >
              <Button
                size="small"
                color="error"
                variant="outlined"
                disabled={key.isRevoked}
                onClick={() => onRevoke(key)}
              >
                Revoke
              </Button>
            </Box>
          )}
        </Box>
      ))}
    </Paper>
  )
}

export default ApiKeyList
