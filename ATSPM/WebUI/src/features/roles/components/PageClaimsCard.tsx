// PageClaimsCard.tsx
import { Box, Checkbox, Typography } from '@mui/material'
import { useEffect, useState } from 'react'
import { Role } from '../types/roles'

interface PageClaimsCardProps {
  currentClaims: { role: string; claims: string[] }[]
  onClaimsChange: (role: string, claims: string[]) => void
  currentRole: string
  setCurrentRole: (role: string) => void
  userClaims: string[]
  setUserClaims: (claims: string[]) => void
  id: string
}

const PageClaimsCard = ({
  currentClaims,
  onClaimsChange,
  currentRole,
  setCurrentRole,
  userClaims,
  setUserClaims,
  id,
}: PageClaimsCardProps) => {
  const claims = [
    'User:View',
    'User:Edit',
    'User:Delete',
    'Role:View',
    'Role:Edit',
    'Role:Delete',
    'LocationConfiguration:View',
    'LocationConfiguration:Edit',
    'LocationConfiguration:Delete',
    'GeneralConfiguration:View',
    'GeneralConfiguration:Edit',
    'GeneralConfiguration:Delete',
    'Data:View',
    'Data:Edit',
    'Watchdog:View',
    'Report:View',
  ]

  const SetInStone: Role[] = [
    {
      role: 'Admin',
      claims: ['Admin'],
    },
    {
      role: 'ReportAdmin',
      claims: ['Report:View'],
    },
    {
      role: 'RoleAdmin',
      claims: ['Role:View', 'Role:Edit', 'Role:Delete'],
    },
    {
      role: 'UserAdmin',
      claims: ['User:Edit', 'User:Delete', 'User:View'],
    },
    {
      role: 'LocationConfigurationAdmin',
      claims: [
        'LocationConfiguration:View',
        'LocationConfiguration:Edit',
        'LocationConfiguration:Delete',
      ],
    },
    {
      role: 'GeneralConfigurationAdmin',
      claims: [
        'GeneralConfiguration:View',
        'GeneralConfiguration:Edit',
        'GeneralConfiguration:Delete',
      ],
    },
    {
      role: 'DataAdmin',
      claims: ['Data:View', 'Data:Edit'],
    },
    {
      role: 'WatchdogSubscriber',
      claims: ['Watchdog:View'],
    },
  ]

  const spaceIdName = id
    ? id
        .toString()
        .replace(/(\[A-Z\])/g, ' $1')
        .trim()
    : ''

  const roleCurrentClaims =
    currentClaims.find((item) => item.role === id)?.claims || []
  const roleSetInStoneClaims =
    SetInStone.find((item) => item.role === id)?.claims || []

  const [selectedClaims, setSelectedClaims] =
    useState<string[]>(roleCurrentClaims)

  useEffect(() => {
    if (id === 'Admin') {
      setSelectedClaims(claims)
      onClaimsChange(id as string, claims)
    }
  }, [id])

  const handleClaimChange = (claim: string, checked: boolean) => {
    if (id === 'Admin' || roleSetInStoneClaims.includes(claim)) return

    const updatedClaims = checked
      ? [...selectedClaims, claim]
      : selectedClaims.filter((c) => c !== claim)
    setSelectedClaims(updatedClaims)
    onClaimsChange(id as string, updatedClaims)
  }

  const getPermissionName = (claim: string) => {
    return claim.split(':')[0]
  }

  const uniquePermissions = Array.from(new Set(claims.map(getPermissionName)))

  const formatPermissionName = (permission: string) => {
    return permission.replace(/(?<!^)([A-Z])/g, ' $1')
  }

  useEffect(() => {
    if (id === 'Admin') {
      setUserClaims(claims)
      onClaimsChange(id as string, claims)
    } else {
      setCurrentRole(id as string)
      setUserClaims(roleCurrentClaims)
    }
  }, [id, roleCurrentClaims])

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
        <Box
          sx={{
            flexBasis: '35%',
            display: 'flex',
            justifyContent: 'flex-start',
            paddingLeft: '4rem',
          }}
        >
          <Typography sx={{ mr: 2 }}>Permission</Typography>
        </Box>
        <Box
          sx={{
            flexBasis: '66.67%',
            display: 'flex',
            alignItems: 'center',
            paddingLeft: '8.5rem',
          }}
        >
          <Typography sx={{ mr: 4 }}>View</Typography>
          <Typography sx={{ mr: 4 }}>Edit</Typography>
          <Typography>Delete</Typography>
        </Box>
      </Box>
      {uniquePermissions.map((permission) => {
        const permissionClaims = claims.filter((claim) =>
          claim.startsWith(permission)
        )
        const hasView = permissionClaims.some((claim) => claim.endsWith('View'))
        const hasEdit = permissionClaims.some((claim) => claim.endsWith('Edit'))
        const hasDelete = permissionClaims.some((claim) =>
          claim.endsWith('Delete')
        )

        return (
          <Box
            key={permission}
            sx={{ display: 'flex', alignItems: 'center', mb: 1 }}
          >
            <Box
              sx={{
                flexBasis: '35%',
                display: 'flex',
                paddingLeft: '4rem',
                justifyContent: 'flex-start',
              }}
            >
              <Typography sx={{ mr: 2 }}>{`${formatPermissionName(
                permission
              )}:`}</Typography>
            </Box>
<Box
  sx={{
    flexBasis: '66.67%',
    display: 'flex',
    alignItems: 'center',
    paddingLeft: '8rem',
  }}
>
  {hasView && (
    <Box sx={{ display: 'flex', alignItems: 'center', mr: 4 }}>
      <Checkbox
        id={`${permission}-view-checkbox`}
        checked={
          id === 'Admin' ||
          selectedClaims.includes(`${permission}:View`)
        }
        onChange={(e) =>
          handleClaimChange(`${permission}:View`, e.target.checked)
        }
        disabled={
          id === 'Admin' ||
          roleSetInStoneClaims.includes(`${permission}:View`)
        }
      />
      <label
        htmlFor={`${permission}-view-checkbox`}
        style={{ visibility: 'hidden', position: 'absolute', marginLeft:'-15rem'  }}
      >{`${permission} View`}</label>
    </Box>
  )}
  {hasEdit && (
    <Box sx={{ display: 'flex', alignItems: 'center', mr: 3 }}>
      <Checkbox
        id={`${permission}-edit-checkbox`}
        checked={
          id === 'Admin' ||
          selectedClaims.includes(`${permission}:Edit`)
        }
        onChange={(e) =>
          handleClaimChange(`${permission}:Edit`, e.target.checked)
        }
        disabled={
          id === 'Admin' ||
          roleSetInStoneClaims.includes(`${permission}:Edit`)
        }
      />
      <label
        htmlFor={`${permission}-edit-checkbox`}
        style={{ visibility: 'hidden', position: 'absolute', marginLeft:'-15rem' }}
      >{`${permission} Edit`}</label>
    </Box>
  )}
  {hasDelete && (
    <Box sx={{ display: 'flex', alignItems: 'center', mr: 2 }}>
      <Checkbox
        id={`${permission}-delete-checkbox`}
        checked={
          id === 'Admin' ||
          selectedClaims.includes(`${permission}:Delete`)
        }
        onChange={(e) =>
          handleClaimChange(`${permission}:Delete`, e.target.checked)
        }
        disabled={
          id === 'Admin' ||
          roleSetInStoneClaims.includes(`${permission}:Delete`)
        }
      />
      <label
        htmlFor={`${permission}-delete-checkbox`}
        style={{ visibility: 'hidden', position: 'absolute', marginLeft:'-15rem'  }}
      >{`${permission} Delete`}</label>
    </Box>
  )}
</Box>

          </Box>
        )
      })}
    </Box>
  )
}

export default PageClaimsCard
