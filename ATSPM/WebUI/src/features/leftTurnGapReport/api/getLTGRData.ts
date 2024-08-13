// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getLTGRData.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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