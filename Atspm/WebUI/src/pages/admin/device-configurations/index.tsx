import { Device } from '@/api/config/aTSPMConfigurationApi.schemas'
import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  useCreateDeviceConfiguration,
  useDeleteDeviceConfiguration,
  useEditDeviceConfiguration,
  useGetDeviceConfigurations,
} from '@/features/devices/api/deviceConfigurations'
import { useGetDevices } from '@/features/devices/api/devices'
import DeviceConfigModal from '@/features/devices/components/DeviceConfigModal'
import { DeviceConfiguration } from '@/features/devices/types/index'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import { useGetProducts } from '@/features/products/api'
import { Backdrop, CircularProgress } from '@mui/material'

const DevicesAdmin = () => {
  const pageAccess = useViewPage(PageNames.DeviceConfigurations)

  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDeleteClaim = useUserHasClaim(
    'LocationConfiguration:Delete'
  )

  const { mutateAsync: createMutation } = useCreateDeviceConfiguration()
  const { mutateAsync: deleteMutation } = useDeleteDeviceConfiguration()
  const { mutateAsync: editMutation } = useEditDeviceConfiguration()

  const { data: allDevicesData } = useGetDevices()
  const devices = allDevicesData?.value

  const { data: locationsData } = useLatestVersionOfAllLocations()
  const locations = locationsData?.value
  const {
    data: deviceConfigurationData,
    isLoading,
    refetch: refetchDeviceConfiguration,
  } = useGetDeviceConfigurations()
  const deviceConfigurations = deviceConfigurationData?.value

  const { data: productData, isLoading: productIsloading } = useGetProducts()

  if (pageAccess.isLoading) {
    return
  }

  const HandleCreateDevice = async (
    deviceConfigurationData: DeviceConfiguration
  ) => {
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
    } = deviceConfigurationData

    const sanitizedDeviceConfigurationData: Partial<DeviceConfiguration> = {}

    if (firmware) sanitizedDeviceConfigurationData.firmware = firmware
    if (notes) sanitizedDeviceConfigurationData.notes = notes
    if (protocol) sanitizedDeviceConfigurationData.protocol = protocol
    if (port) sanitizedDeviceConfigurationData.port = parseInt(port)
    if (directory) sanitizedDeviceConfigurationData.directory = directory
    if (connectionTimeout)
      sanitizedDeviceConfigurationData.connectionTimeout =
        parseInt(connectionTimeout)
    if (operationTimeout)
      sanitizedDeviceConfigurationData.operationTimeout =
        parseInt(operationTimeout)
    if (userName) sanitizedDeviceConfigurationData.userName = userName
    if (password) sanitizedDeviceConfigurationData.password = password
    if (productId)
      sanitizedDeviceConfigurationData.productId = parseInt(productId)

    try {
      await createMutation(sanitizedDeviceConfigurationData)
      refetchDeviceConfiguration()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteDevice = async (id: number) => {
    try {
      await deleteMutation(id)
      refetchDeviceConfiguration()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditDevice = async (
    deviceConfigurationData: DeviceConfiguration
  ) => {
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
    } = deviceConfigurationData

    const sanitizedDeviceConfigurationData: Partial<DeviceConfiguration> = {}

    if (firmware) sanitizedDeviceConfigurationData.firmware = firmware
    if (notes) sanitizedDeviceConfigurationData.notes = notes
    if (protocol) sanitizedDeviceConfigurationData.protocol = protocol
    if (port) sanitizedDeviceConfigurationData.port = parseInt(port)
    if (directory) sanitizedDeviceConfigurationData.directory = directory
    if (connectionTimeout)
      sanitizedDeviceConfigurationData.connectionTimeout =
        parseInt(connectionTimeout)
    if (operationTimeout)
      sanitizedDeviceConfigurationData.operationTimeout =
        parseInt(operationTimeout)
    if (userName) sanitizedDeviceConfigurationData.userName = userName
    if (password) sanitizedDeviceConfigurationData.password = password
    if (productId)
      sanitizedDeviceConfigurationData.productId = parseInt(productId)

    try {
      await editMutation({
        data: sanitizedDeviceConfigurationData,
        id,
      })
      refetchDeviceConfiguration()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const getProductDetails = (productId: number) => {
    const product = productData?.value.find((p) => p.id === productId)
    return product ? `${product.model}` : ''
  }

  const onModalClose = () => {
    //do something?? potentially just delete
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!deviceConfigurations) {
    return <div>Error returning data</div>
  }
  const filterAssociatedObjects = (
    deviceConfigurationId: number,
    objects: Device[]
  ): { id: number; name: string }[] => {
    const associatedDeviceLocations = objects.filter((object) => {
      return object.deviceConfigurationId === deviceConfigurationId
    })

    return associatedDeviceLocations.map((devices) => ({
      id: devices.location?.id ?? 0,
      name: `${devices.location?.primaryName ?? ''} & ${devices.location?.secondaryName ?? ''} - ${devices?.deviceType ?? ''}`,
    }))
  }

  const filteredData = deviceConfigurations.map((obj: DeviceConfiguration) => {
    const productName = getProductDetails(obj.productId)
    return {
      ...obj,
      productName: productName,
    }
  })

  const headers = [
    'Firmware',
    'Notes',
    'Protocol',
    'Port',
    'Directory',
    'Connection Timeout',
    'Operation Timeout',
    'Username',
    'Password',
    'Product Name',
  ]
  const headerKeys = [
    'firmware',
    'notes',
    'protocol',
    'port',
    'directory',
    'connectionTimeout',
    'operationTimeout',
    'userName',
    'password',
    'productName',
  ]

  return (
    <ResponsivePageLayout title="Device Configurations" noBottomMargin>
      <AdminTable
        pageName="Device Configuration"
        headers={headers}
        headerKeys={headerKeys}
        data={filteredData}
        hasEditPrivileges={hasLocationsEditClaim}
        hasDeletePrivileges={hasLocationsDeleteClaim}
        editModal={
          <DeviceConfigModal
            isOpen={true}
            onSave={HandleEditDevice}
            onClose={onModalClose}
          />
        }
        createModal={
          <DeviceConfigModal
            isOpen={true}
            onSave={HandleCreateDevice}
            onClose={onModalClose}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name={'test'}
            deleteLabel={(selectedRow: (typeof filteredData)[number]) =>
              `${selectedRow.product?.manufacturer} ${selectedRow.product?.model} ${selectedRow.firmware}`
            }
            objectType="Device Configuration"
            open={false}
            onClose={() => {}}
            onConfirm={HandleDeleteDevice}
            associatedObjects={devices}
            associatedObjectsLabel="devices and locations"
            filterFunction={filterAssociatedObjects}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default DevicesAdmin
