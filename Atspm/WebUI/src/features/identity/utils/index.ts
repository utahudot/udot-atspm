// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - index.ts
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
import Cookies from 'js-cookie'

export const doesUserHaveAccess = () => {
  if (typeof window === 'undefined') {
    return false
  }

  const loggedIn = Cookies.get('loggedIn')
  if (!loggedIn) {
    return false
  }

  const claims = Cookies.get('claims')
  return !!claims
}

interface CookieOptions {
  expires?: Date | number // Expiration can be a Date or number (number of days)
  secure?: boolean // Secure attribute (defaults to true)
  sameSite?: 'Strict' | 'Lax' | 'None' // SameSite policy options
  path?: string // Path attribute (optional)
  domain?: string // Domain attribute (optional)
}

export function setSecureCookie(
  name: string,
  value: string,
  options: CookieOptions = {}
): void {
  const defaultOptions: CookieOptions = {
    secure: true,
    expires: 1, // Default expiration: 1 day
  }

  const finalOptions = { ...defaultOptions, ...options }

  Cookies.set(name, value, finalOptions)
}
