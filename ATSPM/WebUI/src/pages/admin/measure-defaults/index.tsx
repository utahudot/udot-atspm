import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useChartDefaults, useUpdateChartDefaults } from '@/features/charts/api'
import { ChartType, chartTypeToString } from '@/features/charts/common/types'
import { chartComponents } from '@/features/charts/components/selectChart'
import { Default } from '@/features/charts/types'
import { useUserHasClaim, PageNames, useViewPage } from '@/features/identity/pagesCheck'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  Divider,
  Typography,
} from '@mui/material'


const MeasureDefaults = () => {
  const pageAccess = useViewPage(PageNames.MeasureDefaults) 

  const { data: chartDefaultsData, isLoading } = useChartDefaults()
  const { mutate } = useUpdateChartDefaults()
  const hasEditClaim = useUserHasClaim('GeneralConfiguration:Edit');

  if (pageAccess.isLoading) {
    return
  }

  const chartDefaults = chartDefaultsData?.value

  const getChartDefaults = (chartType: string) => {
    return chartDefaults?.find((chart) => chart.chartType === chartType)
      ?.measureOptions
  }

  const handleChartOptionsUpdate = (update: Default) => {
    mutate({ value: update.value, id: update.id })
  }
 
  if (isLoading) return <div>Loading...</div>

  const renderChartOptionsComponent = (chartType: ChartType) => {
    const ChartComponent =
      chartComponents[chartType as keyof typeof chartComponents]
    const chartDefaults = getChartDefaults(
      chartType as keyof typeof chartComponents
    )

    if (!ChartComponent) return null

    if (isLoading) return <div>Loading...</div>

    if (!ChartComponent || chartDefaults === undefined) return null

    return (
      <ChartComponent
        chartDefaults={chartDefaults}
        handleChartOptionsUpdate={handleChartOptionsUpdate}
      />
    )
  }

  return (
    <>
      <ResponsivePageLayout title={'Measure Defaults'}>
        {Object.entries(chartComponents).map(([type]) => (
          <Accordion key={type}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography sx={{ fontWeight: 'bold', fontSize: '18px' }}>
                {chartTypeToString(type as ChartType)}
              </Typography>
            </AccordionSummary>
            <Divider />
            <AccordionDetails>
              {/* The Timing and Actuation should not have any defaults */}
              {type === ChartType.TimingAndActuation ? (
                <Box
                  key={type}
                  sx={{
                    padding: '0.25rem',
                  }}
                >
                  <Typography>No options available for this chart.</Typography>
                </Box>
              ) : (
                renderChartOptionsComponent(type as ChartType)
              )}
            </AccordionDetails>
          </Accordion>
        ))}
      </ResponsivePageLayout>
    </>
  )
}

export default MeasureDefaults
