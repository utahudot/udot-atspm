import { ChangeEvent } from 'react'
import { StyledPaper } from '@/components/StyledPaper'
import CustomSelect from '@/components/customSelect'
import { SingularListDataItems } from '@/components/customSelect/CustomSelect'
import SelectDateTime from '@/components/selectTimeSpan'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import {
  Box,
  FormControl,
  FormControlLabel,
  Grid,
  Paper,
  Radio,
  RadioGroup,
  SelectChangeEvent,
  Slider,
  Typography,
} from '@mui/material'
import { AggregateOptionsHandler } from '../handlers/aggregateDataHandler'
import {
  MetricTypeOptionsList,
  YAxisOptions,
  binSizeMarks,
  chartTypeOptions,
  xAxisOptions,
} from '../types/aggregateOptionsData'
import {
  AggregateTypeSelect,
  GroupedListDataItems,
} from './aggregateTypeSelect'

interface props {
  handler: AggregateOptionsHandler
}

const movementList = [
  'Left',
  'Thru-Left',
  'Thru',
  'Thru-Right',
  'Right',
  'None',
]

const directionFilter = [
  'EastBound',
  'Westbound',
  'Northbound',
  'Southbound',
  'Northeast',
  'Northwest',
  'Southeast',
  'Southwest',
]

const daysOfWeekList: string[] = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thrusday',
  'Friday',
  'Saturday',
]

