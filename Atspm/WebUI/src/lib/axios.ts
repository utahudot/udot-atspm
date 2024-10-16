import axios, { AxiosRequestConfig, InternalAxiosRequestConfig } from 'axios'
import Cookies from 'js-cookie'
import { getEnv } from './getEnv'

// Retrieve environment variables synchronously
const env = getEnv()

// Initialize Axios instances synchronously
export const configAxios = createAxiosInstance(env.CONFIG_URL)
export const reportsAxios = createAxiosInstance(env.REPORTS_URL)
export const identityAxios = createAxiosInstance(env.IDENTITY_URL)
export const dataAxios = createAxiosInstance(env.DATA_URL)
export const speedAxios = createAxiosInstance(env.SPEED_URL)

// Function to create an Axios instance with common interceptors
function createAxiosInstance(baseURL: string) {
  const instance = axios.create({ baseURL })

  instance.interceptors.request.use(authRequestInterceptor)
  instance.interceptors.response.use(
    (response) => {
      return response.data // Modify response data if necessary
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

// Define individual request functions for each Axios instance
export const configRequest = <T>(config: AxiosRequestConfig) => {
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
