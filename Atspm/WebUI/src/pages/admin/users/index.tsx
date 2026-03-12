import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useDeleteUser } from '@/features/identity/api/deleteUser'
import { useEditUsers } from '@/features/identity/api/editUsers'
import { useGetAllUsers } from '@/features/identity/api/getAllUsers'
import UserModal from '@/features/identity/components/users/UserModal'
import { UserRolesCell } from '@/features/identity/components/users/UserRolesCell'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import UserDto from '@/features/identity/types/userDto'
import { useNotificationStore } from '@/stores/notifications'
import { toUTCDateStamp } from '@/utils/dateTime'
import { Backdrop, CircularProgress } from '@mui/material'

const UsersAdmin = () => {
  const pageAccess = useViewPage(PageNames.Users)
  const hasUserEditClaim = useUserHasClaim('User:Edit')
  const hasUserDeleteClaim = useUserHasClaim('User:Delete')

  const { addNotification } = useNotificationStore()

  const { mutateAsync: deleteMutation } = useDeleteUser()
  const { mutateAsync: editMutation } = useEditUsers()
  const {
    data: allUserData,
    isLoading: usersIsLoading,
    refetch: refetchUsers,
  } = useGetAllUsers()

  const users = allUserData

  const handleEditUser = async (userData) => {
    const { userId, firstName, lastName, agency, userName, email, roles } =
      userData
    try {
      await editMutation({
        userId,
        firstName,
        lastName,
        agency,
        email: email.toLowerCase(),
        userName: userName.toLowerCase(),
        roles,
      })
      addNotification({
        title: `User updated successfully.`,
        type: 'success',
      })
      refetchUsers()
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        title: `Error updating user: ${error.message}`,
        type: 'error',
      })
    }
  }

  const handleDeleteUser = async (id: number) => {
    try {
      await deleteMutation(id)
      addNotification({
        title: `User deleted successfully.`,
        type: 'success',
      })
      refetchUsers()
    } catch (error) {
      console.error('Error deleting user:', error)
      addNotification({
        title: `Error deleting user: ${error.message}`,
        type: 'error',
      })
    }
  }

  if (pageAccess.isLoading) {
    return
  }

  const filteredData = users?.map((user) => {
    return {
      ...user,
      roles: user.roles?.sort(),
      created: user.created ? toUTCDateStamp(user.created) : '',
      modified: user.modified ? toUTCDateStamp(user.modified) : '',
    }
  })

  const cells = [
    {
      key: 'fullName',
      label: 'Full Name',
    },
    {
      key: 'userName',
      label: 'Username',
    },
    {
      key: 'email',
      label: 'Email',
    },
    {
      key: 'agency',
      label: 'Agency',
    },
    {
      key: 'roles',
      label: 'Roles',
      component: UserRolesCell,
    },
  ]

  if (usersIsLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!allUserData) {
    return <div>Error returning data</div>
  }

  return (
    <ResponsivePageLayout title="Manage Users" noBottomMargin>
      <AdminTable
        pageName="User"
        cells={cells}
        data={filteredData}
        hasEditPrivileges={hasUserEditClaim}
        hasDeletePrivileges={hasUserDeleteClaim}
        hideAuditProperties
        editModal={
          <UserModal isOpen={true} onSave={handleEditUser} data={null} />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name={''}
            deleteByKey="userId"
            objectType="User"
            deleteLabel={(selectedRow: UserDto) => selectedRow.fullName}
            open={false}
            onConfirm={handleDeleteUser}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default UsersAdmin
