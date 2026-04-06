import { ToolType } from '@/features/charts/common/types'
import {
  TimeSpaceAverageOptions,
  TimeSpaceHistoricOptions,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import { toUTCDateStamp, toUTCDateWithTimeStamp } from '@/utils/dateTime'
import { getLinkPivotPcdTimeWindowFromTimeSpaceOptions } from './linkPivotPcdTimeWindow'

describe('getLinkPivotPcdTimeWindowFromTimeSpaceOptions', () => {
  it('builds a PCD time window from average options', () => {
    const options: TimeSpaceAverageOptions = {
      startDate: '2026-04-01',
      endDate: '2026-04-03',
      startTime: '08:15:00',
      endTime: '09:45:00',
      routeId: '42',
      speedLimit: null,
      daysOfWeek: [1, 2, 3],
      sequence: [],
      coordinatedPhases: [],
    }

    const result = getLinkPivotPcdTimeWindowFromTimeSpaceOptions(options)

    expect(toUTCDateStamp(result.startDateTime)).toBe('2026-04-01')
    expect(toUTCDateStamp(result.endDateTime)).toBe('2026-04-03')
    expect(toUTCDateWithTimeStamp(result.startTime)).toBe('08:15:00')
    expect(toUTCDateWithTimeStamp(result.endTime)).toBe('09:45:00')
  })

  it('builds a PCD time window from historic options', () => {
    const options: TimeSpaceHistoricOptions = {
      locationIdentifier: '1234',
      extendStartStopSearch: 0,
      showAllLanesInfo: false,
      routeId: '42',
      chartType: ToolType.TimeSpaceHistoric,
      speedLimit: null,
      start: new Date('2026-04-05T07:30:00Z'),
      end: new Date('2026-04-06T08:45:00Z'),
    }

    const result = getLinkPivotPcdTimeWindowFromTimeSpaceOptions(options)

    expect(toUTCDateStamp(result.startDateTime)).toBe('2026-04-05')
    expect(toUTCDateStamp(result.endDateTime)).toBe('2026-04-06')
    expect(result.startTime.toISOString()).toBe('2026-04-05T07:30:00.000Z')
    expect(result.endTime.toISOString()).toBe('2026-04-06T08:45:00.000Z')
  })
})
