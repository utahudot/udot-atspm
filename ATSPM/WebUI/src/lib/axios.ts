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

function isValidTimestamp(value: string) {
  const iso8601Regex = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?Z$/
  return iso8601Regex.test(value)
}

function removeZFromTimestamps(data: any): any {
  if (typeof data === 'string' && isValidTimestamp(data)) {
    return data.replace(/Z$/, '')
  }
  if (Array.isArray(data)) {
    return data.map((item) => removeZFromTimestamps(item))
  }
  if (typeof data === 'object' && data !== null) {
    return Object.keys(data).reduce((acc, key) => {
      acc[key] = removeZFromTimestamps(data[key])
      return acc
    }, {} as any)
  }
  return data
}

function createAxiosInstance(baseURL: string): AxiosInstance {
  const instance = Axios.create({ baseURL })

  instance.interceptors.request.use(authRequestInterceptor)
  instance.interceptors.response.use(
    (response: AxiosResponse) => {
      if (baseURL === CONFIG_URL) {
        response.data = removeZFromTimestamps(response.data)
      }
      return response.data
    },
    (error) => Promise.reject(error)
  )

  return instance
}

export const configAxios = createAxiosInstance(CONFIG_URL)
export const reportsAxios = createAxiosInstance(REPORTS_URL)
export const identityAxios = createAxiosInstance(IDENTITY_URL)
export const dataAxios = createAxiosInstance(DATA_URL)
