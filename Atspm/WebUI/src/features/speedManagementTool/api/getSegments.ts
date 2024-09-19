import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import Cookies from 'js-cookie'
import { useQuery } from 'react-query'
import { getEnv } from '@/lib/getEnv'
import {Segment} from '../types/segement'

const token = Cookies.get('token')

const localhostURL = 'https://localhost:44343/api/v1/'
const getSegments = async (): Promise<Segment[]> => {
  const env = await getEnv()
  const { data } = await axios.get<Segment[]>(`${localhostURL}segment`, {
    headers: {
      Authorization: `Bearer ${token}`,
      //   'Content-Type': 'application/json',
    },
  })
  

  return data
}
// const getSegments = async (): Promise<Segment[]> => {
//   const env = await getEnv()
//   const { data } = await axios.get<Segment[]>(`${env.SPEED_URL}segment`, {
//     headers: {
//       Authorization: `Bearer ${token}`,
//       //   'Content-Type': 'application/json',
//     },
//   })
  

//   return data
// }

export const useGetSegments = (config?: QueryConfig<typeof getSegments>) => {
  return useQuery<ExtractFnReturnType<typeof getSegments>>({
    queryKey: ['segments'],
    queryFn: getSegments,
    ...config,
  })
}
