// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - identity.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
