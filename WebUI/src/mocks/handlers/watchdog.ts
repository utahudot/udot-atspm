import { CONFIG_URL } from '@/config'
import { Watchdog } from '@/features/watchdog/types'
import { rest } from 'msw'
import { watchDog } from '../data/watchDog'

const config = (path: string) => `${CONFIG_URL}${path}`

export const watchdogHandlers = [
  rest.get(config(`watchdog`), (_req, res, ctx) => {
    return res(ctx.json<Watchdog[]>(watchDog))
  }),
]
