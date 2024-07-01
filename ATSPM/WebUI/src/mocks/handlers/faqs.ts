import { CONFIG_URL } from '@/config'
import { Faq } from '@/features/faq/types'
import { faqs } from '@/mocks/data/faqs'
import { ApiResponse } from '@/types'
import { rest } from 'msw'

const config = (path: string) => `${CONFIG_URL}${path}`

export const faqHandlers = [
  rest.get(config(`Faq`), (_req, res, ctx) => {
    return res(ctx.json<ApiResponse<Faq>>(faqs))
  }),
]
