import { SM_ChartType } from '@/features/speedManagementTool/types/charts';
import { reportsAxios } from '@/lib/axios';
import { useQuery } from 'react-query';
import { dateToTimestamp } from '@/utils/dateTime';
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query';
import { TransformedChartResponse, RawChartResponse } from '@/features/charts/types';
import { transformChartData } from './transformData';

export const SM_TypeApiMap: Record<SM_ChartType, string> = {
  [SM_ChartType.CONGESTION_TRACKING]: '/CongestionTracking/GetReportData',
  [SM_ChartType.SPEED_OVER_TIME]: '/SpeedOverTime/GetReportData',
  // Add other SM_ChartTypes and their corresponding endpoints here
};

type StringBooleanMap = Record<string, boolean | string | Date>;

const mapStringBooleansToBoolean = (obj: ChartOptions) => {
  return Object.entries(obj).reduce<StringBooleanMap>((acc, [key, value]) => {
    if (typeof value === 'string') {
      if (value.toLowerCase() === 'true') {
        acc[key] = true;
      } else if (value.toLowerCase() === 'false') {
        acc[key] = false;
      } else {
        acc[key] = value;
      }
    } else {
      acc[key] = value;
    }
    return acc;
  }, {});
};

export const getSMCharts = async (
  type: SM_ChartType,
  options: ChartOptions
): Promise<TransformedChartResponse> => {
  const endpoint = SM_TypeApiMap[type];

  const transformedOptions = mapStringBooleansToBoolean(options);

  transformedOptions.start = dateToTimestamp(transformedOptions.start as Date);
  transformedOptions.end = dateToTimestamp(transformedOptions.end as Date);

  const response = await reportsAxios.post(endpoint, transformedOptions);

  return transformChartData({
    type: type,
    data: response,
  } as unknown as RawChartResponse);
};\\type QueryFnType = typeof getSMCharts;

type UseSMChartsOptions = BaseOptions & {
  chartType: SM_ChartType;
  chartOptions: ChartOptions;
};

type BaseOptions = {
  config?: QueryConfig<QueryFnType>;
};

export const useSMCharts = ({
  chartType,
  chartOptions,
  config,
}: UseSMChartsOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false, // Set to false to control when the query runs
    queryKey: ['sm_charts', chartType, chartOptions],
    queryFn: () => getSMCharts(chartType, chartOptions),
  });
};