export const AggregateDataOptions = ({ handler }: props) => {
  const renderSelect = (
    label: string,
    name: string,
    data: SingularListDataItems[],
    onEdit: (val: any) => void,
    value: string | number
  ) => {
    return (
      <CustomSelect
        label={label}
        name={name}
        data={data}
        onChange={(e) => onEdit(e.target.value)}
        displayProperty="label"
        value={value}
        sx={{
          marginRight: 2,
          marginBottom: 1,
          minWidth: '226px',
        }}
      />
    )
  }

  // const renderAggregateSelect = (
  //   label: string,
  //   name: string,
  //   data: GroupedListDataItems[],
  //   onEdit: (val: any) => void,
  //   value: string
  // ) => {
  //   return (
  //     <AggregateTypeSelect
  //       label={label}
  //       name={name}
  //       data={data}
  //       displayProperty="label"
  //       onChange={(e) => onEdit(e.target.value)}
  //       value={value}
  //     />
  //   )
  // }

  // const renderCountCheckbox = () => {
  //   return (
  //     <Box sx={{ padding: '10px', paddingLeft: '20%', minWidth: '250px' }}>
  //       <FormControlLabel
  //         control={
  //           <Checkbox
  //             checked={handler.eventChecked}
  //             onChange={handler.changeEventChecked}
  //             name="showEventCounts"
  //           />
  //         }
  //         label="Show Event Counts"
  //       />
  //     </Box>
  //   )
  // }

  const renderBinSizeSlider = () => {
    return (
      <Box sx={{ paddingLeft: '5%', paddingRight: '20%', minWidth: '250px' }}>
        <label
          id="bin-size-slider-label"
          sx={{ fontWeight: 'bold', marginBottom: '6px' }}
        >
          Bin Size
        </label>
        <Slider
          aria-labelledby="bin-size-slider-label"
          value={handler.binSize}
          onChange={(e, val) => handler.changeBinSize(val as number)}
          marks={binSizeMarks}
          step={null}
          min={1}
          max={4}
        />
      </Box>
    )
  }

  const renderAggregationTypeSelector = () => {
    return (
      <Box sx={{ paddingLeft: '5%', paddingRight: '15%', minWidth: '250px' }}>
        <Typography sx={{ fontWeight: 'bold' }}>Aggregation Type</Typography>
        <FormControl component="fieldset">
          <RadioGroup
            row
            value={handler.averageOrSum}
            onChange={(e, value) =>
              handler.changeAverageOrSum(Number.parseInt(value))
            }
          >
            <FormControlLabel value={0} control={<Radio />} label="Sum" />
            <FormControlLabel value={1} control={<Radio />} label="Average" />
          </RadioGroup>
        </FormControl>
      </Box>
    )
  }
  const handleAggregateChange = (e: SelectChangeEvent<unknown>) => {
    handler.changeMetricType(e.target.value)
  }
  const renderAggregateChartOptions = () => {
    return (
      <Paper sx={{ padding: 3 }}>
        <Grid container sx={{}}>
          <Grid item md={4} lg={4} xl={4}>
            <AggregateTypeSelect
              label={'Metric Type'}
              name={'MetricType'}
              data={MetricTypeOptionsList}
              displayProperty="label"
              onChange={handleAggregateChange}
              value={handler.metricType}
      />
          </Grid>
          <Grid item md={4} lg={4} xl={4}>
            {renderSelect(
              'X-Axis',
              'X-Axis',
              xAxisOptions,
              handler.changeXAxisType,
              handler.xAxisType
            )}
          </Grid>
          {/* May add later depending for requirements for aggregation*/}
          {/* <Grid item md={4} lg={4} xl={4}>
            {renderSelect(
              'Detection Type',
              'DetectionType',
              detectionTypes,
              handler.changeDetectionType,
              handler.detectionType
            )}
          </Grid> */}
          <Grid item md={4} lg={4} xl={4}>
            {renderSelect(
              'Chart Type',
              'ChartType',
              chartTypeOptions,
              handler.changeVisualChartType,
              handler.visualChartType
            )}
          </Grid>
          <Grid item md={4} lg={4} xl={4}>
            {renderSelect(
              'Y-Axis',
              'Y-Axis',
              YAxisOptions,
              handler.changeYAxisType,
              handler.yAxisType
            )}
          </Grid>
          {/* <Grid item md={4} lg={4} xl={4}>
            {renderCountCheckbox()}
          </Grid> */}
          <Grid item md={4} lg={4} xl={4}>
            {renderBinSizeSlider()}
          </Grid>
          <Grid item md={4} lg={4} xl={4}>
            {renderAggregationTypeSelector()}
          </Grid>
        </Grid>
      </Paper>
    )
  }

  return (
    <Box
      sx={{
        display: 'flex',
        flexWrap: 'wrap',
        flexDirection: 'column',
        maxWidth: '950px',
        gap: 2,
      }}
    >
      <Box
        sx={{
          display: 'flex',
          flexWrap: 'wrap',
          gap: 2,
          justifyContent: 'space-between',
        }}
      >
        <Box
          sx={{
            flex: 1,
            display: 'flex',
            minWidth: '280px',
            gap: 2,
          }}
        >
          <StyledPaper sx={{ padding: 3, flexGrow: 1 }}>
            <SelectDateTime
              dateFormat={'MMM dd, yyyy'}
              startDateTime={handler.startDateTime}
              endDateTime={handler.endDateTime}
              changeStartDate={handler.changeStartDate}
              changeEndDate={handler.changeEndDate}
              timePeriod={true}
              startTimePeriod={handler.startDateTime}
              endTimePeriod={handler.endTime}
              changeStartTimePeriod={handler.changeStartTime}
              changeEndTimePeriod={handler.changeEndTime}
              noCalendar
            />
          </StyledPaper>
        </Box>

        <Box
          sx={{
            flex: 1,
            display: 'flex',
            gap: 2,
          }}
        >
          <MultiSelectCheckbox
            itemList={daysOfWeekList}
            selectedItems={handler.selectedDays}
            setSelectedItems={handler.changeSelectedDays}
            header="Days"
          />
          <MultiSelectCheckbox
            itemList={directionFilter}
            selectedItems={handler.selectedDirections}
            setSelectedItems={handler.changeSelectedDirections}
            header="Directional Filters"
          />

          <MultiSelectCheckbox
            itemList={movementList}
            selectedItems={handler.selectedMovements}
            setSelectedItems={handler.changeSelectedMovements}
            header="Movement Filters"
          />
        </Box>
      </Box>
      {renderAggregateChartOptions()}
    </Box>
  )
}
