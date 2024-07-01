import { useGetRequest } from '@/hooks/useGetRequest'
import { identityAxios } from '@/lib/axios'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'
import { RolesResponse } from '../types/roles'

const route = '/users'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

export function useGetAllUsers() {
  return useGetRequest<RolesResponse>({
    route,
    headers,
    axiosInstance: identityAxios,
  })
}
