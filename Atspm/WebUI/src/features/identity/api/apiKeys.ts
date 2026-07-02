// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - apiKeys.ts
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
import { identityAxios } from '@/lib/axios'
import { queryClient } from '@/lib/react-query'
import { useMutation, useQuery } from 'react-query'

export type ApiKeyScope = 'mine' | 'all'

export interface ApiKeyMetadata {
  id: number
  name: string
  ownerId: string
  ownerEmail: string
  ownerName: string
  createdAt: string
  expiresAt?: string | null
  isRevoked: boolean
  claims: string[]
}

export interface CreateApiKeyData {
  name: string
  expiresAt?: string | null
  claims: string[]
}

export interface CreatedApiKeyResponse {
  key: string
  message: string
}

type ApiKeyMetadataResponse = Partial<ApiKeyMetadata> & {
  id: number
}

interface UseApiKeysOptions {
  enabled?: boolean
  scope?: ApiKeyScope
}

const apiKeysBaseQueryKey = ['api-keys'] as const

export const apiKeysQueryKey = (scope?: ApiKeyScope) =>
  scope ? [...apiKeysBaseQueryKey, scope] : [...apiKeysBaseQueryKey]
export const identityClaimsQueryKey = ['identity-claims']

export const normalizeApiKey = (
  apiKey: ApiKeyMetadataResponse
): ApiKeyMetadata => {
  return {
    id: apiKey.id,
    name: apiKey.name ?? '',
    ownerId: apiKey.ownerId ?? '',
    ownerEmail: apiKey.ownerEmail ?? '',
    ownerName: apiKey.ownerName ?? '',
    createdAt: apiKey.createdAt ?? '',
    expiresAt: apiKey.expiresAt ?? null,
    isRevoked: Boolean(apiKey.isRevoked),
    claims: Array.isArray(apiKey.claims)
      ? apiKey.claims.filter(
          (claim): claim is string => typeof claim === 'string'
        )
      : [],
  }
}

const getApiKeys = async (scope: ApiKeyScope): Promise<ApiKeyMetadata[]> => {
  const path = scope === 'all' ? 'ApiKey/all-keys' : 'ApiKey/my-keys'
  const apiKeys = await identityAxios.get<unknown, ApiKeyMetadataResponse[]>(
    path
  )

  return apiKeys.map(normalizeApiKey)
}

const getIdentityClaims = async (): Promise<string[]> => {
  return identityAxios.get<unknown, string[]>('Claims')
}

const createApiKey = async (
  data: CreateApiKeyData
): Promise<CreatedApiKeyResponse> => {
  return identityAxios.post<unknown, CreatedApiKeyResponse>(
    'ApiKey/create',
    data
  )
}

const revokeApiKey = async (id: number): Promise<{ message: string }> => {
  return identityAxios.post<unknown, { message: string }>(`ApiKey/revoke/${id}`)
}

export const useApiKeys = (options: UseApiKeysOptions | boolean = true) => {
  const normalizedOptions =
    typeof options === 'boolean' ? { enabled: options } : options
  const scope = normalizedOptions.scope ?? 'mine'

  return useQuery<ApiKeyMetadata[]>(
    apiKeysQueryKey(scope),
    () => getApiKeys(scope),
    { enabled: normalizedOptions.enabled ?? true }
  )
}

export const useIdentityClaims = (enabled = true) => {
  return useQuery<string[]>(identityClaimsQueryKey, getIdentityClaims, {
    enabled,
  })
}

export const useCreateApiKey = () => {
  return useMutation(createApiKey, {
    onSuccess: () => {
      queryClient.invalidateQueries(apiKeysQueryKey())
    },
  })
}

export const useRevokeApiKey = () => {
  return useMutation(revokeApiKey, {
    onSuccess: () => {
      queryClient.invalidateQueries(apiKeysQueryKey())
    },
  })
}
