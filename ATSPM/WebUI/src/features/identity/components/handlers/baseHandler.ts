export interface ResponseHandler {
  responseSuccess: boolean
  responseError: boolean
  handleResponseSuccess(val: boolean): void
  handleResponseError(val: boolean): void
}

export interface EmailAndPasswordHandler extends PasswordHandler {
  email: string
  validateEmail(): string | null
  saveEmail(email: string): void
}

export interface PasswordHandler {
  password: string
  validatePassword(): string | null
  savePassword(pass: string): void
}
