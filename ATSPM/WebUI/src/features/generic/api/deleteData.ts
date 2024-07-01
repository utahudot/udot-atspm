import { configAxios } from '@/lib/axios'
import { MutationConfig } from '@/lib/react-query'

type UsePostOptions = {
  apiCall: string
  //   data: any
  config?: MutationConfig<typeof deleteData>
}

interface DeleteDataDto {
  apiCall: string
  data: any
}

export const deleteData = ({ apiCall, data }: DeleteDataDto): Promise<any> => {
  const response = configAxios.delete(apiCall, data)

  return response
}

// export const useDeleteData = ({ apiCall, config = {} }: UsePostOptions) => {
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
//     mutationFn: deleteData,
//   })
// }
