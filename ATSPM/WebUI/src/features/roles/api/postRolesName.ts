import { usePostRequest } from '@/hooks/usePostRequest'
import { identityAxios } from '@/lib/axios'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

interface RoleData {
  roleName: string
}
const route = 'roles'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})
const axiosInstance = identityAxios

export function usePostRoleName() {
  const mutation = usePostRequest({ url: route, axiosInstance, headers })
  return mutation
}
