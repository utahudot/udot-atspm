import { DeviceCount } from '@/features/watchdog/types'
import { configAxios } from '@/lib/axios'
import { ExtractFnReturnType } from '@/lib/react-query'
import { useQuery } from 'react-query'

const getDeviceCount = async (): Promise<DeviceCount[]> => {
  const response = await configAxios.get('Device/GetActiveDevicesCount')
  return response.data
}

type QueryFnType = typeof getDeviceCount

export const useGetDeviceCount = () => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    queryKey: ['deviceCount'],
    queryFn: getDeviceCount,
  })
}
