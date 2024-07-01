import { CONFIG_URL, DATA_URL, IDENTITY_URL, REPORTS_URL } from '@/config'
import storage from '@/utils/storage'
import Axios, {
  AxiosInstance,
  AxiosResponse,
  InternalAxiosRequestConfig,
} from 'axios'

function authRequestInterceptor(config: InternalAxiosRequestConfig) {
  const token = storage.getToken()
  if (token) {
    config.headers.authorization = `${token}`
  }
  return config
}

function createAxiosInstance(baseURL: string): AxiosInstance {
  const instance = Axios.create({ baseURL })

  instance.interceptors.request.use(authRequestInterceptor)
  instance.interceptors.response.use(
    (response: AxiosResponse) => response.data,
    (error) => {
      // const message = error.response?.data?.message || error.message
      // useNotificationStore.getState().addNotification({
      //   type: 'error',
      //   title: 'Error',
      //   message,
      // })

      return Promise.reject(error)
    }
  )

  return instance
}

export const configAxios = createAxiosInstance(CONFIG_URL)
export const reportsAxios = createAxiosInstance(REPORTS_URL)
export const identityAxios = createAxiosInstance(IDENTITY_URL)
export const dataAxios = createAxiosInstance(DATA_URL)
