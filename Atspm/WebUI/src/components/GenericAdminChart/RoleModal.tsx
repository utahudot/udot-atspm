import { useAddRoleClaims } from '@/features/identity/api/addRoleClaims'
import { useGetClaims } from '@/features/identity/api/getClaims'
import { useGetRoles } from '@/features/identity/api/getRoles'
import { Role } from '@/features/identity/types/roles'
import PageClaimsCard from '@/features/roles/components/PageClaimsCard'
import { useNotificationStore } from '@/stores/notifications'
import { Tooltip } from '@mui/material';
import InfoIcon from '@mui/icons-material/Info';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
} from '@mui/material'
import React, { useState } from 'react'

interface ModalProps {
  open: boolean
  onClose: () => void
  data: Role | null
  onSave: (role: Role) => void
}

const RoleModal: React.FC<ModalProps> = ({ open, onClose, data }) => {
  const {
    data: rolesData,
    isLoading: rolesIsLoading,
    error: rolesError,
    refetch: refetchRoles,
  } = useGetRoles()
  const { mutate: addClaimsMutation } = useAddRoleClaims()
  const { isLoading: claimsIsLoading, error: claimsError } = useGetClaims()

  const [currentRole, setCurrentRole] = useState<string>('')
  const [userClaims, setUserClaims] = useState<string[]>([])

  const addNotification = useNotificationStore((state) => state.addNotification)
  const roleId = data?.role || null

  const handleClaimsChange = (role: string, claims: string[]) => {
    console.log(`Updated claims for ${role}:`, claims)
    setUserClaims(claims)
  }

  const handleSaveClick = () => {
    if (currentRole && userClaims) {
      addClaimsMutation(
        { roleName: currentRole, claims: userClaims },
        {
          onSuccess: () => {
            addNotification({
              type: 'success',
              title: 'Success',
              message: 'Permissions added to role successfully',
            })
            refetchRoles()
            onClose()
          },
          onError: (error) => {
            console.error('Failed to add permissions:', error)
            addNotification({
              type: 'error',
              title: 'Error',
              message: 'Failed to add permissions:',
            })
          },
        }
      )
    } else {
      console.error('Current role or user claims are missing')
      addNotification({
        type: 'warning',
        title: 'Warning',
        message: 'No permissions were added',
      })
    }
  }

  if (rolesIsLoading || claimsIsLoading) {
    return <div>Loading...</div>
  }

  if (rolesError || claimsError) {
    return (
      <div>
        Error:{' '}
        {(rolesError as Error)?.message || (claimsError as Error)?.message}
      </div>
    )
  }
  return (
<Dialog open={open} onClose={onClose} aria-labelledby="role-permissions-label">
  <DialogTitle sx={{ fontSize: '1.3rem', margin: '2rem' }} id="role-permissions-label">Role Permissions - {roleId}
  <Tooltip
  title={
    <React.Fragment>
      <p>Admin – All privileges are granted as described for the Data, Technician & Configuration users, including access to Menu Configuration, FAQs, Watch Dog, Settings, General Settings, Roles, & Users.</p>
      <p>Data Admin – Privileges are granted to the Admin menu to access the Raw Data Export page.</p>
      <p>General Configuration Admin – Privileges are granted to add, edit, and delete all configurations excluding location configuration.</p>
      <p>Location Configuration Admin – Privileges are granted to add, edit, and delete location configurations excluding all other configurations.</p>
      <p>Report Admin – Privileges are granted to access restricted reports.</p>
      <p>Role Admin – Privileges are granted to add, edit, and delete roles.</p>
      <p>User Admin – Privileges are granted to view, edit, and delete users.</p>
      <p>Watchdog Subscriber – Privileges are granted to receive the watchdog email and access the watchdog report.</p>
    </React.Fragment>
  }
>
  <InfoIcon sx={{ ml: 1, color: 'action.active', fontSize: 20 }} />
</Tooltip>

  </DialogTitle>
  <DialogContent>
    <Box sx={{ display: 'flex', alignItems: 'center', }}>
      <PageClaimsCard
        id={roleId ?? ''}
        currentClaims={rolesData || []}
        onClaimsChange={handleClaimsChange}
        currentRole={currentRole}
        setCurrentRole={setCurrentRole}
        userClaims={userClaims}
        setUserClaims={setUserClaims}
      />
    </Box>
  </DialogContent>
  <DialogActions>
    <Box sx={{ marginRight: '1rem', marginBottom: '.5rem' }}>
      <Button onClick={onClose}>Cancel</Button>
      <Button variant="contained" onClick={handleSaveClick}>
        Update Role
      </Button>
    </Box>
  </DialogActions>
</Dialog>

  )
}

export default RoleModal
