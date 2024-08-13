// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - pagesCheck.ts
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
import { useRouter } from 'next/router'
import { useEffect, useState } from 'react'

export enum PageNames {
  Areas = 'Areas',
  DeviceConfigurations = 'Device Configurations',
  FAQs = 'FAQs',
  Jurisdiction = 'Jurisdictions',
  MenuItems = 'Menu Items',
  Location = 'Locations',
  MeasureDefaults = 'Measure Defaults',
  Products = 'Products',
  Region = 'Regions',
  Roles = ' Roles',
  Routes = 'Routes',
  Users = ' Users',
  WatchdogDashboard = 'Watchdog Dashboard',
}

const generalConfigListToLink: Map<string, string> = new Map([
  [PageNames.FAQs, '/admin/faq'],
  [PageNames.MenuItems, '/admin/menu-items'],
  [PageNames.MeasureDefaults, '/admin/measure-defaults'],
])
const locationConfigListToLink: Map<string, string> = new Map([
  [PageNames.Areas, '/admin/areas'],
  [PageNames.Jurisdiction, '/admin/jurisdictions'],
  [PageNames.Location, '/admin/locations'],
  [PageNames.Region, '/admin/regions'],
  [PageNames.Routes, '/admin/routes'],
  [PageNames.Products, '/admin/products'],
  [PageNames.DeviceConfigurations, '/admin/device-configurations'],
])

const userConfigToLink: Map<string, string> = new Map([
  [PageNames.Users, '/admin/users'],
])

const rolesConfigToLink: Map<string, string> = new Map([
  [PageNames.Roles, '/admin/roles'],
])

const adminAccessToLinks = new Map([
  ['GeneralConfiguration:View', generalConfigListToLink],
  ['LocationConfiguration:View', locationConfigListToLink],
  ['User:View', userConfigToLink],
  ['Role:View', rolesConfigToLink],
])

export const useGetAdminPagesList = () => {
  const claims = Cookies.get('claims')

  if (!claims) {
    return new Map()
  }
  const pagesToView = new Map<string, string>()

  if (claims.toLowerCase().includes('admin')) {
    adminAccessToLinks.forEach((map) => {
      map.forEach((value, key) => {
        pagesToView.set(key, value)
      })
    })
    return pagesToView
  } else {
    const claimsList = claims.split(',')
    claimsList.forEach((claim) => {
      adminAccessToLinks.forEach((value, key) => {
        if (claim.toLowerCase() === key.toLowerCase()) {
          value.forEach((linkValue, linkKey) => {
            pagesToView.set(linkKey, linkValue)
          })
        }
      })
    })
    return pagesToView
  }
}

export const useViewPage = (page: string) => {
  const [isLoading, setIsLoading] = useState(true)
  const isLoggedIn = Cookies.get('loggedIn')
  const pagesToView = useGetAdminPagesList()

  useEffect(() => {
    const redirectToLogin = () => {
      window.location.replace('/login')
    }

    const redirectToUnauthorized = () => {
      window.location.replace('/unauthorized')
    }

    if (!isLoggedIn || isLoggedIn.toLowerCase() !== 'true') {
      redirectToLogin()
      return
    }

    if (!pagesToView.has(page)) {
      redirectToUnauthorized()
    } else {
      setIsLoading(false)
    }
  }, [page, isLoggedIn, pagesToView])

  return { isLoading }
}

export const useSideBarPermission = (
  claim: string,
  redirectOnMissingPermissions = false
) => {
  const [hasPermission, setHasPermission] = useState(false)
  const router = useRouter()

  useEffect(() => {
    const checkPermission = () => {
      if (typeof window === 'undefined') {
        setHasPermission(false)
        return
      }

      const userClaims = Cookies.get('claims')
      if (
        userClaims &&
        (userClaims.toLowerCase().includes(claim.toLowerCase()) ||
          userClaims.toLowerCase().includes('admin'))
      ) {
        setHasPermission(true)
      } else {
        setHasPermission(false)
        if (redirectOnMissingPermissions) {
          window.location.replace('/unauthorized')
        }
      }
    }

    checkPermission()
  }, [claim, redirectOnMissingPermissions, router])

  return hasPermission
}

export const useUserHasClaim = (claim: string) => {
  const [hasPermission, setHasPermission] = useState(false)
  const claims = Cookies.get('claims')
  const userClaims = claims ? claims.split(',') : []

  useEffect(() => {
    const checkPermission = () => {
      if (typeof window === 'undefined') {
        setHasPermission(false)
        return
      }

      if (userClaims.includes(claim) || userClaims.includes('Admin')) {
        setHasPermission(true)
      } else {
        setHasPermission(false)
      }
    }

    checkPermission()
  }, [claim, userClaims])

  return hasPermission
}
