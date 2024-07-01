import { MeasureType } from '@/features/charts/types'
import { useGetRequest } from '@/hooks/useGetRequest'
import { ApiResponse } from '@/types'

const route = '/MeasureType'
export function useGetMeasureTypes() {
  return useGetRequest<ApiResponse<MeasureType>>({ route })
}
