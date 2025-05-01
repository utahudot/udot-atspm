import { setSecureCookie } from '@/features/identity/utils'
import Cookies from 'js-cookie'
import { useRouter } from 'next/router'
import { FormEvent, useEffect, useState } from 'react'
import { useChangePassword } from '../../api/changePassword'
import { VerifyToken, useVerifyResetToken } from '../../api/verifyResetToken'
import { ResponseDto } from '../../types/responseDto'
import { PasswordHandler, ResponseHandler } from './baseHandler'

export interface ChangePasswordHandler
  extends PasswordHandler,
    ResponseHandler {
  data: ResponseDto
  submitted: boolean
  confirmPassword: string
  validateConfirmPassword(): string | null
  saveConfirmPassword(pass: string): void
  handleSubmit(event: FormEvent<HTMLFormElement>): void
}

export interface VerifyTokenHandler {
  data: ResponseDto
  isLoadingValidity: boolean
  isValidToken: boolean
  resetToken: string
}

interface changePasswordProp {
  resetToken: string
}

export const useChangePasswordHandler = ({
  resetToken,
}: changePasswordProp): ChangePasswordHandler => {
  const [submitted, setSubmitted] = useState(false)
  const [password, setPassword] = useState<string>('')
  // const [oldPassword, setOldPassword] = useState<string>('')
  const [confirmPassword, setConfirmPassword] = useState<string>('')
  const [responseSuccess, setResponseSuccess] = useState(false)
  const [responseError, setResponseError] = useState(false)
  const [data, setData] = useState<ResponseDto>()

  const {
    refetch,
    data: changePasswordData,
    status,
  } = useChangePassword({
    resetToken,
    newPassword: password,
    confirmPassword,
  })

  useEffect(() => {
    if (status !== 'loading' && status === 'success') {
      setResponseSuccess(true)
    }

    if (status === 'error') {
      setResponseError(true)
    }
  }, [status])

  useEffect(() => {
    if (changePasswordData) {
      setData(changePasswordData)
    }
  }, [changePasswordData])

  const passwordCheck = () => {
    if (password.length < 8) {
      return 'Password should be at least 8 characters long.'
    }

    const hasUpperCase = /[A-Z]/
    const hasDigit = /\d/
    const hasSpecialChar = /[!@#$%^&*()_+\[\]{};:'"\\|,.<>?]/

    if (
      !hasUpperCase.test(password) ||
      !hasDigit.test(password) ||
      !hasSpecialChar.test(password)
    ) {
      return 'Password should contain at least one uppercase letter, one digit, and one symbol.'
    }

    return null
  }

  const confirmPasswordCheck = () => {
    if (password !== confirmPassword) {
      return 'Passwords do not match.'
    }

    return null
  }

  const handleSubmitForm = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const passwordError = passwordCheck()
    const confirmPasswordError = confirmPasswordCheck()

    if (passwordError || confirmPasswordError) {
      return
    }
    setSubmitted(true)
    refetch()
  }

  const component: ChangePasswordHandler = {
    data: data as ResponseDto,
    password,
    confirmPassword,
    responseError,
    responseSuccess,
    submitted,
    handleResponseError: (val: boolean) => {
      setResponseError(val)
    },
    handleResponseSuccess: (val: boolean) => {
      setResponseSuccess(val)
    },
    validatePassword: () => {
      return passwordCheck()
    },
    validateConfirmPassword: () => {
      return confirmPasswordCheck()
    },
    handleSubmit: (event: FormEvent<HTMLFormElement>) => {
      handleSubmitForm(event)
    },
    savePassword: (pass: string) => {
      setPassword(pass)
    },
    saveConfirmPassword: (pass: string) => {
      setConfirmPassword(pass)
    },
  }

  return component
}

export const useVerifyTokenHandler = (): VerifyTokenHandler => {
  const router = useRouter()
  const [isLoadingValidity, setIsLoadingValidity] = useState(true)
  const [username, setUsername] = useState('')
  const [resetToken, setResetToken] = useState('')
  const [isValidToken, setIsValidToken] = useState(false)
  const [data, setData] = useState<VerifyToken>()

  const {
    data: verifyResetTokenData,
    refetch,
    status,
  } = useVerifyResetToken({
    token: resetToken,
    username,
  })

  useEffect(() => {
    if (verifyResetTokenData) {
      setData(verifyResetTokenData)
      setIsValidToken(true)
      setSecureCookie('token', verifyResetTokenData.token)
    }
  }, [verifyResetTokenData])

  useEffect(() => {
    const queryParams = new URLSearchParams(router.asPath.split('?')[1])
    const name = queryParams.get('username')
    const code = queryParams.get('token')
    if (name && code) {
      setUsername(name)
      setResetToken(code)
    }
  }, [router.asPath])

  useEffect(() => {
    const code = Cookies.get('resetToken')
    const name = Cookies.get('username')
    if (name && code) {
      setUsername(name)
      setResetToken(code)
    }
  }, [])

  useEffect(() => {
    if (resetToken && username && isLoadingValidity) {
      refetch()
    }
  }, [isLoadingValidity, refetch, resetToken, username])

  useEffect(() => {
    if (status === 'success') {
      setIsLoadingValidity(false)
    }
    if (status === 'error') {
      window.location.href = '/unauthorized'
    }
  }, [status])

  const component: VerifyTokenHandler = {
    data: data as any,
    isLoadingValidity,
    isValidToken,
    resetToken,
  }

  return component
}
