import { useNotificationStore } from '@/stores/notifications'
import ContentCopyIcon from '@mui/icons-material/ContentCopy'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import React from 'react'

interface SplitMonitorPlan {
  planNumber: string
  planDescription: string
  start: string
  end: string
  percentSkips: number
  percentGapOuts: number
  percentMaxOuts: number
  percentForceOffs: number
  averageSplit: number
  minTime: number
  programmedSplit: number
  percentileSplit85th: number
  percentileSplit50th: number
}

interface ChartResults {
  chart: {
    displayProps: {
      plans: SplitMonitorPlan[]
      phaseNumber: string
    }
  }
}

interface PhaseTableProps {
  phases: ChartResults[]
}

const PhaseTable = ({ phases }: PhaseTableProps) => {
  const { addNotification } = useNotificationStore()
  if (phases.length === 0) {
    return null
  }
  const createHeaderLabel = (planNumber: string) => {
    return planNumber === '254' ? 'Free' : `Plan ${planNumber}`
  }

  const resolveForceOffsOrMaxOuts = (plan: SplitMonitorPlan) => {
    return (
      <BorderedCell color={plan.planNumber === '254' ? 'red' : 'blue'}>
        {formatNumber(plan.percentForceOffs)}
      </BorderedCell>
    )
  }

  const BorderedCell = ({
    children,
    borderBottom,
    color,
    sx,
  }: {
    children: React.ReactNode
    borderBottom?: boolean
    color?: string
    sx?: any
  }) => (
    <TableCell
      sx={{
        color: color || 'inherit',
        border: '1px solid rgb(224, 224, 224)',
        fontSize: '0.8rem',
        borderBottom: borderBottom
          ? '2px solid rgb(200, 200, 200)'
          : '1px solid rgb(224, 224, 224)',
        ...sx,
      }}
    >
      {children}
    </TableCell>
  )

  const formatNumber = (value: string | number) => {
    if (
      value === '' ||
      value === undefined ||
      value === null ||
      typeof value === 'string'
    ) {
      return value === undefined || value === null ? '' : value
    }
    const numberValue = parseFloat(value)
    if (Number.isNaN(numberValue)) {
      return ''
    }
    if (Number.isInteger(numberValue)) {
      return numberValue
    } else {
      const rounded = Math.round(numberValue * 10) / 10
      return rounded % 1 === 0 ? rounded : rounded.toFixed(1)
    }
  }

  const createClipboardText = () => {
    let clipboardText = 'Phase\tMetric'
    phases[0].chart.displayProps.plans.forEach((plan) => {
      clipboardText += `\t${
        plan.planNumber === '254' ? 'Free' : `Plan ${plan.planNumber}`
      }`
    })
    clipboardText += '\n'

    const metrics = [
      { key: 'start', label: 'Time', formatter: formatDateTime },
      { key: 'minTime', label: 'Min Time', formatter: formatNumber },
      {
        key: 'programmedSplit',
        label: 'Programmed Split (sec)',
        formatter: formatNumber,
      },
      {
        key: 'percentileSplit85th',
        label: '85th Percentile Split (sec)',
        formatter: formatNumber,
      },
      {
        key: 'percentileSplit50th',
        label: '50th Percentile Split (sec)',
        formatter: formatNumber,
      },
      {
        key: 'averageSplit',
        label: 'Average Split (sec)',
        formatter: formatNumber,
      },
      {
        key: 'percentForceOffs',
        label: 'Force Offs or Max Outs (%)',
        formatter: formatNumber,
        specialCondition: (plan: SplitMonitorPlan) => plan.planNumber === '254',
      },
      { key: 'percentGapOuts', label: 'Gap Outs (%)', formatter: formatNumber },
      { key: 'percentSkips', label: 'Skips (%)', formatter: formatNumber },
    ]

    phases.forEach((phase) => {
      metrics.forEach((metric) => {
        clipboardText += `${phase.chart.displayProps.phaseNumber}\t${metric.label}`
        phase.chart.displayProps.plans.forEach((plan) => {
          let value
          if (metric.key === 'start') {
            // For the time metric, pass both start and end times
            value = metric.formatter(plan.start, plan.end)
          } else {
            value = plan[metric.key]
            if (metric.specialCondition?.(plan)) {
              value = plan['percentMaxOuts']
            }
            value =
              value !== undefined
                ? typeof metric.formatter === 'function'
                  ? metric.formatter(value)
                  : value
                : ''
          }
          clipboardText += `\t${value}`
        })
        clipboardText += '\n'
      })
    })

    return clipboardText
  }

  const handleCopyToClipboard = async (e) => {
    e.stopPropagation()
    const clipboardText = createClipboardText()

    try {
      await navigator.clipboard.writeText(clipboardText)
      addNotification({
        title: 'Table data copied to clipboard',
        type: 'info',
      })
    } catch (err) {
      console.error('Failed to copy text: ', err)
    }
  }

  const formatDateTime = (start, end) => {
    const formatTime = (time) => {
      if (!time) {
        return '00:00' // Default time if not specified
      }
      try {
        return new Intl.DateTimeFormat('default', {
          hour: '2-digit',
          minute: '2-digit',
          hour12: false,
        }).format(new Date(time))
      } catch (error) {
        console.error('Error formatting time:', error)
        return '00:00' // Fallback time on error
      }
    }

    // Handle cases where start or end times might be missing
    const startTimeFormatted = formatTime(start)
    const endTimeFormatted = formatTime(end)

    return `${startTimeFormatted} - ${endTimeFormatted}`
  }

  const maxPhase = phases.reduce((acc, current) =>
    current.chart.displayProps.plans.length >
    acc.chart.displayProps.plans.length
      ? current
      : acc
  )

  const syncedPhases = phases.map((phase) => {
    const syncedPlans = maxPhase.chart.displayProps.plans.map((plan) => {
      const matchingPlan = phase.chart.displayProps.plans.find(
        (p) => p.start === plan.start
      )
      return (
        matchingPlan || {
          planNumber: '',
          start: '',
          end: '',
          percentSkips: '',
          percentGapOuts: '',
          percentMaxOuts: '',
          percentForceOffs: '',
          averageSplit: '',
          minTime: '',
          programmedSplit: '',
          percentileSplit85th: '',
          percentileSplit50th: '',
        }
      )
    })
    return {
      ...phase,
      chart: {
        ...phase.chart,
        displayProps: { ...phase.chart.displayProps, plans: syncedPlans },
      },
    }
  })

  return (
    <Accordion disableGutters>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <Typography variant="h4">Phase Details</Typography>
          <IconButton
            color="primary"
            onClick={(e) => handleCopyToClipboard(e)}
            sx={{ ml: 2 }}
            size="small"
            aria-label="Copy to Clipboard"
          >
            <ContentCopyIcon />
          </IconButton>
        </Box>
      </AccordionSummary>
      <AccordionDetails>
        <Box sx={{ width: '100%', overflowX: 'auto' }}>
          <Table stickyHeader size="small">
            <TableHead>
              <TableRow>
                <BorderedCell>Phase</BorderedCell>
                <BorderedCell>Metric</BorderedCell>
                {maxPhase.chart.displayProps.plans.map((plan) => (
                  <BorderedCell key={plan.planNumber}>
                    {createHeaderLabel(plan.planNumber)}
                  </BorderedCell>
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {syncedPhases.map((phase) => (
                <React.Fragment
                  key={`phase-${phase.chart.displayProps.phaseNumber}`}
                >
                  <TableRow>
                    <BorderedCell>
                      {phase.chart.displayProps.phaseNumber}
                    </BorderedCell>
                    <BorderedCell>Time</BorderedCell>
                    {phase.chart.displayProps.plans.map((plan) => (
                      <BorderedCell key={`time-${plan.planNumber}`}>
                        {`${formatDateTime(plan.start, plan.end)} `}
                      </BorderedCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <BorderedCell>
                      {phase.chart.displayProps.phaseNumber}
                    </BorderedCell>
                    <BorderedCell>Min Time</BorderedCell>
                    {phase.chart.displayProps.plans.map((plan) => (
                      <BorderedCell key={`minTime-${plan.minTime}`}>
                        {plan.minTime}
                      </BorderedCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <BorderedCell>
                      {phase.chart.displayProps.phaseNumber}
                    </BorderedCell>
                    <BorderedCell>Programmed Split (sec)</BorderedCell>
                    {phase.chart.displayProps.plans.map((plan) => (
                      <BorderedCell
                        key={`programmedSplit-${plan.programmedSplit}`}
                      >
                        {plan.programmedSplit}
                      </BorderedCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <BorderedCell>
                      {phase.chart.displayProps.phaseNumber}
                    </BorderedCell>
                    <BorderedCell color="purple">
                      85th percentile split (sec)
                    </BorderedCell>
                    {phase.chart.displayProps.plans.map((plan) => (
                      <BorderedCell
                        key={`percentileSplit85th-${plan.percentileSplit85th}`}
                        color="purple"
                      >
                        {formatNumber(plan.percentileSplit85th)}
                      </BorderedCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <BorderedCell color="purple">
                      {phase.chart.displayProps.phaseNumber}
                    </BorderedCell>
                    <BorderedCell color="purple">
                      50th percentile split (sec)
                    </BorderedCell>
                    {phase.chart.displayProps.plans.map((plan) => (
                      <BorderedCell
                        key={`percentileSplit50th-${plan.percentileSplit50th}`}
                        color="purple"
                      >
                        {formatNumber(plan.percentileSplit50th)}
                      </BorderedCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <BorderedCell>
                      {phase.chart.displayProps.phaseNumber}
                    </BorderedCell>
                    <BorderedCell>Average Split (sec)</BorderedCell>
                    {phase.chart.displayProps.plans.map((plan) => (
                      <BorderedCell key={`averageSplit-${plan.averageSplit}`}>
                        {formatNumber(plan.averageSplit)}
                      </BorderedCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <BorderedCell>
                      {phase.chart.displayProps.phaseNumber}
                    </BorderedCell>
                    <BorderedCell>
                      <span style={{ color: 'blue' }}>Force Offs</span> or{' '}
                      <span style={{ color: 'red' }}>Max Outs</span> (%)
                    </BorderedCell>
                    {phase.chart.displayProps.plans.map((plan) =>
                      resolveForceOffsOrMaxOuts(plan)
                    )}
                  </TableRow>
                  <TableRow>
                    <BorderedCell>
                      {phase.chart.displayProps.phaseNumber}
                    </BorderedCell>
                    <BorderedCell color="green">Gap Outs (%)</BorderedCell>
                    {phase.chart.displayProps.plans.map((plan) => (
                      <BorderedCell
                        key={`percentGapOuts-${plan.percentGapOuts}`}
                        color="green"
                      >
                        {formatNumber(plan.percentGapOuts)}
                      </BorderedCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <BorderedCell borderBottom>
                      {phase.chart.displayProps.phaseNumber}
                    </BorderedCell>
                    <BorderedCell borderBottom>Skips (%)</BorderedCell>
                    {phase.chart.displayProps.plans.map((plan) => (
                      <BorderedCell
                        key={`percentSkips-${plan.percentSkips}`}
                        borderBottom
                      >
                        {formatNumber(plan.percentSkips)}
                      </BorderedCell>
                    ))}
                  </TableRow>
                </React.Fragment>
              ))}
            </TableBody>
          </Table>
        </Box>
      </AccordionDetails>
    </Accordion>
  )
}

export default PhaseTable
