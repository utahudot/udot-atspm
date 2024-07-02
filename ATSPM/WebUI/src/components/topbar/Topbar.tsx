import { transformMenuItems } from '@/components/topbar/menuUtils'
import { useGetAdminPagesList } from '@/features/identity/pagesCheck'
import { useGetMenuItems } from '@/features/links/api/getMenuItems'
import { useSidebarStore } from '@/stores/sidebar'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import PublishedWithChangesOutlinedIcon from '@mui/icons-material/PublishedWithChangesOutlined'
import QuestionAnswerOutlinedIcon from '@mui/icons-material/QuestionAnswerOutlined'
import { Box, Button, Typography, useTheme } from '@mui/material'
import Cookies from 'js-cookie'
import { useRouter } from 'next/router'
import { useEffect, useState } from 'react'
import DropDownButton from './DropdownButton'
import UserMenu from './UserMenu'

const doesUserHaveAccess = () => {
  if (typeof window === 'undefined') {
    return false
  }

  const loggedIn = Cookies.get('loggedIn')
  if (!loggedIn) {
    return false
  }

  const claims = Cookies.get('claims')
  return !!claims
}

export default function Topbar() {
  const theme = useTheme()
  const { isSidebarOpen, toggleSidebar } = useSidebarStore()
  const [userHasAccess, setUserHasAccess] = useState(false)
  const { data: menuItemsData, isLoading } = useGetMenuItems()
  const router = useRouter()

  useEffect(() => {
    setUserHasAccess(doesUserHaveAccess())
  }, [])
  const adminPagesList = useGetAdminPagesList()

  if (isLoading) {
    return null
  }

  const menuItems = menuItemsData ? transformMenuItems(menuItemsData.value) : []

  const handleMenuCollapseClick = () => {
    toggleSidebar()
  }

  const handleNavigation = (path: string) => {
    router.push(path)
  }

  const infoItems = [
    {
      name: 'About',
      icon: <InfoOutlinedIcon fontSize="small" />,
      link: '/about',
    },
    {
      name: 'FAQ',
      icon: <QuestionAnswerOutlinedIcon fontSize="small" />,
      link: '/faq',
    },
    {
      name: 'Changelog',
      icon: <PublishedWithChangesOutlinedIcon fontSize="small" />,
      link: '/changelog',
    },
  ]

  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'space-between',
        p: 1,
        borderBottom: '1px solid rgba(0, 0, 0, 0.12)',
      }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          borderRadius: '3px',
          backgroundColor: theme.palette.background.paper,
          marginY: '7px',
        }}
      ></Box>

      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          color: theme.palette.mode === 'light' ? 'white' : 'black',
        }}
      >
        <DropDownButton
          title="Info"
          icon={<InfoOutlinedIcon />}
          menuItems={infoItems}
        />
        {menuItems.map((item) =>
          item.parentId === null && item.link ? (
            <Button
              key={item.name}
              onClick={() => handleNavigation(item.link)}
              sx={{
                mx: '2px',
                color: theme.palette.text.primary,
                textTransform: 'none',
              }}
            >
              <Typography fontWeight={400} sx={{ textTransform: 'none' }}>
                {item.name}
              </Typography>
            </Button>
          ) : (
            <DropDownButton
              key={item.name}
              title={item.name}
              icon={item.icon || <InfoOutlinedIcon />}
              menuItems={
                item.children.length > 0
                  ? item.children
                  : [{ name: item.name, icon: item.icon, link: item.link }]
              }
            />
          )
        )}
        {userHasAccess && (
          <DropDownButton
            title="Admin"
            icon={<InfoOutlinedIcon />}
            menuItems={Array.from(adminPagesList.keys()).map((key) => ({
              name: key,
              icon: null,
              link: adminPagesList.get(key) as string,
            }))}
          />
        )}
        <UserMenu />
      </Box>
    </Box>
  )
}
