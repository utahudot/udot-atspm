import { IDENTITY_URL } from '@/config'
import IdentityDto from '@/features/identity/types/identityDto'
import { loginData } from '@/mocks/data/loginData'
import { registerData } from '@/mocks/data/registerData'
import { rest } from 'msw'

const config = (path: string) => `${IDENTITY_URL}${path}`

export const identityHandler = [
  rest.post(config(`Account/login`), (_req, res, ctx) => {
    return res(ctx.json<IdentityDto>(loginData))
  }),
  rest.post(config(`Account/register`), (_req, res, ctx) => {
    return res(ctx.json<IdentityDto>(registerData))
  }),
]
