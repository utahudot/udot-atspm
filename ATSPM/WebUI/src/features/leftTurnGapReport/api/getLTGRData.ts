import { ApiResponse } from '@/types';
import { useQuery } from 'react-query';
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query';
import { reportsAxios } from '@/lib/axios';

export interface LeftTurnGapReportParams {
  locationIdentifier: string;
  start: string;
  end: string;
  approachIds: number[];
  daysOfWeek: number[];
  startHour: number;
  startMinute: number;
  endHour: number;
  endMinute: number;
  getAMPMPeakPeriod: boolean;
  getAMPMPeakHour: boolean;
  get24HourPeriod: boolean;
  getGapReport: boolean;
  acceptableGapPercentage: number;
  getSplitFail: boolean;
  acceptableSplitFailPercentage: number;
  getPedestrianCall: boolean;
  getConflictingVolume: boolean;
}

export const getLeftTurnGapReport = async (
  params: LeftTurnGapReportParams
): Promise<any> => {
  const result: ApiResponse<any> = await reportsAxios.post(
    `LeftTurnGapReport/getReportData`,
    params
  );
  return result;
};

type QueryFnType = typeof getLeftTurnGapReport;

type UseLeftTurnGapReport = {
  config?: QueryConfig<QueryFnType>;
  params: LeftTurnGapReportParams;
};

export const useLeftTurnGapReport = ({
  config,
  params,
}: UseLeftTurnGapReport) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['LeftTurnGapReport', params],
    enabled: false,
    queryFn: () => getLeftTurnGapReport(params),
  });
};