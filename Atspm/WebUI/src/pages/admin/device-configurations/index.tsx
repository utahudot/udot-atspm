import {
  Device,
  DeviceConfiguration,
  useDeleteDeviceConfigurationFromKey,
  useGetDevice,
  useGetDeviceConfiguration,
  useGetProduct,
  usePostDeviceConfiguration,
  usePutDeviceConfigurationFromKey,
} from '@/api/config'
import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { DeviceConfigCustomCellRender } from '@/features/devices/components/DeviceConfigCustomRenderCell'
import DeviceConfigModal from '@/features/devices/components/DeviceConfigModal'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { useNotificationStore } from '@/stores/notifications'
import { removeAuditFields } from '@/utils/removeAuditFields'
import { Backdrop, CircularProgress } from '@mui/material'

const DevicesAdmin = () => {
  const pageAccess = useViewPage(PageNames.DeviceConfigurations)
  const { addNotification } = useNotificationStore()
  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDeleteClaim = useUserHasClaim(
    'LocationConfiguration:Delete'
  )

  const { mutateAsync: createMutation } = usePostDeviceConfiguration()
  const { mutateAsync: deleteMutation } = useDeleteDeviceConfigurationFromKey()
  const { mutateAsync: editMutation } = usePutDeviceConfigurationFromKey()

  const { data: allDevicesData } = useGetDevice()
  const devices = allDevicesData?.value

  const {
    data: deviceConfigurationData,
    isLoading,
    refetch: refetchDeviceConfiguration,
  } = useGetDeviceConfiguration()
  const deviceConfigurations = deviceConfigurationData?.value

  const { data: productData } = useGetProduct()

  if (pageAccess.isLoading) {
    return
  }

  const handleCreateDeviceConfiguration = async (
    deviceConfigurationData: DeviceConfiguration
  ) => {
    try {
      const { id, ...dataWithoutId } = deviceConfigurationData
      await createMutation({ data: dataWithoutId })
      refetchDeviceConfiguration()
      addNotification({
        type: 'success',
        title: 'Device Configuration Created',
      })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        type: 'error',
        title: 'Error Creating Device Configuration',
      })
    }
  }

  const handleEditDeviceConfiguration = async (
    deviceConfigurationData: DeviceConfiguration
  ) => {
    try {
      const { productName, ...dataWithoutProductName } = deviceConfigurationData

      const deviceConfigDTO = removeAuditFields(dataWithoutProductName)

      await editMutation({
        data: deviceConfigDTO,
        key: deviceConfigurationData.id,
      })
      addNotification({
        title: 'Device Configuration Updated',
        type: 'success',
      })
      refetchDeviceConfiguration()
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        title: 'Device Configuration Updated',
        type: 'error',
      })
    }
  }

  const HandleDeleteDevice = async (id: number) => {
    try {
      await deleteMutation({ key: id })
      refetchDeviceConfiguration()
      addNotification({
        type: 'success',
        title: 'Device Configuration Deleted',
      })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        type: 'error',
        title: 'Device Configuration Delete Unsuccessful',
      })
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
      name: obj.product?.manufacturer + ' ' + obj.product?.model || '',
      productName: productName,
    }
  })

  const headers = [
    'Product Name',
    'Description',
    'Protocol',
    'Port',
    'Path',
    'Query',
    'Connection Properties',
    'Connection Timeout',
    'Operation Timeout',
    'Logging Offset',
    'Decoders',
    'Username',
    'Password',
  ]

  const headerKeys = [
    'productName',
    'description',
    'protocol',
    'port',
    'path',
    'query',
    'connectionProperties',
    'connectionTimeout',
    'operationTimeout',
    'loggingOffset',
    'decoders',
    'userName',
    'password',
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
        customCellRender={DeviceConfigCustomCellRender}
        editModal={
          <DeviceConfigModal
            isOpen={true}
            onSave={handleEditDeviceConfiguration}
            onClose={onModalClose}
          />
        }
        createModal={
          <DeviceConfigModal
            isOpen={true}
            onSave={handleCreateDeviceConfiguration}
            onClose={onModalClose}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name={'This device configuration'}
            deleteLabel={(selectedRow: (typeof filteredData)[number]) =>
              `${selectedRow.product?.manufacturer} ${selectedRow.product?.model}`
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
