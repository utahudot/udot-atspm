import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import RoleModal from '@/components/GenericAdminChart/RoleModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useAddRoleClaims } from '@/features/identity/api/addRoleClaims'
import { useGetRoles } from '@/features/identity/api/getRoles'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { Role } from '@/features/identity/types/roles'
import { useDeleteRole } from '@/features/roles/api/deleteRoles'
import { usePostRoleName } from '@/features/roles/api/postRolesName'
import { Backdrop, CircularProgress } from '@mui/material'

const RolesAdmin = () => {
  const pageAccess = useViewPage(PageNames.Roles)
  const hasRoleEditClaim = useUserHasClaim('Role:Edit')
  const hasRolesDeleteClaim = useUserHasClaim('Role:Delete')

  const { data: allRolesData, isLoading, refetch: refetchRoles } = useGetRoles()
  const roles = allRolesData

  const { mutateAsync: createMutation } = usePostRoleName()
  const { mutateAsync: deleteMutation } = useDeleteRole()
  const { mutateAsync: editMutation } = useAddRoleClaims()

  // TODO - MAKE SURE THIS LOGIC ISCODED IN BACKEND.
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

  const HandleDeleteRole = async (roleName: string) => {
    if (protectedRoles.includes(roleName)) {
      return
    }
    try {
      await deleteMutation(roleName)
      refetchRoles()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditRole = async (roleData: {
    roleName: string
    claims: string[]
  }) => {
    try {
      await editMutation({
        roleName: roleData.roleName,
        claims: roleData.claims,
      })
      refetchRoles()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleCreateRole = async (roleData: {
    roleName: string
    claims: string[]
  }) => {
    try {
      await createMutation({ roleName: roleData.roleName })
      if (roleData.claims.length > 0) {
        await editMutation({
          roleName: roleData.roleName,
          claims: roleData.claims,
        })
      }
      refetchRoles()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  if (pageAccess.isLoading) {
    return
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

  if (!roles) {
    return <div>Error returning data</div>
  }

  const filteredData = roles
    .map((role: Role, index: number) => {
      return {
        id: index,
        role: role.role,
      }
    })
    .sort((a, b) => a.role.localeCompare(b.role))

  const headers = ['Role']
  const headerKeys = ['role']

  return (
    <ResponsivePageLayout title="Manage Roles" noBottomMargin>
      <AdminTable
        pageName="Role"
        headers={headers}
        headerKeys={headerKeys}
        data={filteredData}
        hasEditPrivileges={hasRoleEditClaim}
        hasDeletePrivileges={hasRolesDeleteClaim}
        protectedFromDeleteItems={protectedRoles}
        editModal={
          <RoleModal
            isOpen={true}
            onSave={HandleEditRole}
            onClose={onModalClose}
          />
        }
        createModal={
          <RoleModal
            isOpen={true}
            onSave={HandleCreateRole}
            onClose={onModalClose}
          />
        }
        deleteModal={
          <DeleteModal
            id={''}
            name={''}
            objectType="Role"
            deleteByKey="role"
            open={false}
            onClose={() => {}}
            onConfirm={HandleDeleteRole}
            deleteLabel={(selectedRow: Role) => selectedRow.role}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default RolesAdmin
