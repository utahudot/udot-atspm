import { RouteLocation } from '@/features/routes/types'
import CheckIcon from '@mui/icons-material/Check'
import CloseIcon from '@mui/icons-material/Close'
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline'
import {
  Box,
  ButtonBase,
  Grid,
  IconButton,
  Paper,
  Typography,
} from '@mui/material'

const OverlapIcon = ({ isOverlap }: { isOverlap: boolean | null }) => {
  return isOverlap ? (
    <CheckIcon sx={{ fontSize: '14px' }} />
  ) : (
    <CloseIcon sx={{ fontSize: '14px' }} />
  )
}

const DescriptionText = ({ text }: { text: string }) => {
  return <Typography sx={{ m: 1, fontSize: '12px' }}>{text}</Typography>
}

interface LinkEditorProps {
  link: RouteLocation
}

const LinkEditor = ({ link }: LinkEditorProps) => {
  return (
    <Paper>
      <ButtonBase
        sx={{
          cursor: 'pointer',
          textTransform: 'none',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          width: '100%',
          padding: 1,
        }}
      >
        <Box sx={{ flex: 1, textAlign: 'left' }}>
          <Typography variant="h5" sx={{ padding: 1, marginRight: 2 }}>
            {link.locationIdentifier} - {link.primaryName} &{' '}
            {link.secondaryName}
          </Typography>
        </Box>

        <Box sx={{ display: 'flex' }}>
          <Grid container spacing={1} marginX={1} alignItems="center">
            <Grid item xs={4}>
              <DescriptionText text={link.primaryDirectionId} />
            </Grid>
            <Grid item xs={4}>
              <DescriptionText text={link.primaryPhase} />
            </Grid>
            <Grid item xs={4}>
              <Typography>
                <OverlapIcon isOverlap={link.isPrimaryOverlap} />
              </Typography>
            </Grid>
          </Grid>
          <Grid container spacing={1} marginX={1} alignItems="center">
            <Grid item xs={4}>
              <DescriptionText text={link.opposingDirectionId} />
            </Grid>
            <Grid item xs={4}>
              <DescriptionText text={link.opposingPhase} />
            </Grid>
            <Grid item xs={4}>
              <OverlapIcon isOverlap={link.isPrimaryOverlap} />
            </Grid>
          </Grid>
        </Box>

        <Box marginLeft={3}>
          <IconButton aria-label="delete" color="error">
            <DeleteOutlineIcon />
          </IconButton>
        </Box>
      </ButtonBase>
    </Paper>
  )
}

export default LinkEditor
