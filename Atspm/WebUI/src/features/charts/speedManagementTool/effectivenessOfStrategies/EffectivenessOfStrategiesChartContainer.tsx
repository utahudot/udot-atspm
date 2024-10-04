import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart';
import {
  Box,
  TextField,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableRow,
  TableHead,
  Typography,
} from '@mui/material';
import { useEffect, useState } from 'react';
import transformEffectivenessOfStrategiesData from './effectivenessOfStrategies.transformer';
import { format } from 'date-fns';

interface WeeklyEffectiveness {
  startDate: string;
  endDate: string;
  flow: number;
  minSpeed: number;
  maxSpeed: number;
  averageSpeed: number;
  averageEightyFifthSpeed: number;
  variability: number;
  percentViolations: number;
  percentExtremeViolations: number;
}

interface Metrics {
  startDate: string;
  endDate: string;
  flow: number;
  minSpeed: number;
  maxSpeed: number;
  averageSpeed: number;
  averageEightyFifthSpeed: number;
  variability: number;
  percentViolations: number;
  percentExtremeViolations: number;
}

interface OverallsData {
  segmentId: string;
  segmentName: string;
  changeInAverageSpeed: number;
  changeInEightyFifthPercentileSpeed: number;
  changeInVariability: number;
  changeInPercentViolations: number;
  changeInPercentExtremeViolations: number;
  speedLimit: number;
  weeklyEffectiveness: WeeklyEffectiveness[];
  before: Metrics;
  after: Metrics;
}

interface ChartData {
  title: any;
  xAxis: any;
  yAxis: any;
  grid: any;
  legend: any;
  tooltip: any;
  series: any;
  dataZoom: any;
  response: OverallsData[];
}

