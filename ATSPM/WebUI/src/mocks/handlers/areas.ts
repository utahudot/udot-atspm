import { CONFIG_URL } from '@/config'
import { areaData } from '@/mocks/data/areasData'
import { rest } from 'msw'

const config = (path: string) => `${CONFIG_URL}${path}`

export const areaHandlers = [
  rest.get(config(`Area`), (_req, res, ctx) => {
    return res(ctx.json<any>(areaData))
  }),
]
