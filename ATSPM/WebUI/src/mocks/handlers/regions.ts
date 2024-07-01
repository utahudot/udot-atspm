import { CONFIG_URL } from '@/config'
import { regionsData } from '@/mocks/data/regionsData'
import { rest } from 'msw'

const config = (path: string) => `${CONFIG_URL}${path}`

export const regionHandlers = [
  rest.get(config(`Region`), (_req, res, ctx) => {
    return res(ctx.json<any>(regionsData))
  }),
]
