import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import DeviceConfigModal from '@/components/GenericAdminChart/DeviceConfigModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  useCreateDeviceConfiguration,
  useDeleteDeviceConfiguration,
  useEditDeviceConfiguration,
  useGetDeviceConfigurations,
} from '@/features/devices/api/deviceConfigurations'
import { useGetDevices } from '@/features/devices/api/devices'
import { DeviceConfiguration } from '@/features/devices/types/index'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { useGetProducts } from '@/features/products/api'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import { useEffect, useState } from 'react'

const DevicesAdmin = () => {
  const pageAccess = useViewPage(PageNames.DeviceConfigurations)

  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.DeviceConfigurations
  ) as GridColDef[]

  const hasEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasDeleteClaim = useUserHasClaim('LocationConfiguration:Delete')
  const { data: allDevicesData } = useGetDevices();
  const { data: deviceData, isLoading } = useGetDeviceConfigurations()
  const createMutation = useCreateDeviceConfiguration()
  const deleteMutation = useDeleteDeviceConfiguration()
  const editMutation = useEditDeviceConfiguration()
  const { data: productData, isLoading: productIsloading } = useGetProducts()
  useEffect(() => {
    if (deviceData) {
      setData(deviceData)
    }
  }, [isLoading])

  if (pageAccess.isLoading) {
    return
  }

  const HandleCreateDevice = async (deviceData: DeviceConfiguration) => {
    const {
      firmware,
      notes,
      protocol,
      port,
      directory,
      connectionTimeout,
      operationTimeout,
      userName,
      password,
      productId,
    } = deviceData

    //TODO: is there a better way to do this? Note from Dan: created sanitized object as backend will only accept vars that have value otherwise they shouldn't be passed through.
    const sanitizedDevice: Partial<DeviceConfiguration> = {}

    if (firmware) sanitizedDevice.firmware = firmware
    if (notes) sanitizedDevice.notes = notes
    if (protocol) sanitizedDevice.protocol = protocol
    if (port) sanitizedDevice.port = parseInt(port)
    if (directory) sanitizedDevice.directory = directory
    if (connectionTimeout)
      sanitizedDevice.connectionTimeout = parseInt(connectionTimeout)
    if (operationTimeout)
      sanitizedDevice.operationTimeout = parseInt(operationTimeout)
    if (userName) sanitizedDevice.userName = userName
    if (password) sanitizedDevice.password = password
    if (productId) sanitizedDevice.productId = parseInt(productId)

    try {
      createMutation.mutateAsync(sanitizedDevice)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteDevice = async (deviceData: DeviceConfiguration) => {
    const { id } = deviceData
    try {
      deleteMutation.mutateAsync(id)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditDevice = async (deviceData: DeviceConfiguration) => {
    const {
      id,
      firmware,
      notes,
      protocol,
      port,
      directory,
      searchTerms,
      connectionTimeout,
      operationTimeout,
      dataModel,
      userName,
      password,
      productId,
    } = deviceData

    //TODO: is there a better way to do this? Note from Dan: created sanitized object as backend will only accept vars that have value otherwise they shouldn't be passed through.
    const sanitizedDevice: Partial<DeviceConfiguration> = {}

    if (firmware) sanitizedDevice.firmware = firmware
    if (notes) sanitizedDevice.notes = notes
    if (protocol) sanitizedDevice.protocol = protocol
    if (port) sanitizedDevice.port = parseInt(port)
    if (directory) sanitizedDevice.directory = directory
    if (connectionTimeout)
      sanitizedDevice.connectionTimeout = parseInt(connectionTimeout)
    if (operationTimeout)
      sanitizedDevice.operationTimeout = parseInt(operationTimeout)
    if (userName) sanitizedDevice.userName = userName
    if (password) sanitizedDevice.password = password
    if (productId) sanitizedDevice.productId = parseInt(productId)

    try {
      editMutation.mutateAsync({
        data: sanitizedDevice,
        id,
      })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const deleteDevice = (data: DeviceConfiguration) => {
    HandleDeleteDevice(data)
  }

  const editDevice = (data: DeviceConfiguration) => {
    HandleEditDevice(data)
  }

  const createDevice = (data: DeviceConfiguration) => {
    HandleCreateDevice(data)
  }

  const getProductDetails = (productId: number) => {
    const product = productData?.value.find((p) => p.id === productId)
    return product ? `${product.model}` : ''
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!data) {
    return <div>Error returning data</div>
  }

  const filteredData = data?.value.map((obj: DeviceConfiguration) => {
    const productName = getProductDetails(obj.productId)
    return {
      id: obj.id,
      firmware: obj.firmware,
      notes: obj.notes,
      protocol: obj.protocol,
      port: obj.port,
      directory: obj.directory,
      connectionTimeout: obj.connectionTimeout,
      operationTimeout: obj.operationTimeout,
      userName: obj.userName,
      password: obj.password,
      productName: productName,
      productId: obj.productId,
    }
  })

  const baseType = {
    firmware: '',
    notes: '',
    protocol: '',
    port: '',
    directory: '',
    connectionTimeout: '',
    operationTimeout: '',
    userName: '',
    password: '',
    productName: '',
    productId: '',
  }


  return (
    <ResponsivePageLayout title={'Device Configurations'} noBottomMargin>
      <GenericAdminChart
        pageName={PageNames.DeviceConfigurations}
        headers={headers}
        data={filteredData}
        baseRowType={baseType}
        onDelete={deleteDevice}
        onEdit={editDevice}
        onCreate={createDevice}
        locations={allDevicesData?.value}
        hasEditPrivileges={hasEditClaim}
        hasDeletePrivileges={hasDeleteClaim}
        customModal={
          <DeviceConfigModal
            onCreate={createDevice}
            onEdit={editDevice}
            onDelete={deleteDevice}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default DevicesAdmin
