import Axios, {
  AxiosInstance,
  AxiosResponse,
  InternalAxiosRequestConfig,
} from 'axios'
import Cookies from 'js-cookie'
import { getEnv } from './getEnv'

let configAxios: AxiosInstance
let reportsAxios: AxiosInstance
let identityAxios: AxiosInstance
let dataAxios: AxiosInstance
let speedAxios: AxiosInstance

export async function initializeAxiosInstances() {
  const env = await getEnv()

  configAxios = createAxiosInstance(env.CONFIG_URL)
  reportsAxios = createAxiosInstance(env.REPORTS_URL)
  identityAxios = createAxiosInstance(env.IDENTITY_URL)
  dataAxios = createAxiosInstance(env.DATA_URL)
  speedAxios = createAxiosInstance(env.SPEED_URL)
}

function createAxiosInstance(baseURL: string): AxiosInstance {
  const instance = Axios.create({ baseURL })

  instance.interceptors.request.use(authRequestInterceptor)
  instance.interceptors.response.use(
    (response: AxiosResponse) => {
      if (baseURL === configAxios?.defaults.baseURL) {
        response.data = removeZFromTimestamps(response.data)
      }
      return response.data
    },
    (error) => Promise.reject(error)
  )

  return instance
}

function authRequestInterceptor(config: InternalAxiosRequestConfig) {
  const token = Cookies.get('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
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

// Export instances as undefined initially, but they will be initialized
// when the application starts by calling `initializeAxiosInstances`.
export { configAxios, dataAxios, identityAxios, reportsAxios, speedAxios }
