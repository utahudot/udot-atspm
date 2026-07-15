import type { MapLayer, PersistedMapLayer } from '@/features/mapLayers/types'
import { configAxios } from '@/lib/axios'
import type { ApiResponse } from '@/types'
import { useMutation, useQuery, useQueryClient } from 'react-query'

const route = '/MapLayer'
const queryKey = [route]

const normalizeMapLayers = (response: unknown): PersistedMapLayer[] => {
  const items = Array.isArray(response)
    ? response
    : ((response as Partial<ApiResponse<MapLayer>> | undefined)?.value ?? [])

  return items.filter(
    (item): item is PersistedMapLayer =>
      typeof item?.id === 'number' && typeof item?.name === 'string'
  )
}

export function useGetMapLayers() {
  return useQuery(queryKey, async () => {
    const response = await configAxios.get(route)
    return normalizeMapLayers(response)
  })
}

export function useCreateMapLayer() {
  const queryClient = useQueryClient()

  return useMutation(
    (mapLayer: MapLayer) => configAxios.post(route, mapLayer),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(queryKey)
      },
    }
  )
}

export function useUpdateMapLayer() {
  const queryClient = useQueryClient()

  return useMutation(
    (mapLayer: PersistedMapLayer) =>
      configAxios.patch(`${route}/${mapLayer.id}`, mapLayer),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(queryKey)
      },
    }
  )
}

export function useDeleteMapLayer() {
  const queryClient = useQueryClient()

  return useMutation((id: number) => configAxios.delete(`${route}/${id}`), {
    onSuccess: () => {
      queryClient.invalidateQueries(queryKey)
    },
  })
}
