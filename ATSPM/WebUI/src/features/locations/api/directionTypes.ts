import { DirectionType } from '@/features/locations/types'
import { useGetRequest } from '@/hooks/useGetRequest'
import { ApiResponse } from '@/types'

const route = '/DirectionType'

export function useGetDirectionTypes() {
  return useGetRequest<ApiResponse<DirectionType>>({ route })
}
