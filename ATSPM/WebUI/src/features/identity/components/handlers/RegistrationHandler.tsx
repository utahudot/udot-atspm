import Cookies from 'js-cookie'
import { FormEvent, useEffect, useState } from 'react'
import { useCreateUser } from '../../api/createUser'
import IdentityDto from '../../types/identityDto'
import { EmailAndPasswordHandler, ResponseHandler } from './baseHandler'

export interface RegistrationHandler
  extends EmailAndPasswordHandler,
    ResponseHandler {
  firstName: string
  lastName: string
  agency: string
  data: IdentityDto
  submitted: boolean
  handleSubmit(event: FormEvent<HTMLFormElement>): void
  saveFirstName(name: string): void
  saveLastName(name: string): void
  saveAgency(agency: string): void
  validateFirstName(): string | null
  validateLastName(): string | null
  validateAgency(): string | null
}

export const useRegistrationHandler = (): RegistrationHandler => {
  const [submitted, setSubmitted] = useState(false)
  const [email, setEmail] = useState<string>('')
  const [password, setPassword] = useState<string>('')
  const [firstName, setFirstName] = useState<string>('')
  const [lastName, setLastName] = useState<string>('')
  const [agency, setAgency] = useState<string>('')
  const [responseSuccess, setResponseSuccess] = useState(false)
  const [responseError, setResponseError] = useState(false)

  const [data, setData] = useState<IdentityDto>()

  const {
    refetch,
    data: queryData,
    error,
    status,
  } = useCreateUser({
    email,
    password,
    firstName,
    lastName,
    agency,
  })

  useEffect(() => {
    if (status === 'error' && error) {
      setData((error as any).response.data as IdentityDto)
    }
    if (queryData) {
      setData(queryData as IdentityDto)
    }
  }, [error, queryData, status])

  useEffect(() => {
    if (status === 'success' && data !== undefined) {
      Cookies.set('token', data.token)
      Cookies.set('claims', data.claims.join(','))
      Cookies.set('loggedIn', 'True')
      window.location.href = '/locations'
    }
  }, [data, status])

  useEffect(() => {
    if (status === 'success') {
      setResponseSuccess(true)
    }

    if (status === 'error') {
      setResponseError(true)
    }
  }, [status])

  const passwordCheck = () => {
    if (password.length < 8) {
      return 'Password should be at least 8 characters long.'
    }

    if (
      !/(?=.*[A-Z])/.test(password) ||
      !/(?=.*\d)/.test(password) ||
      !/(?=.*[!@#$%^&*()_+[\]{};':"\\|,.<>?])/.test(password)
    ) {
      return 'Password should contain at least one uppercase letter, one digit, and one symbol.'
    }

    return null
  }

  const emailCheck = () => {
    if (!email) {
      return 'Email is required.'
    }

    // Regular expression for validating email addresses
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!regex.test(email)) {
      return 'Please enter a valid email address.'
    }

    return null
  }

  const firstNameCheck = () => {
    if (!firstName) {
      return 'Name is Required'
    }
    return null
  }

  const lastNameCheck = () => {
    if (!firstName) {
      return 'Name is Required'
    }
    return null
  }

  const agencyCheck = () => {
    if (!firstName) {
      return 'Name is Required'
    }
    return null
  }

  const handleSubmitForm = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSubmitted(true)

    const passwordError = passwordCheck()
    if (passwordError) {
      return
    }
    refetch()
  }

  const component: RegistrationHandler = {
    data: data as IdentityDto,
    email,
    password,
    firstName,
    lastName,
    agency,
    responseError,
    responseSuccess,
    submitted,
    handleResponseError: (val: boolean) => {
      setResponseError(val)
    },
    handleResponseSuccess: (val: boolean) => {
      setResponseSuccess(val)
    },
    validateEmail: () => {
      return emailCheck()
    },
    validatePassword: () => {
      return passwordCheck()
    },
    handleSubmit: (event: FormEvent<HTMLFormElement>) => {
      handleSubmitForm(event)
    },
    saveAgency: (agency: string) => {
      setAgency(agency)
    },
    saveEmail: (email: string) => {
      setEmail(email)
    },
    saveFirstName: (name: string) => {
      setFirstName(name)
    },
    saveLastName: (name: string) => {
      setLastName(name)
    },
    savePassword: (pass: string) => {
      setPassword(pass)
    },
    validateAgency: () => {
      return agencyCheck()
    },
    validateFirstName: () => {
      return firstNameCheck()
    },
    validateLastName: () => {
      return lastNameCheck()
    },
  }

  return component
}
