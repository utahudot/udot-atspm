import { Box, MenuItem, Select, Typography } from '@mui/material'
import { useEffect, useState } from 'react'

interface PageClaimsCardProps {
  currentClaims: { role: string; claims: string[] }[]
  onClaimsChange: (role: string, claims: string[]) => void
  userClaims: string[]
  setUserClaims: (claims: string[]) => void
  id: string
  claimsData?: string[]
  isNewRole?: boolean
}

const PageClaimsCard = ({
  currentClaims,
  onClaimsChange,
  userClaims,
  setUserClaims,
  id,
  claimsData,
  isNewRole,
}: PageClaimsCardProps) => {
  const claims = claimsData?.filter((claim) => claim !== 'Admin') || []
  const roleCurrentClaims =
    currentClaims.find((item) => item.role === id)?.claims || []

  const [selectedPermissions, setSelectedPermissions] = useState<{
    [key: string]: string
  }>({})

  const getPermissionName = (claim: string) => claim.split(':')[0]
  const uniquePermissions = Array.from(new Set(claims.map(getPermissionName)))

  const getAvailableOptions = (permission: string) => {
    const availableClaims = claims.filter((c) => c.startsWith(permission))
    const options: string[] = []
    if (availableClaims.some((c) => c.endsWith('View'))) options.push('View')
    if (availableClaims.some((c) => c.endsWith('Edit')))
      options.push('View & Edit')
    if (availableClaims.some((c) => c.endsWith('Delete')))
      options.push('View, Edit, Delete')
    return options
  }

  const permissionDescriptions: Record<string, string> = {
    User: 'Update and delete user accounts, and assign roles to users.',
    Role: 'Manage roles, and assign permissions to roles.',
    LocationConfiguration: 'Manage locations info and settings.',
    GeneralConfiguration: 'Manage faqs, areas, regions, jurisdictions, etc.',
    Data: 'Export raw event logs.',
    Watchdog:
      'View the system’s watchdog logs and subscribe to daily watchdog email updates.',
    Report: 'View the left turn gap report.',
  }

  useEffect(() => {
    if (!id || !claims.length) return

    const initialPermissions: { [key: string]: string } = {}
    const initialClaims = isNewRole
      ? userClaims
      : userClaims.length > 0
        ? userClaims
        : roleCurrentClaims

    uniquePermissions.forEach((permission) => {
      const permClaims = initialClaims.filter((c) => c.startsWith(permission))
      if (permClaims.includes(`${permission}:Delete`)) {
        initialPermissions[permission] = 'View, Edit, Delete'
      } else if (permClaims.includes(`${permission}:Edit`)) {
        initialPermissions[permission] = 'View & Edit'
      } else if (permClaims.includes(`${permission}:View`)) {
        initialPermissions[permission] = 'View'
      } else {
        initialPermissions[permission] = ''
      }
    })

    setSelectedPermissions(initialPermissions)
  }, [id, roleCurrentClaims, isNewRole])

  const formatPermissionName = (permission: string) =>
    permission.replace(/(?<!^)([A-Z])/g, ' $1')

  const handlePermissionChange = (permission: string, value: string) => {
    const updatedPermissions = { ...selectedPermissions, [permission]: value }
    setSelectedPermissions(updatedPermissions)

    const newClaims: string[] = []

    Object.entries(updatedPermissions).forEach(([perm, val]) => {
      switch (val) {
        case 'View':
          newClaims.push(`${perm}:View`)
          break
        case 'View & Edit':
          newClaims.push(`${perm}:View`, `${perm}:Edit`)
          break
        case 'View, Edit, Delete':
          newClaims.push(`${perm}:View`, `${perm}:Edit`, `${perm}:Delete`)
          break
      }
    })

    setUserClaims(newClaims.length > 0 ? [...newClaims] : [])
    onClaimsChange(id, newClaims.length > 0 ? [...newClaims] : [])
  }

  return (
    <>
      {uniquePermissions.map((permission) => {
        const availableOptions = getAvailableOptions(permission)
        return (
          <Box
            key={permission}
            sx={{
              display: 'flex',
              flexDirection: 'column',
              mb: 2,
              width: '100%',
            }}
          >
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                mb: 1,
              }}
            >
              <Box>
                <Typography variant="h6" component="div" fontWeight="bold">
                  {formatPermissionName(permission)}
                </Typography>
                <Box sx={{ marginRight: 2, maxWidth: '400px' }}>
                  <Typography variant="body2" color="text.secondary">
                    {permissionDescriptions[permission] ||
                      'No description available.'}
                  </Typography>
                </Box>
              </Box>
              <Box sx={{ minWidth: 200 }}>
                <Select
                  fullWidth
                  size="small"
                  value={selectedPermissions[permission] || ''}
                  onChange={(e) =>
                    handlePermissionChange(permission, e.target.value)
                  }
                  displayEmpty
                >
                  <MenuItem value="">None</MenuItem>
                  {availableOptions.map((option) => (
                    <MenuItem key={option} value={option}>
                      {option}
                    </MenuItem>
                  ))}
                </Select>
              </Box>
            </Box>
          </Box>
        )
      })}
    </>
  )
}

export default PageClaimsCard
