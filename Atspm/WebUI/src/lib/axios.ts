// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - axios.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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

  if (process.env.CONFIG_URL)
    configAxios = createAxiosInstance(process.env.CONFIG_URL + BASE_PATH)
  if (process.env.REPORTS_URL)
    reportsAxios = createAxiosInstance(process.env.REPORTS_URL + BASE_PATH)
  if (process.env.IDENTITY_URL)
    identityAxios = createAxiosInstance(process.env.IDENTITY_URL + BASE_PATH)
  if (process.env.DATA_URL)
    dataAxios = createAxiosInstance(process.env.DATA_URL + BASE_PATH)
  if (process.env.SPEED_URL)
    speedAxios = createAxiosInstance(process.env.SPEED_URL + BASE_PATH)
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
