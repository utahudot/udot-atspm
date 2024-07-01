import { useGetRequest } from '@/hooks/useGetRequest'
import { identityAxios } from '@/lib/axios'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'
type ClaimsList = string[]

const route = '/claims'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

export function useGetClaims() {
  return useGetRequest<ClaimsList>({
    route,
    headers,
    axiosInstance: identityAxios,
  })
}
