interface linkDto {
  id: number
  name: string
  displayOrder: number
  url: string
}
export default linkDto

export interface MenuItems {
  id: number;
  name: string;
  icon?: string | null;
  displayOrder: number;
  link: string | null
  document?: Buffer | null;
  parentId: number | null;
  children: MenuItems[];
}

