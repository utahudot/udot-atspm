import { MenuItems } from '@/features/links/types/linkDto'


export const transformMenuItems = (menuItemsData: MenuItems[]): MenuItems[] => {
  if (!menuItemsData) {
    return []
  }

  const menuItemsMap: { [id: number]: MenuItems } = {}

  menuItemsData.forEach((item) => {
    menuItemsMap[item.id] = {
      id: item.id,
      name: item.name,
      icon: item.icon ? item.icon : null,
      link: item.link,
      parentId: item.parentId,
      displayOrder: item.displayOrder,
      document: item.document,
      children: [],
    }
  })

  menuItemsData.forEach((item) => {
    if (item.parentId) {
      menuItemsMap[item.parentId].children.push(menuItemsMap[item.id])
    }
  })

  return menuItemsData
    .filter((item) => item.parentId === null)
    .map((item) => menuItemsMap[item.id])
}
