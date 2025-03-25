import {
  useDeleteApiV1RolesRoleName,
  useGetApiV1Roles,
  usePostApiV1Roles,
} from '@/api/identity/atspmAuthenticationApi'
import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useAddRoleClaims } from '@/features/identity/api/addRoleClaims'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { Role } from '@/features/identity/types/roles'
import RoleModal from '@/features/roles/components/RoleModal'
import { useNotificationStore } from '@/stores/notifications'
import { Backdrop, Box, CircularProgress, Typography } from '@mui/material'

const RolesAdmin = () => {
  const pageAccess = useViewPage(PageNames.Roles)
  const hasRoleEditClaim = useUserHasClaim('Role:Edit')
  const hasRolesDeleteClaim = useUserHasClaim('Role:Delete')
  const { addNotification } = useNotificationStore()

  const {
    data: allRolesData,
    isLoading,
    refetch: refetchRoles,
  } = useGetApiV1Roles()
  const roles = allRolesData

  const { mutateAsync: createMutation } = usePostApiV1Roles()
  const { mutateAsync: deleteMutation } = useDeleteApiV1RolesRoleName()
  const { mutateAsync: editMutation } = useAddRoleClaims()

  const builtInRoles = [
    {
      role: 'Admin',
      description:
        'Full access to all configurations, data, reports, and user management.',
    },
    {
      role: 'DataAdmin',
      description: 'Can access and export raw data logs from the export page.',
    },
    {
      role: 'GeneralConfigurationAdmin',
      description:
        'Can manage all system-wide configurations except location-specific settings.',
    },
    {
      role: 'LocationConfigurationAdmin',
      description: 'Can manage location-specific settings and configurations.',
    },
    {
      role: 'RoleAdmin',
      description: 'Can manage roles and permissions.',
    },
    {
      role: 'UserAdmin',
      description: 'Can manage Users.',
    },
    {
      role: 'ReportAdmin',
      description: 'Privileges are granted to access restricted reports.',
    },
    {
      role: 'WatchdogSubscriber',
      description:
        'Can view Watchdog reports and is subscribed to email notifications for Watchdog alerts.',
    },
  ]

  const protectedRoles: string[] = [
    'Admin',
    'DataAdmin',
    'GeneralConfigurationAdmin',
    'LocationConfigurationAdmin',
    'RoleAdmin',
    'UserAdmin',
    'ReportAdmin',
    'WatchdogSubscriber',
  ]

  const HandleDeleteRole = async (roleName: string) => {
    if (protectedRoles.includes(roleName)) {
      return;
    }
    try {
      await deleteMutation({ roleName })
      refetchRoles()
      addNotification({ title: 'Role Deleted', type: 'success' })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({ title: 'Delete role Unsuccessful', type: 'error' })
    }
  }

  const HandleEditRole = async (roleData: {
    roleName: string;
    claims: string[];
  }) => {
    try {
      await editMutation({
        roleName: roleData.roleName,
        claims: roleData.claims,
      })
      refetchRoles()
      addNotification({
        title: `${roleData.roleName} updated`,
        type: 'success',
      })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({ title: ' Role update Unsuccessful', type: 'error' })
    }
  }

  const HandleCreateRole = async (roleData: {
    roleName: string;
    claims: string[];
  }) => {
    try {
      await createMutation({
        data: {
          roleName: roleData.roleName,
        },
      })
      if (roleData.claims.length > 0) {
        await editMutation({
          roleName: roleData.roleName,
          claims: roleData.claims,
        });
      }
      refetchRoles()
      addNotification({
        title: `${roleData.roleName} created`,
        type: 'success',
      })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({ title: ' Role create Unsuccessful', type: 'error' })
    }
  }

  if (pageAccess.isLoading) {
    return;
  }

  const onModalClose = () => {
    //do something?? potentially just delete
  };

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    );
  }

  if (!roles) {
    return <div>Error returning data</div>;
  }

  const customRoleFilteredData = roles
    .filter((role: Role) => !builtInRoles.some((pr) => pr.role === role.role))
    .map((role: Role, index: number) => ({
      id: index,
      role: role.role,
    }))
    .sort((a: Role, b: Role) => a.role.localeCompare(b.role))

  const filteredDefaultRoles = builtInRoles.map((roleObj, index: number) => {
    return {
      id: index,
      name: roleObj.role.replace(/(?<!^)([A-Z])/g, ' $1'),
      description: roleObj.description,
    }
  })

  const defaultRoleHeaders = ['Name', 'Description']
  const defaultRoleKeys = ['name', 'description']

  const customRolesHeaders = ['Name']
  const customRoleKeys = ['role']

  return (
    <ResponsivePageLayout title="Manage Roles" noBottomMargin>
      <Box sx={{ marginBottom: 10 }}>
        <Typography variant="h5" fontWeight={'bold'} sx={{ mt: 2, mb: -6 }}>
          Built-in Roles
        </Typography>
        <AdminTable
          pageName="Role"
          headers={defaultRoleHeaders}
          headerKeys={defaultRoleKeys}
          data={filteredDefaultRoles}
        />
      </Box>
      <Typography variant="h5" fontWeight={'bold'} sx={{ mb: -2 }}>
        Custom Roles
      </Typography>
      <AdminTable
        pageName="Custom Role"
        headers={customRolesHeaders}
        headerKeys={customRoleKeys}
        data={customRoleFilteredData}
        hasEditPrivileges={hasRoleEditClaim}
        hasDeletePrivileges={hasRolesDeleteClaim}
        protectedFromDeleteItems={protectedRoles}
        editModal={
          <RoleModal
            isOpen={true}
            onSave={HandleEditRole}
            onClose={onModalClose}
            data={null}
          />
        }
        createModal={
          <RoleModal
            isOpen={true}
            onSave={HandleCreateRole}
            onClose={onModalClose}
            data={null}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
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
  );
};

export default RolesAdmin;
