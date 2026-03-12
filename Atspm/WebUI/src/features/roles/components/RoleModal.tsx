import ATSPMDialog from '@/components/ATSPMDialog'
import { useGetClaims } from '@/features/identity/api/getClaims'
import { useGetRoles } from '@/features/identity/api/getRoles'
import { Role } from '@/features/identity/types/roles'
import PageClaimsCard from '@/features/roles/components/PageClaimsCard'
import { Box, TextField } from '@mui/material'
import { useState } from 'react'
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

const RoleModal = ({ isOpen, onSave, onClose, data }: ModalProps) => {
  const {
    data: rolesData,
    isLoading: rolesIsLoading,
    error: rolesError,
  } = useGetRoles()
  const {
    data: claimsData,
    isLoading: claimsIsLoading,
    error: claimsError,
  } = useGetClaims()

  const [userClaims, setUserClaims] = useState<string[]>(data?.claims || [])
  const [currentRole, setCurrentRole] = useState<string>(data?.role || '')

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors, isValid },
  } = useForm<RoleFormData>({
    defaultValues: {
      roleName: data?.role || '',
      claims: data?.claims || [],
    },
    mode: 'onChange',
  })

  const roleId = data?.role
  const isNewRole = !roleId
  const watchedRoleName = watch('roleName')

  const handleClaimsChange = (_role: string, claims: string[]) => {
    setUserClaims(claims)
    setValue('claims', claims)
  }

  const onSubmit = (formData: RoleFormData) => {
    if (!formData.roleName) return
    onSave({
      roleName: formData.roleName,
      claims: userClaims,
    })
    onClose()
  }

  const existingRoleNames = (rolesData || []).map((role) =>
    role.role.toLowerCase()
  )
  const isDuplicateRoleName =
    Boolean(isNewRole) &&
    Boolean(watchedRoleName) &&
    existingRoleNames.includes(watchedRoleName.toLowerCase())

  if (rolesIsLoading || claimsIsLoading) return null
  if (rolesError || claimsError) {
    return <div>Error: {rolesError?.message || claimsError?.message}</div>
  }

  return (
    <ATSPMDialog
      isOpen={isOpen}
      onClose={onClose}
      title={isNewRole ? 'Create New Role' : `Role Permissions - ${roleId}`}
      auditInfo={data}
      onSubmit={handleSubmit(onSubmit)}
      dialogProps={{ sx: { minWidth: 600 } }}
    >
      {isNewRole && (
        <Box sx={{ mb: 3, mt: 1 }}>
          <TextField
            fullWidth
            label="Role Name"
            {...register('roleName', {
              required: 'Role name is required',
              validate: (value) => {
                if (!value || value.trim() === '') return true
                return (
                  !existingRoleNames.includes(value.toLowerCase()) ||
                  'Role name already exists'
                )
              },
            })}
            error={!!errors.roleName || isDuplicateRoleName}
            helperText={errors.roleName ? errors.roleName.message : ''}
          />
        </Box>
      )}

      <PageClaimsCard
        id={isNewRole ? watchedRoleName : (roleId ?? '')}
        currentClaims={rolesData || []}
        onClaimsChange={handleClaimsChange}
        currentRole={currentRole}
        setCurrentRole={setCurrentRole}
        userClaims={userClaims}
        setUserClaims={setUserClaims}
        claimsData={claimsData}
        isNewRole={isNewRole}
      />
    </ATSPMDialog>
  )
}

export default RoleModal
