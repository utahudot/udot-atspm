import { CONFIG_URL } from '@/config'
import { LocationExpanded } from '@/features/locations/types/Location'
import { configData } from '@/mocks/data/ConfigData'
import { locationExpanded } from '@/mocks/data/locationExpanded'
import { locationsData } from '@/mocks/data/locationsData'
import { versionData } from '@/mocks/data/versionData'
import { ApiResponse } from '@/types'
import { rest } from 'msw'

const config = (path: string) => `${CONFIG_URL}${path}`

export const locationHandlers = [
  rest.get(
    config(`Location/GetLatestVersionOfLocation(identifier=':id')`),
    (_req, res, ctx) => {
      return res(ctx.json<any>(configData))
    }
  ),
  rest.get(config(`Location/GetLocationsForSearch`), (_req, res, ctx) => {
    return res(ctx.json<any>(locationsData))
  }),
  rest.get(config(`Location/:id/approaches`), (_req, res, ctx) => {
    return res(ctx.json<any>(configData))
  }),
  rest.get(config(`Location/:id`), (req, res, ctx) => {
    return res(ctx.json<ApiResponse<LocationExpanded>>(locationExpanded))
  }),
  rest.get(config(`GetAllVersionsOfLocation`), (_req, res, ctx) => {
    const { identifier } = _req.params
    return res(ctx.json<any>(versionData))
  }),
]
