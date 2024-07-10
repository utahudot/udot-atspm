import { useEditUsers } from '@/features/identity/api/editUsers'
import { useGetRoles } from '@/features/identity/api/getRoles'
import {
  Box,
  Button,
  Checkbox,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  SelectChangeEvent,
  TextField,
} from '@mui/material'
import React, { useEffect, useState } from 'react'

interface User {
  id: string
  firstName: string
  lastName: string
  userName: string
  agency: string
  email: string
  roles: string[]
}

interface ModalProps {
  open: boolean
  onClose: () => void
  data: User | null
  onSave: (user: User) => void
}

const UserModal: React.FC<ModalProps> = ({ open, onClose, data, onSave }) => {
  const [user, setUser] = useState<User | null>(null)
  const [usernameError, setUsernameError] = useState('')
  const [emailError, setEmailError] = useState('')
  const { data: roles, isLoading } = useGetRoles()
  const editUser = useEditUsers()

  useEffect(() => {
    setUser(data)
  }, [data])

  const handleChange = (
    event: React.ChangeEvent<{ name?: string; value: unknown }>
  ) => {
    const { name, value } = event.target
    setUser((prevUser) => {
      if (prevUser) {
        if (name === 'userName') {
          const usernameRegex = /^[a-zA-Z0-9_]+$/
          if (!usernameRegex.test(value as string)) {
            setUsernameError(
              'Username can only contain letters, numbers, and underscores'
            )
          } else {
            setUsernameError('')
          }
        }
        if (name === 'email') {
          const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
          if (!emailRegex.test(value as string)) {
            setEmailError('Please enter a valid email address')
          } else {
            setEmailError('')
          }
        }
        return {
          ...prevUser,
          [name as keyof User]: value,
        }
      }
      return prevUser
    })
  }
  const handleRolesChange = (event: SelectChangeEvent<string[]>) => {
    setUser((prevUser) => {
      if (prevUser) {
        return {
          ...prevUser,
          roles: event.target.value as string[],
        }
      }
      return prevUser
    })
  }

  const handleSubmit = async () => {
    if (user) {
      if (usernameError || emailError) {
        return
      }
      try {
        await editUser.mutateAsync({
          firstName: user.firstName,
          lastName: user.lastName,
          agency: user.agency,
          email: user.email.toLowerCase(),
          userName: user.userName.toLowerCase(),
          userId: user.id,
          fullName: `${user.firstName} ${user.lastName}`,
          roles: user.roles,
        })
        onSave({
          ...user,
          email: user.email.toLowerCase(),
          userName: user.userName.toLowerCase(),
        })
        onClose()
      } catch (error) {
        console.error('Error updating user profile:', error)
      }
    }
  }
  if (isLoading) return <div>conent is loading...</div>

  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>User Details</DialogTitle>
      <DialogContent>
        {user ? (
          <>
            <TextField
              autoFocus
              margin="dense"
              name="firstName"
              label="First Name"
              type="text"
              fullWidth
              value={user.firstName}
              onChange={handleChange}
            />
            <TextField
              autoFocus
              margin="dense"
              name="lastName"
              label="Last Name"
              type="text"
              fullWidth
              value={user.lastName}
              onChange={handleChange}
            />
            <TextField
              margin="dense"
              name="userName"
              label="Username"
              type="text"
              fullWidth
              value={user.userName}
              onChange={handleChange}
              error={Boolean(usernameError)}
              helperText={usernameError}
            />
            <TextField
              margin="dense"
              name="agency"
              label="Agency"
              type="text"
              fullWidth
              value={user.agency}
              onChange={handleChange}
            />
            <TextField
              margin="dense"
              name="email"
              label="Email"
              type="email"
              fullWidth
              value={user.email}
              onChange={handleChange}
              error={Boolean(emailError)}
              helperText={emailError}
            />
            <FormControl fullWidth margin="dense">
              <InputLabel>Roles</InputLabel>
              <Select
                multiple
                label="Roles"
                value={user.roles}
                onChange={handleRolesChange}
                renderValue={(selected) => (
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                    {selected.map((value) => (
                      <Chip key={value} label={value} />
                    ))}
                  </Box>
                )}
              >
                {roles
                  ?.sort((a, b) => a.role.localeCompare(b.role))
                  .map((role) => (
                    <MenuItem key={role.role} value={role.role}>
                      <Checkbox checked={user.roles.includes(role.role)} />
                      {role.role}
                    </MenuItem>
                  ))}
              </Select>
            </FormControl>
          </>
        ) : (
          <div>Loading...</div>
        )}
      </DialogContent>
      <DialogActions>
        <Box sx={{ marginRight: '1rem', marginBottom: '.5rem' }}>
          <Button onClick={onClose}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit}>
            Update User
          </Button>
        </Box>
      </DialogActions>
    </Dialog>
  )
}

export default UserModal
