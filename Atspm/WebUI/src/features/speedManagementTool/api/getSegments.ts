import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'

const token = Cookies.get('token')
const localhostURL = 'https://localhost:44379/'
const getSegments = async (): Promise<Segment[]> => {
  // console.log(`switch for ${SPEED_URL} once cloud works`)
  const { data } = await axios.get<Segment[]>(`${localhostURL}api/segment`, {
    headers: {
      Authorization: `Bearer ${token}`,
      //   'Content-Type': 'application/json',
    },
  })

  return data
}

export const useGetSegments = (config?: QueryConfig<typeof getSegments>) => {
  return useQuery<ExtractFnReturnType<typeof getSegments>>({
    queryKey: ['segments'],
    queryFn: getSegments,
    ...config,
  })
}
