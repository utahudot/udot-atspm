import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Box,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  useTheme,
} from '@mui/material'
import { Fragment, useState } from 'react'
import { PieChart } from '../../helpers/pieChart'
import { LinkPivotHandler } from '../handlers/linkPivotHandlers'
import { ApproachLinksDto, CorridorSummary } from '../types'
import { LinkPivotPcdComponent } from './LinkPivotPcdComponent'

interface props {
  data: ApproachLinksDto[]
  corridorSummary: CorridorSummary
  lpHandler: LinkPivotHandler
}

export const LinkPivotApproachLinkComponent = ({
  data,
  corridorSummary,
  lpHandler,
}: props) => {
  const theme = useTheme()
  const [rows, setRows] = useState(data)
  const [expanded, setExpanded] = useState(rows.map(() => false))
  const handleIconClick = (
    event: { stopPropagation: () => void },
    index: number
  ) => {
    setExpanded((prevArr) =>
      prevArr.map((item, i: number) => (i === index ? !item : item))
    )
    event.stopPropagation()
  }

  const createCells = (
    totalAogBefore: number,
    totalPaogBefore: number,
    totalAogPredicted: number,
    totalPaogPredicted: number,
    existing: number,
    positive: number,
    negative: number,
    remaining: number
  ) => {
    return (
      <>
        <TableCell>
          {totalAogBefore} ({totalPaogBefore}%)
        </TableCell>
        <TableCell>
          {totalAogPredicted} ({totalPaogPredicted}%)
        </TableCell>
        <TableCell width="fit-content">
          <PieChart
            id={'upstream'}
            existing={existing}
            positive={positive}
            negative={negative}
            remaining={remaining}
          />
        </TableCell>
      </>
    )
  }

  return (
    <Box>
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell rowSpan={2}>Link</TableCell>
              <TableCell align="center" colSpan={2}>
                Approaches
              </TableCell>
              <TableCell align="center" colSpan={3}>
                Upstream AOG
              </TableCell>
              <TableCell align="center" colSpan={3}>
                Downstream AOG
              </TableCell>
              <TableCell align="center" colSpan={3}>
                Total Link AOG
              </TableCell>
              <TableCell align="center" rowSpan={2}>
                Delta
              </TableCell>
            </TableRow>
            <TableRow>
              <TableCell width="12%">Upstream</TableCell>
              <TableCell width="12%">Downstream</TableCell>
              <TableCell width="7%">Existing</TableCell>
              <TableCell width="7%">Predicted</TableCell>
              <TableCell width="7%">Change</TableCell>
              <TableCell width="7%">Existing</TableCell>
              <TableCell width="7%">Predicted</TableCell>
              <TableCell width="7%">Change</TableCell>
              <TableCell width="7%">Existing</TableCell>
              <TableCell width="7%">Predicted</TableCell>
              <TableCell width="7%">Change</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.map((row, index) => (
              <Fragment key={index}>
                <TableRow
                  key={index}
                  sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                >
                  <TableCell>{row.linkNumber}</TableCell>
                  <TableCell>{row.location}</TableCell>
                  <TableCell>{row.downstreamLocation}</TableCell>
                  {createCells(
                    row.aogUpstreamBefore,
                    row.paogUpstreamBefore,
                    row.aogUpstreamPredicted,
                    row.paogUpstreamPredicted,
                    row.upstreamChartExisting,
                    row.upstreamChartPositiveChange,
                    row.upstreamChartNegativeChange,
                    row.upstreamChartRemaining
                  )}
                  {createCells(
                    row.aogDownstreamBefore,
                    row.paogDownstreamBefore,
                    row.aogDownstreamPredicted,
                    row.paogDownstreamPredicted,
                    row.downstreamChartExisting,
                    row.downstreamChartPositiveChange,
                    row.downstreamChartNegativeChange,
                    row.downstreamChartRemaining
                  )}
                  {createCells(
                    row.aogTotalBefore,
                    row.pAogTotalBefore,
                    row.aogTotalPredicted,
                    row.pAogTotalPredicted,
                    row.totalChartExisting,
                    row.totalChartPositiveChange,
                    row.totalChartNegativeChange,
                    row.totalChartRemaining
                  )}
                  <TableCell align="center">{row.delta}</TableCell>
                  <TableCell>
                    <Box display="flex" alignItems="center">
                      <span>View PCD Options</span>
                      <IconButton
                        aria-label={
                          expanded[index]
                            ? 'Collapse PCD Options'
                            : 'Expand PCD Options'
                        }
                        onClick={(event) => handleIconClick(event, index)}
                        sx={{ cursor: 'pointer' }}
                      >
                        {expanded[index] ? (
                          <ExpandMoreIcon />
                        ) : (
                          <ExpandLessIcon />
                        )}
                      </IconButton>
                    </Box>
                  </TableCell>
                </TableRow>
                {expanded[index] && (
                  <TableRow key={`pcdChart ${index}`}>
                    <TableCell
                      align="center"
                      colSpan={14}
                      sx={{ backgroundColor: theme.palette.background.default }}
                    >
                      <LinkPivotPcdComponent
                        pcdDto={row}
                        lpHandler={lpHandler}
                      />
                    </TableCell>
                  </TableRow>
                )}
              </Fragment>
            ))}
            <TableRow>
              <TableCell colSpan={3} align="center">
                Corridor Summary
              </TableCell>
              {createCells(
                corridorSummary.totalAogUpstreamBefore,
                corridorSummary.totalPaogUpstreamBefore,
                corridorSummary.totalAogUpstreamPredicted,
                corridorSummary.totalPaogUpstreamPredicted,
                corridorSummary.totalUpstreamChartExisting,
                corridorSummary.totalUpstreamChartPositiveChange,
                corridorSummary.totalUpstreamChartNegativeChange,
                corridorSummary.totalUpstreamChartRemaining
              )}
              {createCells(
                corridorSummary.totalAogDownstreamBefore,
                corridorSummary.totalPaogDownstreamBefore,
                corridorSummary.totalAogDownstreamPredicted,
                corridorSummary.totalPaogDownstreamPredicted,
                corridorSummary.totalDownstreamChartExisting,
                corridorSummary.totalDownstreamChartPositiveChange,
                corridorSummary.totalDownstreamChartNegativeChange,
                corridorSummary.totalDownstreamChartRemaining
              )}
              {createCells(
                corridorSummary.totalAogBefore,
                corridorSummary.totalPaogBefore,
                corridorSummary.totalAogPredicted,
                corridorSummary.totalPaogPredicted,
                corridorSummary.totalChartExisting,
                corridorSummary.totalChartPositiveChange,
                corridorSummary.totalChartNegativeChange,
                corridorSummary.totalChartRemaining
              )}
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  )
}
