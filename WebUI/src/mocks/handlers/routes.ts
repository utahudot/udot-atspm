import { CONFIG_URL } from '@/config'
import { rest } from 'msw'
import { routesData } from '../data/routesData'

const config = (path: string) => `${CONFIG_URL}${path}`

export const routeHandler = [
  rest.get(config(`Route`), (_req, res, ctx) => {
    console.log('mocked')
    return res(ctx.json<any>(routesData))
  }),
]
