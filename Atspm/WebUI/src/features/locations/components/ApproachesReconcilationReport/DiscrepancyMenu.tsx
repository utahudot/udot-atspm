import { ConfigApproach } from '@/features/locations/components/editLocation/locationStore'
import AddCircleIcon from '@mui/icons-material/AddCircle'
import DeleteIcon from '@mui/icons-material/Delete'
import RemoveIcon from '@mui/icons-material/Remove'
import { Box, ListItemIcon, ListItemText, Menu, MenuItem } from '@mui/material'
import { DiscrepancyItem } from './DiscrepancyRow'

export interface DiscrepancyMenuProps {
  anchorEl: HTMLElement | null
  menuItem: DiscrepancyItem | null
  onClose: () => void
  onIgnore: (item: DiscrepancyItem) => void
  onAddApproach: (item: DiscrepancyItem) => void
  onAddDetector: (approachId: number, item: DiscrepancyItem) => void
  onDelete: (item: DiscrepancyItem) => void
  approaches: ConfigApproach[]
}

const DiscrepancyMenu = ({
  anchorEl,
  menuItem,
  onClose,
  onIgnore,
  onAddApproach,
  onAddDetector,
  onDelete,
  approaches,
}: DiscrepancyMenuProps) => {
  if (!menuItem) return null

  return (
    <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={onClose}>
      {menuItem.kind === 'FOUND_PHASE' && (
        <Box>
          <MenuItem
            onClick={() => {
              onIgnore(menuItem)
              onClose()
            }}
          >
            <ListItemIcon>
              <RemoveIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>Ignore</ListItemText>
          </MenuItem>
          <MenuItem
            onClick={() => {
              onAddApproach(menuItem)
              onClose()
            }}
          >
            <ListItemIcon>
              <AddCircleIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>Add Approach</ListItemText>
          </MenuItem>
        </Box>
      )}
      {menuItem.kind === 'FOUND_DET' && (
        <Box>
          <MenuItem
            onClick={() => {
              onIgnore(menuItem)
              onClose()
            }}
          >
            <ListItemIcon>
              <RemoveIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>Ignore</ListItemText>
          </MenuItem>
          {approaches.map((a) => (
            <MenuItem
              key={a.id}
              onClick={() => {
                onAddDetector(a.id, menuItem)
                onClose()
              }}
            >
              <ListItemIcon>
                <AddCircleIcon fontSize="small" />
              </ListItemIcon>
              <ListItemText>
                Add to {a.description || `Approach ${a.id}`}
              </ListItemText>
            </MenuItem>
          ))}
        </Box>
      )}
      {menuItem.kind === 'NOT_FOUND_APP' && (
        <Box>
          <MenuItem
            onClick={() => {
              onIgnore(menuItem)
              onClose()
            }}
          >
            <ListItemIcon>
              <RemoveIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>Ignore</ListItemText>
          </MenuItem>
          <MenuItem
            onClick={() => {
              onDelete(menuItem)
              onClose()
            }}
          >
            <ListItemIcon>
              <DeleteIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>Delete Approach</ListItemText>
          </MenuItem>
        </Box>
      )}
      {menuItem.kind === 'NOT_FOUND_DET' && (
        <Box>
          <MenuItem
            onClick={() => {
              onIgnore(menuItem)
              onClose()
            }}
          >
            <ListItemIcon>
              <RemoveIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>Ignore</ListItemText>
          </MenuItem>
          <MenuItem
            onClick={() => {
              onDelete(menuItem)
              onClose()
            }}
          >
            <ListItemIcon>
              <DeleteIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>Delete Detector</ListItemText>
          </MenuItem>
        </Box>
      )}
    </Menu>
  )
}

export default DiscrepancyMenu
