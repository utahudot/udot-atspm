// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - detector.ts
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
import { useDeleteRequest } from '@/hooks/useDeleteRequest'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePatchRequest } from '@/hooks/usePatchRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { usePutRequest } from '@/hooks/usePutRequest'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

interface Comment {
  id: string | number
  comment: string
  timeStamp: string
  detectorId: string | number
}

const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

export function useCreateDetector() {
  const mutation = usePostRequest({ url: '/Detector', headers })
  return mutation
}

export function useEditDetector() {
  const mutation = usePatchRequest({ url: '/Detector', headers })
  return mutation
}

export function useDeleteDetector() {
  const mutation = useDeleteRequest({
    url: '/Detector',
    headers,
  })
  return mutation
}

export function usePutDetector() {
  return usePutRequest({
    url: '/Detector',
    headers,
    notify: false,
  })
}

export function useGetDetectorComments(detectorId: string) {
  return useGetRequest<ApiResponse<Comment>>({
    route: `/DetectorComment?detectorId=${detectorId}`,
    headers,
  })
}

export function useCreateDetectorComment() {
  const mutation = usePostRequest({
    url: '/DetectorComment',
    headers,
  })
  return mutation
}

export function useUpdateDetectorComment() {
  const mutation = usePatchRequest({
    url: '/DetectorComment',
    headers,
  })
  return mutation
}

export function useDeleteDetectorComment() {
  const mutation = useDeleteRequest({
    url: '/DetectorComment',
    headers,
  })
  return mutation
}
