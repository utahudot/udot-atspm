import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { useGetMenuItems } from '@/features/menuItems/api/getMenuItems'
import {
  useCreateMenuItem,
  useDeleteMenuItem,
  useEditMenuItem,
} from '@/features/menuItems/api/postMenuItems'
import MenuItemsModal from '@/features/menuItems/components/MenuItemModal'
import { MenuItems } from '@/features/menuItems/types/linkDto'
import { Backdrop, CircularProgress } from '@mui/material'

const MenuItemsAdmin = () => {
  const pageAccess = useViewPage(PageNames.MenuItems)
  const hasGeneralEditClaim = useUserHasClaim('GeneralConfiguration:Edit')
  const hasGeneralDeleteClaim = useUserHasClaim('GeneralConfiguration:Delete')

  const { mutateAsync: createMutation } = useCreateMenuItem()
  const { mutateAsync: deleteMutation } = useDeleteMenuItem()
  const { mutateAsync: editMutation } = useEditMenuItem()

  const {
    data: menuItemsData,
    isLoading,
    refetch: refetchMenuItems,
  } = useGetMenuItems()
  const menuItems = menuItemsData?.value

  if (pageAccess.isLoading) {
    return
  }

  const handleCreateMenuItem = async (menuItem: MenuItems) => {
    const { name, icon, displayOrder, document, parentId, link } = menuItem

    const sanitizedMenuItem: Partial<MenuItems> = {}

    if (name) sanitizedMenuItem.name = name
    if (icon) sanitizedMenuItem.icon = icon
    if (displayOrder) sanitizedMenuItem.displayOrder = displayOrder
    if (document) sanitizedMenuItem.document = document
    if (parentId) sanitizedMenuItem.parentId = parentId
    if (link) sanitizedMenuItem.link = link

    try {
      await createMutation(sanitizedMenuItem)
      refetchMenuItems()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteMenuItem = async (id: number) => {
    try {
      await deleteMutation(id)
      refetchMenuItems()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const handleEditMenuItem = async (menuItem: MenuItems) => {
    const { id, name, icon, displayOrder, document, parentId, link } = menuItem

    const sanitizedMenuItem: Partial<MenuItems> = {}

    if (name !== undefined) sanitizedMenuItem.name = name
    if (icon !== undefined) sanitizedMenuItem.icon = icon || null
    if (displayOrder !== undefined)
      sanitizedMenuItem.displayOrder = displayOrder || 0
    if (document !== undefined) sanitizedMenuItem.document = document || null
    if (parentId !== undefined) sanitizedMenuItem.parentId = parentId || null
    if (link !== undefined) sanitizedMenuItem.link = link

    try {
      await editMutation({ data: sanitizedMenuItem, id })
      refetchMenuItems()
    } catch (error) {
      console.error('Mutation Error:', error)
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

  const filteredData = menuItems.map((obj: MenuItems, index: number) => {
    return {
      id: obj.id,
      name: obj.name,
      link: obj.link,
      displayOrder: obj.displayOrder,
      document: obj.document,
      parentId: obj.parentId,
      children: obj.children,
    }
  })

  const sortMenuItems = (menuItems) => {
    const baseObjects = []
    const childObjects = []
    const nameMap = {}

    // Separate base objects and child objects, and create a name map
    menuItems.forEach((item) => {
      nameMap[item.id] = item.name
      if (item.parentId === null) {
        baseObjects.push(item)
      } else {
        childObjects.push(item)
      }
    })

    const childrenMap = {}

    childObjects.forEach((child) => {
      if (!childrenMap[child.parentId]) {
        childrenMap[child.parentId] = []
      }
      childrenMap[child.parentId].push(child)
    })

    const sortedMenuItems = baseObjects
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .flatMap((base) => {
        const children = childrenMap[base.id] || []
        const sortedChildren = children
          .sort((a, b) => a.displayOrder - b.displayOrder)
          .map((child) => ({
            ...child,
            parentIdName: nameMap[child.parentId] || null,
          }))
        return [{ ...base, parentIdName: null }, ...sortedChildren]
      })

    return sortedMenuItems
  }

  const sortedFilteredData = sortMenuItems(filteredData)

  const headers = ['Name', 'Link', 'Display Order', 'ParentId Name']
  const headerKeys = ['name', 'link', 'displayOrder', 'parentIdName']

  return (
    <ResponsivePageLayout title="Manage Menu Items" noBottomMargin>
      <AdminTable
        pageName="Menu Item"
        headers={headers}
        headerKeys={headerKeys}
        data={sortedFilteredData}
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
