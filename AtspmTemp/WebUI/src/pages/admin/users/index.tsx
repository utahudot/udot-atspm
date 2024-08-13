import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import UserModal from '@/components/GenericAdminChart/UserModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useDeleteUser } from '@/features/identity/api/deleteUser'
import { useGetAllUsers } from '@/features/identity/api/getAllUsers'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'

const UsersAdmin = () => {
  const { mutate: deleteUserMutation } = useDeleteUser()

  const pageAccess = useViewPage(PageNames.Users)
  const hasEditClaim = useUserHasClaim('User:Edit')
  const hasDeleteClaim = useUserHasClaim('User:Delete')
  const { data: allUserData, isLoading: usersIsLoading } = useGetAllUsers()

  const handleDeleteUser = async (userId: string) => {
    try {
      await deleteUserMutation(userId)
      console.log('User deleted successfully')
    } catch (error) {
      console.error('Error deleting user:', error)
    }
  }

  if (pageAccess.isLoading) {
    return
  }

  const deleteUser = (data: any) => {
    handleDeleteUser(data.id)
  }

  const editUser = () => {
    // not needed
  }

  const createUser = () => {
    // not Needed
  }

  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.Users
  ) as GridColDef[]

  const filteredData = allUserData?.map((user: any) => {
    return {
      id: user.userId,
      firstName: user.firstName,
      lastName: user.lastName,
      fullName: user.fullName,
      userName: user.userName,
      agency: user.agency,
      email: user.email,
      roles: user.roles,
    }
  })

  const baseType = {
    name: '',
  }

  if (!usersIsLoading) console.log(allUserData)

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
    <ResponsivePageLayout title={'Manage Users'}>
      <GenericAdminChart
        pageName={PageNames.Users}
        headers={headers}
        data={filteredData}
        baseRowType={baseType}
        onDelete={deleteUser}
        onEdit={editUser}
        onCreate={createUser}
        customModal={<UserModal />}
        hasEditPrivileges={hasEditClaim}
        hasDeletePrivileges={hasDeleteClaim}
      />
    </ResponsivePageLayout>
  )
}

export default UsersAdmin
