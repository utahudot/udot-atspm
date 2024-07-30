import { usePostRequest } from '@/hooks/usePostRequest'
import { identityAxios } from '@/lib/axios'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

interface EditUsersData {
  firstName: string
  lastName: string
  agency: string
  email: string
  userName: string
  userId: string
  fullName: string
  roles: string[]
}

const route = 'users/update'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})
const axiosInstance = identityAxios

export function useEditUsers() {
  const mutation = usePostRequest({ url: route, axiosInstance, headers })
  return mutation
}
