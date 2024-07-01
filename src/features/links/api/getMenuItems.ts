import { MenuItems } from '@/features/links/types/linkDto'
import { useGetRequest } from '@/hooks/useGetRequest'
import { configAxios } from '@/lib/axios'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const route = '/MenuItems'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

export function useGetMenuItems() {
  return useGetRequest<MenuItems>({
    route,
    headers,
    axiosInstance: configAxios,
  })
}
