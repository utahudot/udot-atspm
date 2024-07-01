import {
  StyledComponentHeader,
  commonPaperStyle,
} from '@/components/HeaderStyling/StyledComponentHeader'
import { Box, Paper, TextField, Typography } from '@mui/material'

const textFieldStyle = {
  width: '40px', // Set the width to 60px
  height: '30px', // Set the height to 60px
  '& input': {
    height: '30px',
    padding: '0', // Adjust padding to center the text vertically
    textAlign: 'center', // Center the text horizontally
    fontSize: '.75rem',
  },
  '& input[type=number]': {
    //hides the up and down arrows in the TextField
    '-moz-appearance': 'textfield',
    '&::-webkit-outer-spin-button': {
      '-webkit-appearance': 'none',
      margin: 0,
    },
    '&::-webkit-inner-spin-button': {
      '-webkit-appearance': 'none',
      margin: 0,
    },
  },
  '& label': {
    display: 'none', // Hide the label visually
  },
}

interface LocationDataCheckPros {
  cyclesWithPedCalls: number
  setCyclesWithPedCalls: React.Dispatch<React.SetStateAction<number>>
  cyclesWithGapOuts: number
  setCyclesWithGapOuts: React.Dispatch<React.SetStateAction<number>>
  leftTurnVolume: number
  setLeftTurnVolume: React.Dispatch<React.SetStateAction<number>>
}

export const LocationDataCheck = ({
  cyclesWithPedCalls,
  setCyclesWithPedCalls,
  cyclesWithGapOuts,
  setCyclesWithGapOuts,
  leftTurnVolume,
  setLeftTurnVolume,
}: LocationDataCheckPros) => {
  return (
    <Paper
      sx={{
        ...commonPaperStyle,
        '@media (max-width: 1200px)': {
          marginBottom: 1,
        },
      }}
    >
      <Box>
        <StyledComponentHeader header={'Location Data Check'} />
      </Box>
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-around',
          padding: '10px',
          height: '190px',
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <TextField
            id="cycles-with-ped-outs"
            value={cyclesWithPedCalls}
            type="number"
            onChange={(e) =>
              setCyclesWithPedCalls(
                (e.target as HTMLInputElement).valueAsNumber
              )
            }
            sx={{ ...textFieldStyle }}
          />
          <label htmlFor="cycles-with-ped-outs">% Cycles with Ped Calls</label>
          <Typography>% Cycles with Ped Calls</Typography>
        </Box>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <TextField
            id="cycles-with-gap-outs"
            type="number"
            value={cyclesWithGapOuts}
            onChange={(e) =>
              setCyclesWithGapOuts((e.target as HTMLInputElement).valueAsNumber)
            }
            sx={{ ...textFieldStyle }}
          />
          <label htmlFor="cycles-with-gap-outs">% Cycles with Gap Outs</label>
          {/* <Typography>% Cycles with Gap Outs</Typography> */}
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <TextField
            id="left-turn-volume"
            type="number"
            value={leftTurnVolume}
            onChange={(e) =>
              setLeftTurnVolume((e.target as HTMLInputElement).valueAsNumber)
            }
            sx={{ ...textFieldStyle }}
          />
          <label htmlFor="left-turn-volume">Left-turn Volume (vph)</label>
        </Box>
      </Box>
    </Paper>
  )
}