const EffectivenessOfStrategiesChartsContainer = ({
  chartData,
}: {
  chartData: ChartData;
}) => {
  const [customSpeedLimit, setCustomSpeedLimit] = useState<number | null>(null);
  const [transformedChartData, setTransformedChartData] =
    useState<ChartData>(chartData);

  const handleSpeedLimitChange = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const value = event.target.value ? parseInt(event.target.value) : null;
    setCustomSpeedLimit(value);
  };

  useEffect(() => {
    if (customSpeedLimit !== null) {
      setTransformedChartData(
        transformEffectivenessOfStrategiesData(
          chartData.response,
          customSpeedLimit
        )
      );
    } else {
      setTransformedChartData(chartData);
    }
  }, [customSpeedLimit, chartData]);

  const Overalls = chartData.response;
  console.log(Overalls);

  const beforeMetrics = Overalls[0].before;
  const afterMetrics = Overalls[0].after;
  const differenceMetrics = {
    averageSpeed: afterMetrics.averageSpeed - beforeMetrics.averageSpeed,
    averageEightyFifthSpeed:
      afterMetrics.averageEightyFifthSpeed -
      beforeMetrics.averageEightyFifthSpeed,
    percentViolations:
      afterMetrics.percentViolations - beforeMetrics.percentViolations,
    percentExtremeViolations:
      afterMetrics.percentExtremeViolations -
      beforeMetrics.percentExtremeViolations,
    variability: afterMetrics.variability - beforeMetrics.variability,
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return format(date, 'MM/dd/yyyy');
  };

  return (
    <>
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mr: 10 }}>
        <TextField
          label="Custom Speed Limit"
          type="number"
          value={customSpeedLimit !== null ? customSpeedLimit : ''}
          onChange={handleSpeedLimitChange}
          InputProps={{ inputProps: { min: 0 } }} // Optional: Prevent negative numbers
          variant="outlined"
          sx={{ width: 150, mt:2 }}
          size='small'
        />
      </Box>
      <Box sx={{ display: 'flex', justifyContent: 'center', pt: 2 }}>
        <ApacheEChart
          id="speed-over-time-chart"
          option={transformedChartData}
          style={{ width: '1100px', height: '500px' }}
          hideInteractionMessage
        />
      </Box>

      <TableContainer
        sx={{
          mt: 4,
          maxWidth: '1100px',
          mx: 'auto',
          overflowX: 'inherit',
        }}
      >
        <Table>
          <TableRow>
          <TableCell colSpan={6} sx={{padding:'20px', border:'none', textAlign: 'center'}}><Typography sx={{fontWeight:"bold"}}>Overalls</Typography></TableCell>
          </TableRow>
          </Table>
        <Table size="small" stickyHeader>

          <TableHead>
            <TableRow>
              <TableCell></TableCell>
              <TableCell>Avg Speed (MPH)</TableCell>
              <TableCell>85th %ile Speed (MPH)</TableCell>
              <TableCell>% Violations</TableCell>
              <TableCell>% Severe Violations</TableCell>
              <TableCell>Speed Variability (MPH)</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
            <TableCell>Before</TableCell>
            <TableCell>{beforeMetrics.averageSpeed.toFixed(2)}</TableCell>
              <TableCell>
                {beforeMetrics.averageEightyFifthSpeed.toFixed(2)}
              </TableCell>
              <TableCell>{beforeMetrics.percentViolations.toFixed(2)}%</TableCell>
              <TableCell>
                {beforeMetrics.percentExtremeViolations.toFixed(2)}%
              </TableCell>
              <TableCell>{beforeMetrics.variability.toFixed(2)}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>After</TableCell>
              <TableCell>{afterMetrics.averageSpeed.toFixed(2)}</TableCell>
              <TableCell>
                {afterMetrics.averageEightyFifthSpeed.toFixed(2)}
              </TableCell>
              <TableCell>{afterMetrics.percentViolations.toFixed(2)}%</TableCell>
              <TableCell>
                {afterMetrics.percentExtremeViolations.toFixed(2)}%
              </TableCell>
              <TableCell>{afterMetrics.variability.toFixed(2)}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Difference</TableCell>
              <TableCell>{differenceMetrics.averageSpeed.toFixed(2)}</TableCell>
              <TableCell>
                {differenceMetrics.averageEightyFifthSpeed.toFixed(2)}
              </TableCell>
              <TableCell>{differenceMetrics.percentViolations.toFixed(2)}%</TableCell>
              <TableCell>
                {differenceMetrics.percentExtremeViolations.toFixed(2)}%
              </TableCell>
              <TableCell>{differenceMetrics.variability.toFixed(2)}</TableCell>
            </TableRow>
            <TableCell colSpan={6} sx={{padding:'10px', border:'none'}}></TableCell>

          </TableBody>   
          <TableRow>
            <TableCell colSpan={6} sx={{padding:'20px', border:'none', textAlign: 'center'}}><Typography sx={{fontWeight:"bold"}}>Before Treatment Application</Typography></TableCell>
          </TableRow>       
          <TableHead>

            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell>Avg Speed (MPH)</TableCell>
              <TableCell>85th %ile Speed (MPH)</TableCell>
              <TableCell>% Violations</TableCell>
              <TableCell>% Severe Violations</TableCell>
              <TableCell>Speed Variability (MPH)</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {Overalls[0].weeklyEffectiveness
              .filter(
                (week) =>
                  new Date(week.endDate) >= new Date(beforeMetrics.startDate) &&
                  new Date(week.endDate) <= new Date(beforeMetrics.endDate)
              )
              .map((week) => (
                <TableRow key={week.endDate}>
                  <TableCell>
                    {`${formatDate(week.startDate)} - ${formatDate(week.endDate)}`}
                  </TableCell>
                  <TableCell>{week.averageSpeed.toFixed(2)}</TableCell>
                  <TableCell>
                    {week.averageEightyFifthSpeed.toFixed(2)}
                  </TableCell>
                  <TableCell>{week.percentViolations.toFixed(2)}%</TableCell>
                  <TableCell>
                    {week.percentExtremeViolations.toFixed(2)}%
                  </TableCell>
                  <TableCell>{week.variability.toFixed(2)}</TableCell>
                </TableRow>
              ))}
                <TableCell colSpan={6} sx={{padding:'10px', border:'none'}}></TableCell>
          </TableBody>
          <TableRow>
          <TableCell colSpan={6} sx={{padding:'10px', border:'none', textAlign: 'center'}}><Typography sx={{fontWeight:"bold"}}>After Treatment Application</Typography></TableCell>
          </TableRow>
          <TableHead>

            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell>Avg Speed (MPH)</TableCell>
              <TableCell>85th %ile Speed (MPH)</TableCell>
              <TableCell>% Violations</TableCell>
              <TableCell>% Extreme Violations</TableCell>
              <TableCell>Speed Variability (MPH)</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {Overalls[0].weeklyEffectiveness
              .filter(
                (week) =>
                  new Date(week.endDate) >= new Date(afterMetrics.startDate) &&
                  new Date(week.endDate) <= new Date(afterMetrics.endDate)
              )
              .map((week) => (
                <TableRow key={week.endDate}>
                  <TableCell>
                    {`${formatDate(week.startDate)} - ${formatDate(week.endDate)}`}
                  </TableCell>
                  <TableCell>{week.averageSpeed.toFixed(2)}</TableCell>
                  <TableCell>
                    {week.averageEightyFifthSpeed.toFixed(4)}
                  </TableCell>
                  <TableCell>{week.percentViolations.toFixed(2)}%</TableCell>
                  <TableCell>
                    {week.percentExtremeViolations.toFixed(2)}%
                  </TableCell>
                  <TableCell>{week.variability.toFixed(2)}</TableCell>
                </TableRow>
              ))}
          </TableBody>
        </Table>
      </TableContainer>
    </>
  );
};

export default EffectivenessOfStrategiesChartsContainer;
