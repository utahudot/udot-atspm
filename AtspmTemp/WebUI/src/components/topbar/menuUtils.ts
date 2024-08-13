// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - menuUtils.ts
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
