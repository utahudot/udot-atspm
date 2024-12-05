import { DeviceCount } from '@/features/watchdog/types'
import { ExtractFnReturnType } from '@/lib/react-query'
import axios from 'axios'
import { useQuery } from 'react-query'

const getDeviceCount = async (): Promise<DeviceCount[]> => {
  const response = await axios.get(
    'https://localhost:44315/api/v1/Device/GetActiveDevicesCount'
  )
  return response.data
}

type QueryFnType = typeof getDeviceCount

export const useGetDeviceCount = () => {
    return useQuery<ExtractFnReturnType<QueryFnType>>({
      queryKey: ['deviceCount'],
      queryFn: getDeviceCount,
    })
  }

