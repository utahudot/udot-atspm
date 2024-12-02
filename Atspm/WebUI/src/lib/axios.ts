import { getEnv } from '@/utils/getEnv'
import axios, { AxiosRequestConfig, InternalAxiosRequestConfig } from 'axios'
import Cookies from 'js-cookie'

export let configAxios: ReturnType<typeof createAxiosInstance>
export let reportsAxios: ReturnType<typeof createAxiosInstance>
export let identityAxios: ReturnType<typeof createAxiosInstance>
export let dataAxios: ReturnType<typeof createAxiosInstance>
export let speedAxios: ReturnType<typeof createAxiosInstance>

const BASE_PATH = '/api/v1/'

export const initializeAxiosInstances = async () => {
  const env = await getEnv()

  if (!env) {
    return
  }

  if (env.CONFIG_URL)
    configAxios = createAxiosInstance(env.CONFIG_URL + BASE_PATH)
  if (env.REPORTS_URL)
    reportsAxios = createAxiosInstance(env.REPORTS_URL + BASE_PATH)
  if (env.IDENTITY_URL)
    identityAxios = createAxiosInstance(env.IDENTITY_URL + BASE_PATH)
  if (env.DATA_URL) dataAxios = createAxiosInstance(env.DATA_URL + BASE_PATH)
  if (env.SPEED_URL) speedAxios = createAxiosInstance(env.SPEED_URL + BASE_PATH)
}

function createAxiosInstance(baseURL: string) {
  const instance = axios.create({ baseURL })

  instance.interceptors.request.use(authRequestInterceptor)
  instance.interceptors.response.use(
    (response) => {
      return response.data
    },
    (error) => Promise.reject(error)
  )

  return instance
}

// Request interceptor to add the Authorization header
function authRequestInterceptor(config: InternalAxiosRequestConfig) {
  const token = Cookies.get('token')
  if (token) {
    config.headers.authorization = `Bearer ${token}`
  }
  return config
}

export const configRequest = <T>(config: AxiosRequestConfig): Promise<T> => {
  return configAxios.request<unknown, T>(config)
}

export const reportsRequest = <T>(config: AxiosRequestConfig): Promise<T> => {
  return reportsAxios.request<unknown, T>(config)
}

export const identityRequest = <T>(config: AxiosRequestConfig): Promise<T> => {
  return identityAxios.request<unknown, T>(config)
}

export const dataRequest = <T>(config: AxiosRequestConfig): Promise<T> => {
  return dataAxios.request<unknown, T>(config)
}

export const speedRequest = <T>(config: AxiosRequestConfig): Promise<T> => {
  return speedAxios.request<unknown, T>(config)
}
