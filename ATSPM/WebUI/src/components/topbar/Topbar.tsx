import { useGetAdminPagesList } from '@/features/identity/pagesCheck'
import { doesUserHaveAccess } from '@/features/identity/utils'
import { useSidebarStore } from '@/stores/sidebar'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import MenuIcon from '@mui/icons-material/Menu'
import PublishedWithChangesOutlinedIcon from '@mui/icons-material/PublishedWithChangesOutlined'
import QuestionAnswerOutlinedIcon from '@mui/icons-material/QuestionAnswerOutlined'
import { Box, IconButton, Paper } from '@mui/material'
import Image from 'next/image'
import NextLink from 'next/link'
import { useEffect, useState } from 'react'
import DropDownButton from './DropdownButton'
import UserMenu from './UserMenu'

export const topbarHeight = 60

export default function Topbar() {
  const { toggleSidebar } = useSidebarStore()
  const [userHasAccess, setUserHasAccess] = useState(false)

  useEffect(() => {
    setUserHasAccess(doesUserHaveAccess())
  }, [])

  const handleMenuCollapseClick = () => {
    toggleSidebar()
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
              style={{ cursor: 'pointer', objectFit: 'contain' }}
            />
          </Box>
        </NextLink>
      </Box>
      <Box>
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
    </Box>
  )
}
