import { TimingAndActuationChartOptionsDefaults } from '@/features/charts/timingAndActuation/types'
import { Default } from '@/features/charts/types'
import { Box, Typography } from '@mui/material'

interface TimingAndActuationChartOptionsProps {
  chartDefaults: TimingAndActuationChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
  isMeasureDefaultView?: boolean
}

export const TimingAndActuationChartOptions = ({
  // chartDefaults,
  handleChartOptionsUpdate,
  isMeasureDefaultView = false,
}: TimingAndActuationChartOptionsProps) => {
  //////////////////////////////
  // It sounds like we'll add this back in later, so I'm leaving the code here for now.
  //////////////////////////////

  // const [phaseEventCodesList, setPhaseEventCodesList] = useState('')
  // const [globalEventCodesList, setGlobalEventCodesList] = useState('')
  // const [globalEventParamsList, setGlobalEventParamsList] = useState('')

  // const transformInputToArray = (input: string): number[] => {
  //   return input
  //     .split(',')
  //     .map((item) => parseInt(item, 10))
  //     .filter((item) => !isNaN(item))
  // }

  // const handlePhaseEventCodesListChange = (
  //   event: ChangeEvent<HTMLInputElement>
  // ) => {
  //   const newValue = event.target.value
  //   setPhaseEventCodesList(newValue)

  //   handleChartOptionsUpdate({
  //     id: 0,
  //     option: 'phaseEventCodesList',
  //     value: transformInputToArray(newValue),
  //   })
  // }

  // const handleGlobalEventCodesListChange = (
  //   event: ChangeEvent<HTMLInputElement>
  // ) => {
  //   const newValue = event.target.value
  //   setGlobalEventCodesList(newValue)

  //   handleChartOptionsUpdate({
  //     id: 0,
  //     option: 'globalEventCodesList',
  //     value: transformInputToArray(newValue),
  //   })
  // }

  // const handleGlobalEventParamsListChange = (
  //   event: ChangeEvent<HTMLInputElement>
  // ) => {
  //   const newValue = event.target.value
  //   setGlobalEventParamsList(newValue)

  //   handleChartOptionsUpdate({
  //     id: 0,
  //     option: 'globalEventParamsList',
  //     value: transformInputToArray(newValue),
  //   })
  // }

  // return (
  //   <>
  //     <Box
  //       sx={{
  //         display: 'flex',
  //         justifyContent: 'space-between',
  //         padding: '0.25rem',
  //       }}
  //     >
  //       <Typography>Phase Event Codes</Typography>
  //       <FormControl variant="standard" sx={{ width: '120px' }}>
  //         <TextField
  //           variant="standard"
  //           value={phaseEventCodesList}
  //           onChange={handlePhaseEventCodesListChange}
  //           placeholder="1,2,3"
  //         />
  //       </FormControl>
  //     </Box>
  //     <Box
  //       sx={{
  //         display: 'flex',
  //         justifyContent: 'space-between',
  //         padding: '0.25rem',
  //       }}
  //     >
  //       <Typography>Global Event Codes</Typography>
  //       <FormControl variant="standard" sx={{ width: '120px' }}>
  //         <TextField
  //           variant="standard"
  //           value={globalEventCodesList}
  //           onChange={handleGlobalEventCodesListChange}
  //           placeholder="1,2,3"
  //         />
  //       </FormControl>
  //     </Box>
  //     <Box
  //       sx={{
  //         display: 'flex',
  //         justifyContent: 'space-between',
  //         padding: '0.25rem',
  //       }}
  //     >
  //       <Typography>Global Event Params</Typography>
  //       <FormControl variant="standard" sx={{ width: '120px' }}>
  //         <TextField
  //           variant="standard"
  //           value={globalEventParamsList}
  //           onChange={handleGlobalEventParamsListChange}
  //           placeholder="1,2,3"
  //         />
  //       </FormControl>
  //     </Box>
  //   </>
  // )

  return (
    <Box
      sx={{
        padding: '0.25rem',
      }}
    >
      <Typography>No options available for this chart.</Typography>
    </Box>
  )
}
