import type { NextApiRequest, NextApiResponse } from 'next'
import handler from '../pages/api/feature-flags'

const original = process.env.ENABLE_LANE_RECONCILIATION
afterAll(() => {
  if (original === undefined) delete process.env.ENABLE_LANE_RECONCILIATION
  else process.env.ENABLE_LANE_RECONCILIATION = original
})
it.each([undefined, 'false', 'TRUE', 'true'])(
  'lane reconciliation flag is explicitly true only: %s',
  (value) => {
    if (value === undefined) delete process.env.ENABLE_LANE_RECONCILIATION
    else process.env.ENABLE_LANE_RECONCILIATION = value
    const json = jest.fn()
    const status = jest.fn().mockReturnValue({ json })
    handler({} as NextApiRequest, { status } as unknown as NextApiResponse)
    expect(json).toHaveBeenCalledWith(
      expect.objectContaining({ laneReconciliation: value === 'true' })
    )
  }
)
