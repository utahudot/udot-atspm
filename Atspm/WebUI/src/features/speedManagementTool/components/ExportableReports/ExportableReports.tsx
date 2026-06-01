import {
  ExportableReportOptions,
  SpeedDataType,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { Box, useTheme } from '@mui/material'
import { RoutesResponse } from '../../types/routes'
import { ReportDisplayComponent } from './ReportComponent'
import { MonthlyReportOptionsComponent } from './components/MonthlyReportOptionsComponent'
import { useERBaseHandler } from './components/handlers/ExportableReportsHandler'
import {
  useERMonthlyReportHandler,
  useExportableReportHandler,
} from './components/handlers/handlers'

export const ExportableReports = () => {
  const theme = useTheme()
  const reportsHandler = useERBaseHandler()
  const hourlyHandler = useExportableReportHandler({
    baseHandler: reportsHandler,
  })
  const monthlyHandler = useERMonthlyReportHandler({
    baseHandler: reportsHandler,
  })

  const currentHandler =
    reportsHandler.speedDataType === SpeedDataType.H
      ? hourlyHandler
      : monthlyHandler

  return (
    <Box>
      {/* <Box
        sx={{
          padding: 2,
          gap: 2,
          alignItems: 'center',
          flexWrap: 'wrap',
        }}
      > */}
      {
        //reportsHandler.speedDataType === SpeedDataType.H ? (
        //<HourlyReportOptionsComponent handler={hourlyHandler} />
        //) : (
        <MonthlyReportOptionsComponent handler={monthlyHandler} />
        // )
      }
      {/* </Box> */}
      {!currentHandler.isPdfDownloading && reportsHandler.options && (
        <Box
          sx={{
            marginTop: theme.spacing(2),
            padding: 2,
            gap: 2,
          }}
        >
          <ReportDisplayComponent
            data={reportsHandler.reportData}
            routeSpeeds={reportsHandler.routeSpeeds as RoutesResponse}
            options={reportsHandler.options as ExportableReportOptions}
            handler={currentHandler}
          />
        </Box>
      )}
    </Box>
  )
}
