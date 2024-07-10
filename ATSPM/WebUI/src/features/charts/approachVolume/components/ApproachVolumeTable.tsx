import { ApproachVolumeSummaryData } from '@/features/charts/approachVolume/types'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Grid,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
  useTheme,
} from '@mui/material'

type ApproachVolumeTableProps = {
  data: ApproachVolumeSummaryData
}

const defaultDecimalPoints = 3

export function ApproachVolumeTable({ data }: ApproachVolumeTableProps) {
  const theme = useTheme()
  return (
    <Accordion
      disableGutters
      elevation={0}
      sx={{
        borderTop: '1px solid',
        marginTop: 3,
        '&:before': {
          display: 'none',
        },
      }}
    >
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>
        <Typography variant="h4">Details</Typography>
      </AccordionSummary>
      <AccordionDetails>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Table sx={{ boxShadow: '1' }}>
              <TableHead>
                <TableRow>
                  <TableCell></TableCell>
                  <TableCell>Total</TableCell>
                  <TableCell>{data.primaryDirectionName}</TableCell>
                  <TableCell>{data.opposingDirectionName}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                <TableRow
                  sx={{ backgroundColor: theme.palette.background.default }}
                >
                  <TableCell variant="head">Peak Hour</TableCell>
                  <TableCell>{data.peakHour}</TableCell>
                  <TableCell>{data.primaryPeakHour}</TableCell>
                  <TableCell>{data.opposingPeakHour}</TableCell>
                </TableRow>
                <TableRow>
                  <TableCell variant="head">Peak Hour K Factor</TableCell>
                  <TableCell>
                    {data.kFactor.toFixed(defaultDecimalPoints)}
                  </TableCell>
                  <TableCell>
                    {data.primaryKFactor.toFixed(defaultDecimalPoints)}
                  </TableCell>
                  <TableCell>
                    {data.opposingKFactor.toFixed(defaultDecimalPoints)}
                  </TableCell>
                </TableRow>
                <TableRow
                  sx={{ backgroundColor: theme.palette.background.default }}
                >
                  <TableCell variant="head">Peak Hour D Factor</TableCell>
                  <TableCell>-</TableCell>
                  <TableCell>
                    {data.primaryDFactor.toFixed(defaultDecimalPoints)}
                  </TableCell>
                  <TableCell>
                    {data.opposingDFactor.toFixed(defaultDecimalPoints)}
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell variant="head">Peak Hour Volume</TableCell>
                  <TableCell>{data.peakHourVolume.toLocaleString()}</TableCell>
                  <TableCell>
                    {data.primaryPeakHourVolume.toLocaleString()}
                  </TableCell>
                  <TableCell>
                    {data.opposingPeakHourVolume.toLocaleString()}
                  </TableCell>
                </TableRow>
                <TableRow
                  sx={{ backgroundColor: theme.palette.background.default }}
                >
                  <TableCell variant="head">Peak Hour Factor</TableCell>
                  <TableCell>
                    {data.peakHourFactor.toFixed(defaultDecimalPoints)}
                  </TableCell>
                  <TableCell>
                    {data.primaryPeakHourFactor.toFixed(defaultDecimalPoints)}
                  </TableCell>
                  <TableCell>
                    {data.opposingPeakHourFactor.toFixed(defaultDecimalPoints)}
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell variant="head">Total Volume</TableCell>
                  <TableCell>{data.totalVolume.toLocaleString()}</TableCell>
                  <TableCell>
                    {data.primaryTotalVolume.toLocaleString()}
                  </TableCell>
                  <TableCell>
                    {data.opposingTotalVolume.toLocaleString()}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </Grid>
        </Grid>
      </AccordionDetails>
    </Accordion>
  )
}
