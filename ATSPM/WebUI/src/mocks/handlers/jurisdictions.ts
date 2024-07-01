import { CONFIG_URL } from '@/config'
import { jurisdictionData } from '@/mocks/data/jurisdictionData'
import { rest } from 'msw'

const config = (path: string) => `${CONFIG_URL}${path}`

export const jurisdictionHandlers = [
  rest.get(config(`Jurisdiction`), (_req, res, ctx) => {
    return res(ctx.json<any>(jurisdictionData))
  }),
]
