import { useFlags } from '@/feature-flags/FeatureFlagContext'
import SyncIcon from '@mui/icons-material/Sync'
import { LoadingButton } from '@mui/lab'
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Tab,
  Tabs,
} from '@mui/material'
import { ReactNode, useState } from 'react'
import { LaneDialog } from './LaneDialog'

export default function ReconcileConfigurationButton({
  onReconcileApproaches,
  approachesBusy,
  approachReport,
}: {
  onReconcileApproaches: () => Promise<void>
  approachesBusy: boolean
  approachReport: ReactNode
}) {
  const flags = useFlags()
  const lanesEnabled = flags.laneReconciliation === true
  const [open, setOpen] = useState(false)
  const [tab, setTab] = useState('approaches')
  const [lanesVisited, setLanesVisited] = useState(false)
  const [lanesBusy, setLanesBusy] = useState(false)
  const busy = approachesBusy || lanesBusy
  const close = () => {
    if (!busy) setOpen(false)
  }
  return (
    <>
      <LoadingButton
        startIcon={<SyncIcon />}
        loading={approachesBusy}
        loadingPosition="start"
        variant="outlined"
        onClick={() => {
          if (lanesEnabled) {
            setTab('approaches')
            setLanesVisited(false)
            setOpen(true)
          }
          void onReconcileApproaches()
        }}
      >
        Reconcile Approaches
      </LoadingButton>
      {/* Disabled installations retain the original workflow and make no lane requests. */}
      {open && lanesEnabled && (
        <Dialog
          open
          disableEscapeKeyDown={busy}
          onClose={close}
          fullWidth
          maxWidth="xl"
        >
          <DialogTitle>Reconcile Approaches and Lanes</DialogTitle>
          <Tabs
            value={tab}
            onChange={(_, value) => {
              setTab(value)
              if (value === 'lanes') setLanesVisited(true)
            }}
            aria-label="Reconciliation"
          >
            <Tab
              label="Approaches"
              value="approaches"
              id="reconcile-approaches-tab"
              aria-controls="reconcile-approaches-panel"
            />
            <Tab
              label="Lanes"
              value="lanes"
              id="reconcile-lanes-tab"
              aria-controls="reconcile-lanes-panel"
            />
          </Tabs>
          <Box
            role="tabpanel"
            hidden={tab !== 'approaches'}
            id="reconcile-approaches-panel"
            aria-labelledby="reconcile-approaches-tab"
          >
            <DialogContent>
              <LoadingButton
                loading={approachesBusy}
                disabled={busy}
                onClick={() => void onReconcileApproaches()}
              >
                Compare approaches
              </LoadingButton>
              {approachReport}
            </DialogContent>
            <DialogActions>
              <Button disabled={busy} onClick={close}>
                Close
              </Button>
            </DialogActions>
          </Box>
          {lanesVisited && (
            <Box
              role="tabpanel"
              hidden={tab !== 'lanes'}
              id="reconcile-lanes-panel"
              aria-labelledby="reconcile-lanes-tab"
            >
              <LaneDialog onClose={close} onBusyChange={setLanesBusy} />
            </Box>
          )}
        </Dialog>
      )}
    </>
  )
}
