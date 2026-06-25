import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  Box,
  Divider,
  List,
  ListItemButton,
  ListItemText,
  Paper,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import AtspmSourcePanel from './AtspmSourcePanel'
import ClearGuideSourcePanel from './ClearGuideSourcePanel'
import PemsSourcePanel from './PemsSourcePanel'

const SOURCE_OPTIONS = [
  { label: 'ATSPM', value: 1 },
  { label: 'PeMS', value: 2 },
  { label: 'ClearGuide', value: 3 },
] as const

export default function SourceVersionManager() {
  const [active, setActive] = useState<number>(1)

  return (
    <ResponsivePageLayout title="Update Source Version">
      <Paper sx={{ p: 3 }}>
        <Typography variant="h5" gutterBottom>
          Manage Data Source & Processing
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          Choose a data source on the left to see how to update its version.
        </Typography>

        <Box sx={{ display: 'flex', alignItems: 'flex-start' }}>
          <Box sx={{ flexShrink: 0, minWidth: 300, overflowY: 'auto' }}>
            <List>
              {SOURCE_OPTIONS.map((opt) => (
                <ListItemButton
                  key={opt.value}
                  selected={active === opt.value}
                  onClick={() => setActive(opt.value)}
                >
                  <ListItemText primary={opt.label} />
                </ListItemButton>
              ))}
            </List>
          </Box>

          <Divider orientation="vertical" flexItem sx={{ mx: 3 }} />

          <Box sx={{ flex: 1, maxWidth: 800 }}>
            {active === 1 && <AtspmSourcePanel />}
            {active === 2 && <PemsSourcePanel />}
            {active === 3 && <ClearGuideSourcePanel />}
          </Box>
        </Box>
      </Paper>
    </ResponsivePageLayout>
  )
}
