import {
  MenuItem,
  useDeleteMenuItemsFromKey,
  useGetMenuItems,
  usePatchMenuItemsFromKey,
  usePostMenuItems,
} from '@/api/config'
import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import MenuItemsModal from '@/features/menuItems/components/MenuItemModal'
import { useNotificationStore } from '@/stores/notifications'
import { Backdrop, CircularProgress } from '@mui/material'

const MenuItemsAdmin = () => {
  const pageAccess = useViewPage(PageNames.MenuItems)
  const hasGeneralEditClaim = useUserHasClaim('GeneralConfiguration:Edit')
  const hasGeneralDeleteClaim = useUserHasClaim('GeneralConfiguration:Delete')

  const { addNotification } = useNotificationStore()

  const { mutateAsync: addMenuItem } = usePostMenuItems()
  const { mutateAsync: deleteMenuItem } = useDeleteMenuItemsFromKey()
  const { mutateAsync: editMenuItem } = usePatchMenuItemsFromKey()

  const {
    data: menuItemsData,
    isLoading,
    refetch: refetchMenuItems,
  } = useGetMenuItems()
  const menuItems = menuItemsData?.value

  if (pageAccess.isLoading) {
    return
  }

  const handleCreateMenuItem = async (menuItem: MenuItem) => {
    const { name, icon, displayOrder, document, parentId, link } = menuItem

    const sanitizedMenuItem: Partial<MenuItem> = {}

    if (name) sanitizedMenuItem.name = name
    if (icon) sanitizedMenuItem.icon = icon
    if (displayOrder) sanitizedMenuItem.displayOrder = displayOrder
    if (document) sanitizedMenuItem.document = document
    if (parentId) sanitizedMenuItem.parentId = parentId
    if (link) sanitizedMenuItem.link = link

    try {
      await addMenuItem({ data: menuItem })
      addNotification({
        type: 'success',
        title: 'Menu Item Created',
      })
      refetchMenuItems()
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        type: 'error',
        title: 'Error Creating Menu Item',
      })
    }
  }

  const HandleDeleteMenuItem = async (id: number) => {
    try {
      await deleteMenuItem({ key: id })
      refetchMenuItems()
      addNotification({
        type: 'success',
        title: 'Menu Item Deleted',
      })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        type: 'error',
        title: 'Error Deleting Menu Item',
      })
    }
  }

  const handleEditMenuItem = async (menuItem: MenuItem) => {
    const { id, name, icon, displayOrder, document, parentId, link } = menuItem

    const sanitizedMenuItem: Partial<MenuItem> = {}

    if (name !== undefined) sanitizedMenuItem.name = name
    if (icon !== undefined) sanitizedMenuItem.icon = icon || null
    if (displayOrder !== undefined)
      sanitizedMenuItem.displayOrder = displayOrder || 0
    if (document !== undefined) sanitizedMenuItem.document = document || null
    if (parentId !== undefined) sanitizedMenuItem.parentId = parentId || null
    if (link !== undefined) sanitizedMenuItem.link = link

    try {
      await editMenuItem({ data: sanitizedMenuItem, key: id })
      refetchMenuItems()
      addNotification({
        type: 'success',
        title: 'Menu Item Edited',
      })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        type: 'error',
        title: 'Error Editing Menu Item',
      })
    }
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!menuItems) {
    return <div>Error returning data</div>
  }

  const onModalClose = () => {
    //do something?? potentially just delete
  }

  const headers = ['Name', 'Link', 'Display Order', 'Parent']
  const headerKeys = ['name', 'link', 'displayOrder', 'parentIdName']

  return (
    <ResponsivePageLayout title="Manage Menu Items" noBottomMargin>
      <AdminTable
        pageName="Menu Item"
        headers={headers}
        headerKeys={headerKeys}
        data={menuItems}
        hasEditPrivileges={hasGeneralEditClaim}
        hasDeletePrivileges={hasGeneralDeleteClaim}
        editModal={
          <MenuItemsModal
            isOpen={true}
            onSave={handleEditMenuItem}
            onClose={onModalClose}
          />
        }
        createModal={
          <MenuItemsModal
            isOpen={true}
            onSave={handleCreateMenuItem}
            onClose={onModalClose}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name={''}
            objectType="Menu Item"
            open={false}
            onClose={() => {}}
            onConfirm={HandleDeleteMenuItem}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default MenuItemsAdmin
