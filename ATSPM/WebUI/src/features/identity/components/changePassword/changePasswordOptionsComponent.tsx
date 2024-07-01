import { Visibility, VisibilityOff } from '@mui/icons-material'
import {
  Box,
  Button,
  Grid,
  IconButton,
  InputAdornment,
  TextField,
} from '@mui/material'
import { useState } from 'react'
import { ChangePasswordHandler } from '../handlers/ChangePasswordHandler'

interface props {
  handler: ChangePasswordHandler
}

export const ChangePasswordOptionsComponent = ({ handler }: props) => {
  const [showCurrentPassword, setShowCurrentPassword] = useState(false)
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)

  return (
    <Box component="form" onSubmit={handler.handleSubmit}>
      <Grid container spacing={2}>
        <Grid item xs={12}>
          <TextField
            required
            fullWidth
            id="currentPassword"
            label="Current Password"
            name="currentPassword"
            type={showCurrentPassword ? 'text' : 'password'}
            onChange={(e) => handler.saveOldPassword(e.target.value)}
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    edge="end"
                    onClick={() => setShowCurrentPassword(!showCurrentPassword)}
                  >
                    {showCurrentPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />
        </Grid>
        <Grid item xs={12}>
          <TextField
            required
            fullWidth
            id="password"
            label="New Password"
            name="password"
            type={showPassword ? 'text' : 'password'}
            onChange={(e) => handler.savePassword(e.target.value)}
            error={handler.submitted && !!handler.validatePassword()}
            helperText={handler.submitted && handler.validatePassword()}
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    edge="end"
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {showPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />
        </Grid>
        <Grid item xs={12}>
          <TextField
            required
            fullWidth
            id="confirmPassword"
            label="Confirm Password"
            name="confirmPassword"
            type={showConfirmPassword ? 'text' : 'password'}
            onChange={(e) => handler.saveConfirmPassword(e.target.value)}
            error={handler.submitted && !!handler.validateConfirmPassword()}
            helperText={handler.submitted && handler.validateConfirmPassword()}
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    edge="end"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  >
                    {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />
        </Grid>
      </Grid>
      <Button type="submit" fullWidth variant="contained" sx={{ mt: 3, mb: 2 }}>
        Change Password
      </Button>
    </Box>
  )
}
