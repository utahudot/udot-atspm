import {
  ApiKeyMetadata,
  CreateApiKeyData,
  CreatedApiKeyResponse,
  useApiKeys,
  useCreateApiKey,
  useIdentityClaims,
  useRevokeApiKey,
} from '@/features/identity/api/apiKeys'
import ApiKeyList from '@/features/identity/components/apiKeys/ApiKeyList'
import CreateApiKeyDialog from '@/features/identity/components/apiKeys/CreateApiKeyDialog'
import CreatedApiKeyDialog from '@/features/identity/components/apiKeys/CreatedApiKeyDialog'
import RevokeApiKeyDialog from '@/features/identity/components/apiKeys/RevokeApiKeyDialog'
import {
  getAssignableApiKeyClaims,
  getCookieClaims,
  getErrorMessage,
} from '@/features/identity/components/apiKeys/apiKeyUtils'
import { useNotificationStore } from '@/stores/notifications'
import AddIcon from '@mui/icons-material/Add'
import VisibilityIcon from '@mui/icons-material/Visibility'
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff'
import { Box, Button, CircularProgress, Typography } from '@mui/material'
import { useMemo, useState } from 'react'

interface ApiKeyManagementProps {
  currentClaims?: string[]
  apiKeys?: ApiKeyMetadata[]
  isApiKeysLoading?: boolean
  refetchApiKeys?: () => void
}

const ApiKeyManagement = ({
  currentClaims: suppliedClaims,
  apiKeys: suppliedApiKeys,
  isApiKeysLoading: suppliedIsApiKeysLoading,
  refetchApiKeys: suppliedRefetchApiKeys,
}: ApiKeyManagementProps) => {
  const { addNotification } = useNotificationStore()
  const currentClaims = suppliedClaims ?? getCookieClaims()
  const isGlobalAdmin = currentClaims.includes('Admin')
  const canViewApiKeys =
    isGlobalAdmin || currentClaims.includes('ApiKey:View')
  const canCreateApiKeys =
    isGlobalAdmin || currentClaims.includes('ApiKey:Create')
  const canRevokeApiKeys =
    isGlobalAdmin || currentClaims.includes('ApiKey:Revoke')

  const {
    data: queriedApiKeys = [],
    isLoading: queriedIsApiKeysLoading,
    refetch: queriedRefetchApiKeys,
  } = useApiKeys(suppliedApiKeys === undefined && canViewApiKeys)
  const { data: allClaims = [], isLoading: isClaimsLoading } =
    useIdentityClaims(isGlobalAdmin)
  const createApiKey = useCreateApiKey()
  const revokeApiKey = useRevokeApiKey()

  const apiKeys = suppliedApiKeys ?? queriedApiKeys
  const isApiKeysLoading =
    suppliedIsApiKeysLoading ?? queriedIsApiKeysLoading
  const refetchApiKeys = suppliedRefetchApiKeys ?? queriedRefetchApiKeys

  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [createdKey, setCreatedKey] = useState<CreatedApiKeyResponse | null>(
    null
  )
  const [revokeTarget, setRevokeTarget] = useState<ApiKeyMetadata | null>(null)
  const [showRevokedApiKeys, setShowRevokedApiKeys] = useState(false)

  const revokedApiKeyCount = apiKeys.filter((key) => key.isRevoked).length
  const visibleApiKeys = showRevokedApiKeys
    ? apiKeys
    : apiKeys.filter((key) => !key.isRevoked)

  const availableClaims = useMemo(() => {
    return getAssignableApiKeyClaims(isGlobalAdmin ? allClaims : currentClaims)
  }, [allClaims, currentClaims, isGlobalAdmin])

  const handleCreate = async (payload: CreateApiKeyData) => {
    try {
      const response = await createApiKey.mutateAsync(payload)
      setCreatedKey(response)
      addNotification({
        type: 'success',
        title: 'API key created',
      })
      return true
    } catch (error) {
      addNotification({
        type: 'error',
        title: 'API key create failed',
        message: getErrorMessage(error),
      })
      return false
    }
  }

  const handleRevoke = async () => {
    if (!revokeTarget) return

    try {
      await revokeApiKey.mutateAsync(revokeTarget.id)
      setRevokeTarget(null)
      refetchApiKeys()
      addNotification({
        type: 'success',
        title: 'API key revoked',
      })
    } catch (error) {
      addNotification({
        type: 'error',
        title: 'API key revoke failed',
        message: getErrorMessage(error),
      })
    }
  }

  const handleCopy = async () => {
    if (!createdKey?.key || !navigator.clipboard) return

    await navigator.clipboard.writeText(createdKey.key)
    addNotification({
      type: 'success',
      title: 'API key copied',
    })
  }

  if (!canViewApiKeys && !canCreateApiKeys) {
    return (
      <Box>
        <Typography variant="h4" sx={{ mb: 1 }}>
          API Keys
        </Typography>
        <Typography variant="body2" color="text.secondary">
          API key management is not enabled for your account.
        </Typography>
      </Box>
    )
  }

  return (
    <Box>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          gap: 2,
          alignItems: { xs: 'flex-start', sm: 'center' },
          flexDirection: { xs: 'column', sm: 'row' },
          mb: 2,
        }}
      >
        <Box>
          <Typography variant="h4" sx={{ mb: 0.5 }}>
            API Keys
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Create and revoke keys for API integrations.
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
          {revokedApiKeyCount > 0 && (
            <Button
              variant="outlined"
              startIcon={
                showRevokedApiKeys ? <VisibilityOffIcon /> : <VisibilityIcon />
              }
              onClick={() => setShowRevokedApiKeys((current) => !current)}
            >
              {showRevokedApiKeys ? 'Hide revoked keys' : 'Show revoked keys'}
            </Button>
          )}
          {canCreateApiKeys && (
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => setIsCreateOpen(true)}
            >
              New API Key
            </Button>
          )}
        </Box>
      </Box>

      {isApiKeysLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
          <CircularProgress size={28} />
        </Box>
      ) : apiKeys.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No API keys have been created.
        </Typography>
      ) : visibleApiKeys.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No active API keys. Show revoked keys to view revoked keys.
        </Typography>
      ) : (
        <ApiKeyList
          apiKeys={visibleApiKeys}
          isGlobalAdmin={isGlobalAdmin}
          canRevokeApiKeys={canRevokeApiKeys}
          onRevoke={setRevokeTarget}
        />
      )}

      <CreateApiKeyDialog
        open={isCreateOpen}
        availableClaims={availableClaims}
        isClaimsLoading={isClaimsLoading}
        isCreating={createApiKey.isLoading}
        onClose={() => setIsCreateOpen(false)}
        onCreate={handleCreate}
      />

      <CreatedApiKeyDialog
        createdKey={createdKey}
        onClose={() => setCreatedKey(null)}
        onCopy={handleCopy}
      />

      <RevokeApiKeyDialog
        apiKey={revokeTarget}
        isRevoking={revokeApiKey.isLoading}
        onClose={() => setRevokeTarget(null)}
        onConfirm={handleRevoke}
      />
    </Box>
  )
}

export default ApiKeyManagement
