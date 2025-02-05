import { transformMenuItems } from '@/components/topbar/menuUtils'
import { useGetAdminPagesList } from '@/features/identity/pagesCheck'
import { doesUserHaveAccess } from '@/features/identity/utils'
import { useGetMenuItems } from '@/features/links/api/getMenuItems'
import { useSidebarStore } from '@/stores/sidebar'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import MenuIcon from '@mui/icons-material/Menu'
import QuestionAnswerOutlinedIcon from '@mui/icons-material/QuestionAnswerOutlined'
import { Box, Button, IconButton, Paper, Typography } from '@mui/material'
import Image from 'next/image'
import NextLink from 'next/link'
import router from 'next/router'
import { useEffect, useState } from 'react'
import DropDownButton from './DropdownButton'
import UserMenu from './UserMenu'

export const topbarHeight = 60

export default function Topbar() {
  const { toggleSidebar } = useSidebarStore()
  const [userHasAccess, setUserHasAccess] = useState(false)
  const { data: menuItemsData, isLoading } = useGetMenuItems()

  useEffect(() => {
    setUserHasAccess(doesUserHaveAccess())
  }, [])

  const handleMenuCollapseClick = () => {
    toggleSidebar()
  }

  const handleNavigation = (path: string) => {
    router.push(path)
  }

  const menuItems = menuItemsData ? transformMenuItems(menuItemsData.value) : []

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
  ]

  const pagesToLinks = useGetAdminPagesList()

  const adminPagesList = Array.from(pagesToLinks.keys()).map((key) => ({
    name: key,
    link: pagesToLinks.get(key) as string,
  }))

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        paddingX: 2,
        borderBottom: '1px solid rgba(0, 0, 0, 0.12)',
        width: '100%',
        height: topbarHeight,
      }}
    >
      <Paper>
        <IconButton
          aria-label="Open navigation menu"
          onClick={handleMenuCollapseClick}
          sx={{ p: 1 }}
        >
          <MenuIcon />
        </IconButton>
      </Paper>
      <Box
        sx={{
          flexGrow: 1,
          display: 'flex',
          justifyContent: 'flex-start',
        }}
      >
        <NextLink href="/" passHref>
          <Box
            sx={{
              width: '160px',
              height: '50px',
              m: 1,
              ml: 2,
              position: 'relative',
            }}
          >
            <Image
              alt="ATSPM Logo"
              src="/images/atspm-logo-new.png"
              priority
              fill
              sizes="200px"
              style={{ cursor: 'pointer', objectFit: 'contain' }}
            />
          </Box>
        </NextLink>
      </Box>

      {!isLoading && (
        <Box>
          {menuItems.map((item) =>
            item.parentId === null && item.link ? (
              <Button
                key={item.name}
                onClick={() => handleNavigation(item.link)}
                sx={{
                  mx: '2px',
                  textTransform: 'none',
                  color: 'black',
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
          <DropDownButton
            title="Info"
            icon={<InfoOutlinedIcon />}
            menuItems={infoItems}
          />
          {userHasAccess && (
            <DropDownButton
              title="Admin"
              icon={<InfoOutlinedIcon />}
              menuItems={adminPagesList}
            />
          )}
          <UserMenu />
        </Box>
      )}
    </Box>
  )
}
