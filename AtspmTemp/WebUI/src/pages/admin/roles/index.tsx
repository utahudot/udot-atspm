import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import RoleModal from '@/components/GenericAdminChart/RoleModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useGetRoles } from '@/features/identity/api/getRoles'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { Role } from '@/features/identity/types/roles'
import { useDeleteRole } from '@/features/roles/api/deleteRoles'
import { usePostRoleName } from '@/features/roles/api/postRolesName'
import { navigateToPage } from '@/utils/routes'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'

const RolesAdmin = () => {
  const pageAccess = useViewPage(PageNames.Roles)
  const hasEditClaim = useUserHasClaim('Role:Edit')
  const hasDeleteClaim = useUserHasClaim('Role:Delete')
  const { data, isLoading, error } = useGetRoles()
  const {
    mutate: postRoleName,
    isLoading: isPostingRole,
    error: postError,
  } = usePostRoleName()
  const {
    mutate: deleteRoleMutation,
    isLoading: isDeletingRole,
    error: deleteError,
  } = useDeleteRole()

  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.Roles
  ) as GridColDef[]

  // TODO - MAKE SURE THIS LOGIC ISCODED IN BACKEND. HACKERS COULD EAILSY START DELELTING PROTECTED ROLES WHICH WE DON'T WANT OB
  const protectedRoles = [
    'ReportAdmin',
    'Admin',
    'RoleAdmin',
    'LocationConfigurationAdmin',
    'GeneralConfigurationAdmin',
    'DataAdmin',
    'WatchdogSubscriber',
    'UserAdmin',
  ]

  const HandleDeleteRole = async (roleData: Role) => {
    const { role: roleName } = roleData

    if (protectedRoles.includes(roleName)) {
      return
    }

    try {
      await deleteRoleMutation(roleName)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditRole = async (roleData: Role) => {
    const { id, role, claims } = roleData
    try {
      navigateToPage(`/admin/roles/${id}/edit`)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleCreateRole = async (roleName: string) => {
    try {
      await postRoleName({ roleName })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  if (pageAccess.isLoading) {
    return
  }

  const deleteRole = (data: Role) => {
    HandleDeleteRole(data)
  }

  const editRole = (data: Role) => {
    HandleEditRole(data)
  }

  const createRole = (data: {
    name: string
    id: null
    isNew: true
    role: string
  }) => {
    const { role } = data
    HandleCreateRole(role)
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

  const filteredData = data
    ?.map((role: Role, index: number) => {
      return {
        id: index,
        role: role.role,
        // claims: role.claims,
      }
    })
    .sort((a, b) => a.role.localeCompare(b.role))
  const baseType = {
    name: '',
  }
  return (
    <ResponsivePageLayout title={'Manage Roles'}>
      <GenericAdminChart
        pageName={PageNames.Roles}
        headers={headers}
        data={filteredData}
        baseRowType={baseType}
        onDelete={deleteRole}
        onEdit={editRole}
        onCreate={createRole}
        customModal={<RoleModal />}
        protectedItems={protectedRoles}
        hasEditPrivileges={hasEditClaim}
        hasDeletePrivileges={hasDeleteClaim}
      />
    </ResponsivePageLayout>
  )
}

export default RolesAdmin
