import { TransformedChartResponse } from '@/features/charts/types'
import ArrowDropDownIcon from '@mui/icons-material/ArrowDropDown'
import ArrowDropUpIcon from '@mui/icons-material/ArrowDropUp'
import VisibilityOffOutlinedIcon from '@mui/icons-material/VisibilityOffOutlined'
import VisibilityOutlinedIcon from '@mui/icons-material/VisibilityOutlined'
import {
  Button,
  ClickAwayListener,
  IconButton,
  List,
  ListItem,
  Typography,
  useTheme,
} from '@mui/material'
import { useState } from 'react'

interface ChartListProps {
  charts: TransformedChartResponse[]
  chartRefs: React.RefObject<HTMLDivElement>[]
  isDisabled: boolean
}

function IndividualChartControls({ charts, chartRefs, isDisabled }: ChartListProps) {
  const theme = useTheme()
  const [dropdownOpen, setDropdownOpen] = useState(false)

  const handleDropdownToggle = () => {
    setDropdownOpen(!dropdownOpen)
  }

  const handleCloseDropdown = () => {
    setDropdownOpen(false)
  }

  const [chartsVisibility, setChartsVisibility] = useState(
    charts.map(() => true)
  )

  const scrollToChart = (index: number) => {
    const chartRef = chartRefs[index]
    if (!chartRef?.current) return
    const chartPosition =
      chartRef.current.getBoundingClientRect().top + window.scrollY
    const offset = window.innerHeight / 2 - chartRef.current.offsetHeight / 2
    const scrollPosition = chartPosition - offset
    window.scrollTo({ top: scrollPosition, behavior: 'smooth' })
  }

  const toggleChartVisibility = (index: number) => {
    const newVisibility = [...chartsVisibility]
    newVisibility[index] = !newVisibility[index]
    setChartsVisibility(newVisibility)

    const chartRef = chartRefs[index]
    if (!chartRef?.current) return
    const isCurrentlyVisible = newVisibility[index]
    const targetHeight = isCurrentlyVisible ? '1000px' : '0px'
    chartRef.current.style.maxHeight = targetHeight
  }

  return (
    <>
      <Button
        aria-label="more"
        aria-haspopup="true"
        onClick={handleDropdownToggle}
        disabled={isDisabled}

        endIcon={dropdownOpen ? <ArrowDropUpIcon /> : <ArrowDropDownIcon />}
        sx={{
          mx: '2px',
          color: theme.palette.text.primary,
          textTransform: 'none',
          '& .MuiButton-endIcon': { ml: '0px' },
        }}
      >
        <Typography
          fontWeight={400}
          fontSize={'.8rem'}
          sx={{ textTransform: 'none' }}
        >
          Charts
        </Typography>
      </Button>

      {dropdownOpen ? (
        <ClickAwayListener onClickAway={handleCloseDropdown}>
          <List
            sx={{
              position: 'absolute',
              top: '100%',
              right: 0,
              backgroundColor: theme.palette.background.paper,
              boxShadow: 2,
              borderRadius: '4px',
              zIndex: 2000,
            }}
          >
            {charts.map((chartWrapper, index) => (
              <ListItem
                key={index}
                sx={{ display: 'flex', justifyContent: 'space-between' }}
              >
                <IconButton onClick={() => toggleChartVisibility(index)}>
                  {chartsVisibility[index] ? (
                    <VisibilityOutlinedIcon fontSize="small" />
                  ) : (
                    <VisibilityOffOutlinedIcon fontSize="small" />
                  )}
                </IconButton>
                <Button
                  aria-label="more"
                  aria-haspopup="true"
                  onClick={() => scrollToChart(index)}
                  sx={{
                    mx: '2px',
                    color: theme.palette.text.primary,
                    textTransform: 'none',
                    '& .MuiButton-endIcon': { ml: '0px' },
                  }}
                >
                  <Typography
                    fontWeight={400}
                    fontSize={'.8rem'}
                    sx={{ textTransform: 'none' }}
                  >
                    {chartWrapper.chart.displayProps.description}
                  </Typography>
                </Button>
              </ListItem>
            ))}
          </List>
        </ClickAwayListener>
      ) : null}
    </>
  )
}

export default IndividualChartControls
