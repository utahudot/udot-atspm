// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - editData.ts
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
import { configAxios } from '@/lib/axios'
import { MutationConfig } from '@/lib/react-query'

type UsePostOptions = {
  apiCall: string
  //   data: any
  config?: MutationConfig<typeof editData>
}

interface EditDataDTO {
  apiCall: string
  data: any
}

export const editData = ({ apiCall, data }: EditDataDTO): Promise<any> => {
  const response = configAxios.patch(apiCall, data)

  return response
}

// export const useEditData = ({ apiCall, config = {} }: UsePostOptions) => {
//   const { addNotification } = useNotificationStore()
//   return useMutation({
//     onMutate: async (obj: any) => {
//       await queryClient.cancelQueries([apiCall])

//       const previousObjs = queryClient.getQueryData<any[]>([apiCall])

//       queryClient.setQueryData([apiCall], [...(previousObjs || []), obj.data])

//       return { previousObjs }
//     },
//     onError: (_, __, context: any) => {
//       if (context?.previousObjs) {
//         queryClient.setQueryData([apiCall], context.previousObjs)
//       }
//     },
//     onSuccess: () => {
//       queryClient.invalidateQueries([apiCall])
//       addNotification({
//         type: 'success',
//         title: `${apiCall} Created`,
//       })
//     },
//     ...config,
//     mutationFn: editData,
//   })
// }
