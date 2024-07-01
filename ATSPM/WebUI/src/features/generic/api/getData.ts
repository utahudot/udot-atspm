import { regionDto } from '@/features/region/types/regionDto'
import { useGetRequest } from '@/hooks/useGetRequest'
import { ApiResponse } from '@/types'

export function useRegion() {
  const route = '/Region'
  return useGetRequest<ApiResponse<regionDto>>({ route })
}
