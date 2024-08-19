// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - TimeProps.ts
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
import { ChartType, ToolType } from '@/features/charts/common/types'

export interface DateTimeProps {
  startDateTime: Date
  endDateTime: Date
  changeStartDate(date: Date): void
  changeEndDate(date: Date): void
  chartType?: ChartType | ToolType
}

export interface TimeOnlyProps {
  startTime: Date
  endTime: Date
  changeStartTime(date: Date): void
  changeEndTime(date: Date): void
}
