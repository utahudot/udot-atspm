import { useFlags } from '@/feature-flags/FeatureFlagContext'
import { PageNames, useGetAdminPagesList } from '@/features/identity/pagesCheck'
import { Box, Button, Divider, Menu, MenuItem, Typography } from '@mui/material'
import { ArrowDropDownIcon } from '@mui/x-date-pickers'
import NextLink from 'next/link'
import React, { useState } from 'react'

const GROUPS: Record<string, PageNames[]> = {
  'Location Configuration': [
    PageNames.Location,
    PageNames.Routes,
    PageNames.Products,
    PageNames.DeviceConfigurations,
  ],
  'Location Categories': [
    PageNames.Areas,
    PageNames.Region,
    PageNames.Jurisdiction,
  ],
  'Speed Management': [
    PageNames.Segments,
    PageNames.Impacts,
    PageNames.ImpactTypes,
    PageNames.UpdateNewEntityVersion,
  ],
  'User Management': [PageNames.Users, PageNames.Roles],
  Other: [PageNames.FAQs, PageNames.MenuItems, PageNames.MeasureDefaults],
}

const AdminMenu = () => {
  const flags = useFlags()
  const pagesToLinks = useGetAdminPagesList()

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)

  const handleClick = (e: React.MouseEvent<HTMLButtonElement>) =>
    setAnchorEl(e.currentTarget)
  const handleClose = () => setAnchorEl(null)

  if (!flags.speedManagementTool) {
    delete GROUPS['Speed Management']
  }

  const visibleSections = Object.entries(GROUPS)
    .map(([label, keys]) => ({
      label,
      items: keys.filter((k) => pagesToLinks.has(k)),
    }))
    .filter((section) => section.items.length > 0)

  return (
    <>
      <Button
        color={'error'}
        variant={'outlined'}
        aria-controls={'menu-list-grow'}
        aria-haspopup="true"
        aria-expanded={true}
        onClick={handleClick}
        endIcon={<ArrowDropDownIcon />}
        sx={{
          mx: '2px',
          color: 'black',
          textTransform: 'none',
          '& .MuiButton-endIcon': { ml: '0px' },
        }}
      >
        <Typography fontWeight={400} sx={{ textTransform: 'none' }}>
          Admin
        </Typography>
      </Button>

      <Menu
        id="adminMenu"
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleClose}
        MenuListProps={{ 'aria-labelledby': 'adminMenu' }}
        slotProps={{ paper: { sx: { py: 1, minWidth: 220 } } }}
      >
        {visibleSections.map(({ label, items }, idx) => {
          return (
            <Box key={label}>
              {idx !== 0 && <Divider sx={{ my: 1 }} />}

              <Box sx={{ px: 2 }}>
                <Typography
                  variant="subtitle2"
                  color="primary"
                  sx={{ fontSize: '0.8rem' }}
                >
                  {label}
                </Typography>
                {items.map((k) => (
                  <MenuItem
                    key={k}
                    component={NextLink}
                    href={pagesToLinks.get(k) as string}
                    onClick={handleClose}
                    sx={{ pl: 1 }}
                  >
                    {k}
                  </MenuItem>
                ))}
              </Box>
            </Box>
          )
        })}
      </Menu>
    </>
  )
}

export default AdminMenu
