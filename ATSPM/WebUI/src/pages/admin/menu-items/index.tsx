import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import MenuItemsModal from '@/components/GenericAdminChart/MenuItemModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { useGetMenuItems } from '@/features/links/api/getMenuItems'
import {
  useCreateMenuItem,
  useDeleteMenuItem,
  useEditMenuItem,
} from '@/features/links/api/postMenuItems'
import { MenuItems } from '@/features/links/types/linkDto'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'

const MenuItemsAdmin = () => {
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.MenuItems
  ) as GridColDef[]

  const pageAccess = useViewPage(PageNames.MenuItems)
  const hasEditClaim = useUserHasClaim('GeneralConfiguration:Edit')
  const hasDeleteClaim = useUserHasClaim('GeneralConxfiguration:Delete')

  const {
    data: menuItemsData,
    isLoading,
    refetch: refetchMenuItems,
  } = useGetMenuItems()
  const createMutation = useCreateMenuItem()
  const { mutate: editMenuItemMutate } = useEditMenuItem()
  const deleteMutation = useDeleteMenuItem()

  if (pageAccess.isLoading) {
    return
  }

  const handleCreateLink = async (menuItem: MenuItems) => {
    const { name, icon, displayOrder, document, parentId, link } = menuItem

    const sanitizedMenuItem: Partial<MenuItems> = {}

    if (name) sanitizedMenuItem.name = name
    if (icon) sanitizedMenuItem.icon = icon
    if (displayOrder) sanitizedMenuItem.displayOrder = displayOrder
    if (document) sanitizedMenuItem.document = document
    if (parentId) sanitizedMenuItem.parentId = parentId
    if (link) sanitizedMenuItem.link = link

    try {
      createMutation.mutateAsync(sanitizedMenuItem)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteLink = async (menuItems: MenuItems) => {
    const { id } = menuItems
    try {
      deleteMutation.mutateAsync(id)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const handleEditLink = async (menuItem: MenuItems) => {
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
      editMenuItemMutate(
        { data: sanitizedMenuItem, id },
        {
          onSuccess: () => {
            refetchMenuItems().then(()=>{console.log(menuItemsData)})
            
          },
        }
      )
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const deleteLink = (data: MenuItems) => {
    HandleDeleteLink(data)
  }

  const editLink = (data: MenuItems) => {
    handleEditLink(data)
  }

  const createLink = (data: MenuItems) => {
    handleCreateLink(data)
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!menuItemsData) {
    return <div>Error returning data</div>
  }

  const filteredData = menuItemsData?.value.map(
    (obj: MenuItems, index: number) => {
      return {
        id: obj.id,
        name: obj.name,
        link: obj.link,
        displayOrder: obj.displayOrder,
        document: obj.document,
        parentId: obj.parentId,
        children: obj.children,
      }
    }
  )

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
        return [
          { ...base, parentIdName: null },
          ...sortedChildren,
        ]
      })

    return sortedMenuItems
  }

  const sortedFilteredData = sortMenuItems(filteredData)

  console.log(sortedFilteredData)
  const baseType = {
    name: '',
    icon: '',
    displayOrder: '',
    link: '',
    document: '',
    parentIdName: '',
  }

  return (
    <ResponsivePageLayout title={'Menu Items'}>
      <GenericAdminChart
        headers={headers}
        pageName={PageNames.MenuItems}
        data={sortedFilteredData}
        baseRowType={baseType}
        onDelete={deleteLink}
        onEdit={editLink}
        onCreate={createLink}
        hasEditPrivileges={hasEditClaim}
        hasDeletePrivileges={hasDeleteClaim}
        customModal={
          <MenuItemsModal
            onCreate={createLink}
            onEdit={editLink}
            onDelete={deleteLink}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default MenuItemsAdmin
