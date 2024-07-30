import router from 'next/router'

export const navigateToPage = (url: string) => {
  if(url == undefined) return null
  router.push(url)
}
