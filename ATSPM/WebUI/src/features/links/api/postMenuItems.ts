import { MenuItems } from "@/features/links/types/linkDto";
import { useDeleteRequest } from "@/hooks/useDeleteRequest";
import { usePatchRequest } from "@/hooks/usePatchRequest";
import { usePostRequest } from "@/hooks/usePostRequest";
import { configAxios } from "@/lib/axios";
import { AxiosHeaders } from 'axios';
import Cookies from 'js-cookie';

const route = "/MenuItems";
const token = Cookies.get('token');
const headers: AxiosHeaders = new AxiosHeaders({
    "Content-Type": 'application/json',
    getAuthorization: `Bearer ${token}`,
});


const axiosInstance = configAxios;

export function useCreateMenuItem() {
    const mutation = usePostRequest({url: route, axiosInstance, headers });
    return mutation;
}

  
  export function useEditMenuItem() {
    const mutation = usePatchRequest({ url: route, axiosInstance, headers })
    return mutation
  }
  
  export function useDeleteMenuItem() {
    const mutation = useDeleteRequest({ url: route, axiosInstance, headers })
    return mutation
  }












