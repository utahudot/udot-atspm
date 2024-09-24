// src/lib/axios.ts

import axios, {
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  InternalAxiosRequestConfig,
} from 'axios'
import Cookies from 'js-cookie'
import { getEnv } from './getEnv'

// Retrieve environment variables synchronously
const env = getEnv()

// Initialize Axios instances synchronously
export const configAxios: AxiosInstance = createAxiosInstance(env.CONFIG_URL)
export const reportsAxios: AxiosInstance = createAxiosInstance(env.REPORTS_URL)
export const identityAxios: AxiosInstance = createAxiosInstance(
  env.IDENTITY_URL
)
export const dataAxios: AxiosInstance = createAxiosInstance(env.DATA_URL)
export const speedAxios: AxiosInstance = createAxiosInstance(env.SPEED_URL)

// Function to create an Axios instance with common interceptors
function createAxiosInstance(baseURL: string): AxiosInstance {
  const instance = axios.create({ baseURL })

  instance.interceptors.request.use(authRequestInterceptor)
  instance.interceptors.response.use(
    (response: AxiosResponse) => {
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
  return configAxios.request<T>(config)
}

export const reportsRequest = <T>(config: AxiosRequestConfig) => {
  return reportsAxios.request<T>(config)
}

export const identityRequest = <T>(config: AxiosRequestConfig) => {
  return identityAxios.request<T>(config)
}

export const dataRequest = <T>(config: AxiosRequestConfig) => {
  return dataAxios.request<T>(config)
}

export const speedRequest = <T>(config: AxiosRequestConfig) => {
  return speedAxios.request<T>(config)
}
