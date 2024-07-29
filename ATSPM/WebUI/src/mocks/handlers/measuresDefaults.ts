import { CONFIG_URL } from '@/config'
import { measuresDefaultData } from '@/mocks/data/measuresDefaultData'
import { rest } from 'msw'

const config = (path: string) => `${CONFIG_URL}${path}`

export const measuresDefaultHandlers = [
  rest.get(config(`MeasuresDefault`), (_req, res, ctx) => {
    return res(ctx.json<any>(measuresDefaultData))
  }),
]
