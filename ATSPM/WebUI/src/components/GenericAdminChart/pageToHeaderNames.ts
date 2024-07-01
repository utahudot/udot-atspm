import { PageNames } from '@/features/identity/pagesCheck'
import { GridColDef } from '@mui/x-data-grid'

const areaHeaders: GridColDef[] = [
  {
    field: 'name',
    headerName: 'Area Name',
    editable: true,
    flex: 1,
  },
]

const regionHeaders: GridColDef[] = [
  {
    field: 'description',
    headerName: 'Region Name',
    editable: true,
    flex: 1,
  },
]

const faqHeaders: GridColDef[] = [
  {
    field: 'header',
    headerName: 'Header',
    flex: 2,
    sortable: false,
    headerAlign: 'center',
    editable: true,
  },
  {
    field: 'body',
    headerName: 'Body',
    flex: 4,
    sortable: false,
    headerAlign: 'center',
    editable: true,
  },
  {
    field: 'displayOrder',
    headerName: 'Order',
    flex: 0.5,
    editable: true,
    type: 'number',
  },
]

const jurisdictionHeaders: GridColDef[] = [
  {
    field: 'name',
    headerName: 'Jurisdiction Name',
    flex: 1,
    editable: true,
  },
  {
    field: 'mpo',
    headerName: 'MPO',
    flex: 1,
    editable: true,
  },
  {
    field: 'countyParish',
    headerName: 'County/Parish Name',
    flex: 1,
    editable: true,
  },
  {
    field: 'otherPartners',
    headerName: 'Other Partners',
    flex: 1,
    editable: true,
  },
]

const menuItemsHeaders: GridColDef[] = [
  {
    field: 'name',
    headerName: 'Name',
    editable: true,
    flex: 1,
  },
  {
    field: 'link',
    headerName: 'Link',
    editable: true,
    flex: 1,
  },
  {
    field: 'displayOrder',
    headerName: 'Display Order',
    editable: true,
    flex: 0.5,
  },
  // {
  //   field: 'document',
  //   headerName: 'Document',
  //   editable: true,
  //   flex: 0.5,
  // },
  {
    field: 'parentIdName',
    headerName: 'ParentId Name',
    editable: true,
    flex: 0.25,
  },
]

const routeHeaders: GridColDef[] = [
  {
    field: 'name',
    headerName: 'Route Name',
    editable: true,
    flex: 1,
  },
]

const rolesHeaders: GridColDef[] = [
  {
    field: 'role',
    headerName: 'Role',
    editable: true,
    flex: 1,
  },
]

const usersHeaders: GridColDef[] = [
  {
    field: 'fullName',
    headerName: 'Name',
    editable: true,
    flex: 1,
  },
  {
    field: 'userName',
    headerName: 'Username',
    editable: true,
    flex: 1,
  },
  {
    field: 'agency',
    headerName: 'Agency',
    editable: true,
    flex: 1,
  },
  {
    field: 'email',
    headerName: 'Email',
    editable: true,
    flex: 2,
  },
  {
    field: 'roles',
    headerName: 'Roles',
    editable: true,
    flex: 2,
  },
]

const productHeaders: GridColDef[] = [
  {
    field: 'manufacturer',
    headerName: 'Manufacturer',
    editable: true,
    flex: 1,
  },
  {
    field: 'model',
    headerName: 'Model',
    editable: true,
    flex: 1,
  },
  {
    field: 'webPage',
    headerName: 'Web Page',
    editable: true,
    flex: 1,
  },
  {
    field: 'notes',
    headerName: 'Notes',
    editable: true,
    flex: 1,
  },
]

const deviceHeaders: GridColDef[] = [
  {
    field: 'firmware',
    headerName: 'Firmware',
    editable: true,
    flex: 1,
  },
  {
    field: 'notes',
    headerName: 'Notes',
    editable: true,
    flex: 1,
  },
  {
    field: 'protocol',
    headerName: 'Protocol',
    editable: true,
    flex: 1,
  },
  {
    field: 'port',
    headerName: 'Port',
    editable: true,
    flex: 1,
  },
  {
    field: 'directory',
    headerName: 'Directory',
    editable: true,
    flex: 1,
  },
  {
    field: 'searchTerms',
    headerName: 'Search Terms',
    editable: true,
    flex: 1,
  },
  {
    field: 'connectionTimeout',
    headerName: 'Connection Timeout',
    editable: true,
    flex: 1,
  },
  {
    field: 'operationTimeout',
    headerName: 'Operation Timeout',
    editable: true,
    flex: 1,
  },
  {
    field: 'dataModel',
    headerName: 'Data Model',
    editable: true,
    flex: 1,
  },
  {
    field: 'userName',
    headerName: 'Username',
    editable: true,
    flex: 1,
  },
  {
    field: 'password',
    headerName: 'Password',
    editable: true,
    flex: 1,
  },
  {
    field: 'productName',
    headerName: 'Product Name',
    editable: true,
    flex: 1,
  },
]

export const pageNameToHeaders: Map<string, GridColDef[]> = new Map()
  .set(PageNames.Areas, areaHeaders)
  .set(PageNames.Jurisdiction, jurisdictionHeaders)
  .set(PageNames.Region, regionHeaders)
  .set(PageNames.FAQs, faqHeaders)
  .set(PageNames.MenuItems, menuItemsHeaders)
  .set(PageNames.Routes, routeHeaders)
  .set(PageNames.Roles, rolesHeaders)
  .set(PageNames.Users, usersHeaders)
  .set(PageNames.Products, productHeaders)
  .set(PageNames.DeviceConfigurations, deviceHeaders)
