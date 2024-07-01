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
import { VerifyUserHandler } from '../handlers/verifyUserHandler'

interface props {
  handler: VerifyUserHandler
}

export const VerifyUserOptionsComponent = ({ handler }: props) => {
  const [showPassword, setShowPassword] = useState(false)
  return (
    <Box
      component="form"
      noValidate
      onSubmit={(event) => handler.handleSubmit(event)}
      sx={{ mt: 3 }}
    >
      <Grid container spacing={2}>
        <Grid item xs={12}>
          <TextField
            required
            fullWidth
            type={showPassword ? 'text' : 'password'}
            name="password"
            label="Password"
            id="password"
            autoComplete="new-password"
            onChange={(e) => handler.savePassword(e.target.value)}
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
        <Grid item xs={12} sx={{ textAlign: 'center' }}>
          <Button
            type="submit"
            fullWidth
            variant="contained"
            sx={{ mt: 3, mb: 2, width: '50%', alignContent: 'center' }}
          >
            Change Password
          </Button>
        </Grid>
      </Grid>
    </Box>
  )
}
