import { useGetClaims } from '@/features/identity/api/getClaims'
import { useGetRoles } from '@/features/identity/api/getRoles'
import { Role } from '@/features/identity/types/roles'
import PageClaimsCard from '@/features/roles/components/PageClaimsCard'
import InfoIcon from '@mui/icons-material/Info'
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
  Tooltip,
} from '@mui/material'
import React, { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'

interface RoleFormData {
  roleName: string
  claims: string[]
}

interface ModalProps {
  isOpen: boolean
  onClose: () => void
  data: Role | null
  onSave: (roleData: RoleFormData) => void
}

const RoleModal: React.FC<ModalProps> = ({ isOpen, onSave, onClose, data }) => {
  const {
    data: rolesData,
    isLoading: rolesIsLoading,
    error: rolesError,
  } = useGetRoles()
  const { isLoading: claimsIsLoading, error: claimsError } = useGetClaims()

  const [userClaims, setUserClaims] = useState<string[]>([])
  const [currentRole, setCurrentRole] = useState<string>('')
  const [tempRoleName, setTempRoleName] = useState<string>('')
  const [tempClaims, setTempClaims] = useState<string[]>([])
  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<RoleFormData>({
    defaultValues: {
      roleName: data?.role || '',
      claims: [],
    },
  })

  const roleId = data?.role || null
  const isNewRole = !roleId
  const watchedRoleName = watch('roleName')

  useEffect(() => {
    if (isNewRole) {
      setCurrentRole(watchedRoleName)
      setTempRoleName(watchedRoleName)
    } else {
      setCurrentRole(data?.role || '')
    }
  }, [watchedRoleName, isNewRole, data])

  const handleClaimsChange = (_role: string, claims: string[]) => {
    setUserClaims(claims)
    setTempClaims(claims)
  }

  const onSubmit = (formData: RoleFormData) => {
    onSave({
      roleName: formData.roleName,
      claims: tempClaims,
    })
    onClose()
  }

  if (rolesIsLoading || claimsIsLoading) {
    return
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
    <Dialog
      open={isOpen}
      onClose={onClose}
      aria-labelledby="role-permissions-label"
    >
      <form onSubmit={handleSubmit(onSubmit)}>
        <DialogTitle
          sx={{ fontSize: '1.3rem', margin: '2rem', mb: 0 }}
          id="role-permissions-label"
        >
          {isNewRole ? (
            <>
              Create New Role
              {errors.roleName && (
                <Box sx={{ color: 'error.main', fontSize: '0.8rem', mt: 1 }}>
                  {errors.roleName.message}
                </Box>
              )}
            </>
          ) : (
            <>
              Role Permissions - {roleId}
              <Tooltip
                title={
                  <React.Fragment>
                    <p>
                      Admin – All privileges are granted as described for the
                      Data, Technician & Configuration users, including access
                      to Menu Configuration, FAQs, Watch Dog, Settings, General
                      Settings, Roles, & Users.
                    </p>
                    <p>
                      Data Admin – Privileges are granted to the Admin menu to
                      access the Raw Data Export page.
                    </p>
                    <p>
                      General Configuration Admin – Privileges are granted to
                      add, edit, and delete all configurations excluding
                      location configuration.
                    </p>
                    <p>
                      Location Configuration Admin – Privileges are granted to
                      add, edit, and delete location configurations excluding
                      all other configurations.
                    </p>
                    <p>
                      Report Admin – Privileges are granted to access restricted
                      reports.
                    </p>
                    <p>
                      Role Admin – Privileges are granted to add, edit, and
                      delete roles.
                    </p>
                    <p>
                      User Admin – Privileges are granted to view, edit, and
                      delete users.
                    </p>
                    <p>
                      Watchdog Subscriber – Privileges are granted to receive
                      the watchdog email and access the watchdog report.
                    </p>
                  </React.Fragment>
                }
              >
                <InfoIcon
                  sx={{ ml: 1, color: 'action.active', fontSize: 20 }}
                />
              </Tooltip>
            </>
          )}
        </DialogTitle>
        <DialogContent>
          {isNewRole && (
            <Box sx={{ mb: 3, mt: 1 }}>
              <TextField
                fullWidth
                label="Role Name"
                {...register('roleName', {
                  required: 'Role name is required',
                  pattern: {
                    value: /^[A-Za-z0-9\s]+$/,
                    message: 'Role name can only contain letters and numbers',
                  },
                })}
                error={!!errors.roleName}
                helperText={errors.roleName?.message}
              />
            </Box>
          )}
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <PageClaimsCard
              id={isNewRole ? tempRoleName : (roleId ?? '')}
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
            <Button variant="contained" type="submit">
              {isNewRole ? 'Create Role' : 'Update Role'}
            </Button>
          </Box>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default RoleModal
