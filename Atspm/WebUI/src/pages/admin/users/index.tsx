import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import UserModal from '@/components/GenericAdminChart/UserModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useDeleteUser } from '@/features/identity/api/deleteUser'
import { useEditUsers } from '@/features/identity/api/editUsers'
import { useGetAllUsers } from '@/features/identity/api/getAllUsers'
import {
  CustomCellConfig,
  UserRolesCell,
} from '@/features/identity/components/users/UserRolesCell'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import UserDto from '@/features/identity/types/userDto'
import { Backdrop, CircularProgress } from '@mui/material'

const UsersAdmin = () => {
  const pageAccess = useViewPage(PageNames.Users)
  const hasUserEditClaim = useUserHasClaim('User:Edit')
  const hasUserDeleteClaim = useUserHasClaim('User:Delete')

  const { mutateAsync: deleteMutation } = useDeleteUser()
  const { mutateAsync: editMutation } = useEditUsers()
  const {
    data: allUserData,
    isLoading: usersIsLoading,
    refetch: refetchUsers,
  } = useGetAllUsers()

  const users = allUserData

  const handleEditUser = async (userData: UserDto) => {
    const {
      userId,
      firstName,
      lastName,
      agency,
      fullName,
      userName,
      email,
      roles,
    } = userData
    try {
      await editMutation({
        data: {
          userId,
          firstName,
          lastName,
          agency,
          fullName,
          email: email.toLowerCase(),
          userName: userName.toLowerCase(),
          roles,
        },
      })
      refetchUsers()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const handleDeleteUser = async (id: number) => {
    try {
      await deleteMutation(id)
      refetchUsers()
      console.log('User deleted successfully')
    } catch (error) {
      console.error('Error deleting user:', error)
    }
  }

  const onModalClose = () => {
    //do something?? potentially just delete
  }

  if (pageAccess.isLoading) {
    return
  }

  const filteredData = users?.map((user: UserDto) => {
    return {
      userId: user.userId,
      firstName: user.firstName,
      lastName: user.lastName,
      fullName: user.fullName,
      userName: user.userName,
      agency: user.agency,
      email: user.email,
      roles: user.roles,
    }
  })

  const headers = ['Full Name', 'Username', 'Agency', 'Email', 'Roles']
  const headerKeys = ['fullName', 'userName', 'agency', 'email', 'roles']

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

  const customCellRender: CustomCellConfig[] = [
    {
      headerKey: 'roles',
      component: (value: string, row: string[]) => (
        <UserRolesCell value={value} row={row} headerKey="roles" />
      ),
    },
  ]

  return (
    <ResponsivePageLayout title="Manage Users" noBottomMargin>
      <AdminTable
        pageName="User"
        headers={headers}
        headerKeys={headerKeys}
        data={filteredData}
        customCellRender={customCellRender}
        hasEditPrivileges={hasUserEditClaim}
        hasDeletePrivileges={hasUserDeleteClaim}
        editModal={
          <UserModal
            isOpen={true}
            onSave={handleEditUser}
            onClose={onModalClose}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name={''}
            objectType="User"
            deleteLabel={(selectedRow: UserDto) => selectedRow.fullName}
            open={false}
            onClose={() => {}}
            onConfirm={handleDeleteUser}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default UsersAdmin
