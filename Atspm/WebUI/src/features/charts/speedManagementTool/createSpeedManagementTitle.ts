// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - createSpeedManagementTitle.ts
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
import type { TitleComponentOption } from 'echarts'

type SpeedManagementTitleProps = {
  title: string
  dateRange?: string
  speedLimit?: number | string | null
  subtitle?: string
  info?: string
}

const formatSpeedLimit = (speedLimit?: number | string | null) => {
  if (speedLimit === undefined || speedLimit === null) return undefined

  const speedLimitText = String(speedLimit).trim()
  if (!speedLimitText) return undefined

  const unitLabel = speedLimitText.toLowerCase().endsWith('mph')
    ? speedLimitText
    : `${speedLimitText} mph`

  return `Speed Limit: ${unitLabel}`
}

export function createSpeedManagementTitle({
  title,
  dateRange,
  speedLimit,
  subtitle,
  info,
}: SpeedManagementTitleProps): TitleComponentOption[] {
  const titles: TitleComponentOption[] = [
    {
      left: 10,
      top: 0,
      text: title,
      textStyle: {
        fontSize: 18,
        fontWeight: 600,
        color: '#1f1f1f',
      },
    },
  ]

  const metadata = [subtitle, dateRange, formatSpeedLimit(speedLimit)]
    .filter(Boolean)
    .join('  •  ')

  if (metadata) {
    titles.push({
      left: 10,
      top: 27,
      text: metadata,
      textStyle: {
        fontSize: 15,
        fontWeight: 400,
        color: '#2b2b2b',
      },
    })
  }

  if (info) {
    titles.push({
      left: 10,
      right: 10,
      top: metadata ? 60 : 35,
      text: info,
      backgroundColor: '#f2f2f2',
      padding: [8, 12],
      borderRadius: 6,
      textStyle: {
        fontWeight: 400,
        fontSize: 12,
        rich: {
          values: { fontWeight: 600 },
        },
      },
    })
  }

  return titles
}
