// Pin a zone that observes DST; under UTC the spring-forward gap never occurs
// and a Date-based parser would pass by accident.
process.env.TZ = 'America/Denver'

import type { Plan } from '@/api/reports'
import {
  formatPlanTime,
  getPlanBoundaryMinutes,
  getPlanIntervalMinutes,
  schedulePlanPalette,
} from './schedule'

describe('time-of-day schedules', () => {
  test('reads plan timestamps as wall-clock time across spring-forward DST', () => {
    const plan = {
      start: '2026-03-08T02:30:00',
      end: '2026-03-08T04:00:00',
    } as Plan

    expect(getPlanBoundaryMinutes(plan.start)).toBe(150)
    expect(formatPlanTime(plan.start)).toBe('02:30')
    expect(getPlanIntervalMinutes(plan)).toEqual({ start: 150, end: 240 })
  })

  test('provides distinct colors for more than six plans', () => {
    expect(schedulePlanPalette).toHaveLength(10)
    expect(new Set(schedulePlanPalette).size).toBe(schedulePlanPalette.length)
  })
})
