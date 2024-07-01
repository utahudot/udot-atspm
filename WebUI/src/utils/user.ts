import Cookies from 'js-cookie'

export const isUserLoggedIn = () => {
  const loggedIn = Cookies.get('loggedIn')

  return loggedIn !== null && loggedIn !== undefined
}
