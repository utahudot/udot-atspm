import { Visibility, VisibilityOff } from '@mui/icons-material'
import {
  Box,
  Button,
  Grid,
  IconButton,
  InputAdornment,
  TextField,
} from '@mui/material'
import Link from 'next/link'
import { useState } from 'react'
import { RegistrationHandler } from '../handlers/RegistrationHandler'

interface props {
  handler: RegistrationHandler
}

export const RegistrationOptionsComponent = ({ handler }: props) => {
  const [showPassword, setShowPassword] = useState(false)

  return (
    <Box
      component="form"
      noValidate
      onSubmit={(event) => handler.handleSubmit(event)}
      sx={{ mt: 3 }}
    >
      <Grid container spacing={2}>
        <Grid item xs={12} sm={6}>
          <TextField
            autoComplete="given-name"
            name="firstName"
            required
            fullWidth
            id="firstName"
            label="First Name"
            autoFocus
            onChange={(e) => handler.saveFirstName(e.target.value)}
            error={handler.submitted && !!handler.validateFirstName()}
            helperText={handler.submitted && handler.validateFirstName()}
          />
        </Grid>
        <Grid item xs={12} sm={6}>
          <TextField
            required
            fullWidth
            id="lastName"
            label="Last Name"
            name="lastName"
            autoComplete="family-name"
            onChange={(e) => handler.saveLastName(e.target.value)}
            error={handler.submitted && !!handler.validateLastName()}
            helperText={handler.submitted && handler.validateLastName()}
          />
        </Grid>
        <Grid item xs={12}>
          <TextField
            required
            fullWidth
            id="email"
            label="Email Address"
            name="email"
            type="email"
            autoComplete="email"
            onChange={(e) => handler.saveEmail(e.target.value)}
            error={handler.submitted && !!handler.validateEmail()}
            helperText={handler.submitted && handler.validateEmail()}
          />
        </Grid>
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
            name="agency"
            label="agency"
            id="agency"
            autoComplete="agency"
            onChange={(e) => handler.saveAgency(e.target.value)}
            error={handler.submitted && !!handler.validateAgency()}
            helperText={handler.submitted && handler.validateAgency()}
          />
        </Grid>
      </Grid>
      <Button type="submit" fullWidth variant="contained" sx={{ mt: 3, mb: 2 }}>
        Sign Up
      </Button>
      <Grid container justifyContent="flex-end">
        <Grid item>
          <Link style={{ color: 'cornflowerblue' }} href="/login">
            Already have an account? Sign in
          </Link>
        </Grid>
      </Grid>
    </Box>
  )
}
