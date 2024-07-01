import { CONFIG_URL } from '@/config'
import { linksData } from '@/mocks/data/linksData'
import { rest } from 'msw'

const config = (path: string) => `${CONFIG_URL}${path}`

export const linksHandlers = [
  rest.get(config(`ExternalLink`), (_req, res, ctx) => {
    return res(ctx.json<any>(linksData))
  }),
]
