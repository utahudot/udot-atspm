import { useSideBarPermission } from '@/features/identity/pagesCheck'
import { useSidebarStore } from '@/stores/sidebar'
import AddchartOutlinedIcon from '@mui/icons-material/AddchartOutlined'
import FileDownloadIcon from '@mui/icons-material/FileDownload'
import ForkLeftOutlinedIcon from '@mui/icons-material/ForkLeftOutlined'
import MenuOpenOutlinedIcon from '@mui/icons-material/MenuOpenOutlined'
import RouteOutlinedIcon from '@mui/icons-material/RouteOutlined'
import ShowChartOutlinedIcon from '@mui/icons-material/ShowChartOutlined'
import TrafficOutlinedIcon from '@mui/icons-material/TrafficOutlined'
import {
  Box,
  Drawer,
  IconButton,
  List,
  Paper,
  useMediaQuery,
  useTheme,
} from '@mui/material'
import Image from 'next/image'
import React, { useEffect } from 'react'
import NavItem from './NavItem'
import Sponsor from './Sponsor'
import SubMenu from './SubMenu'
import dogImg from '/public/images/dog.png'

export default function Sidebar() {
  const theme = useTheme()
  const isMobile = useMediaQuery(theme.breakpoints.down('md'))
  const { isSidebarOpen, toggleSidebar, closeSideBar, openSideBar } =
    useSidebarStore()
  const drawerVariant = isMobile ? 'temporary' : 'permanent'
  const hasDataViewPermission = useSideBarPermission('data:view')
  const hasWatchDogPermission = useSideBarPermission('watchdog:view')
  const hasLTGRPermission = useSideBarPermission('Report:view')

  useEffect(() => {
    // TODO: fix mobile sidebar. set to auto false if page loads in mobile
    let timer: NodeJS.Timeout
    if (isMobile) {
      timer = setTimeout(() => {
        closeSideBar()
      }, 0)
    } else {
      openSideBar()
    }
    return () => clearTimeout(timer)
  }, [isMobile, closeSideBar, openSideBar])

  const toggleMobileDrawer =
    () => (event: React.KeyboardEvent | React.MouseEvent) => {
      if (
        (event.type === 'keydown' &&
          (event as React.KeyboardEvent).key === 'Tab') ||
        (event as React.KeyboardEvent).key === 'Shift'
      ) {
        return
      }
      toggleSidebar()
    }

  return (
    <Box
      sx={{
        height: '100vh',
        width: isMobile ? 'auto' : isSidebarOpen ? '270px' : '10px',
        transition: 'width 0.2s ease-out',
        overflow: 'hidden',
      }}
    >
      <Box
        sx={{
          position: 'fixed',
          top: '23px',
          left: isSidebarOpen ? '250px' : '10px',
          transition: 'left 0.2s ease-out',
          zIndex: 1_300,
        }}
      >
        <Paper>
          <IconButton
            type="button"
            sx={{
              p: 1,
              transform: isSidebarOpen ? 'rotateY(0deg)' : 'rotateY(180deg)',
              transition: 'transform 0.5s',
            }}
            aria-label={isSidebarOpen ? 'Collapse menu' : 'Expand menu'}
            onClick={toggleSidebar}
          >
            <MenuOpenOutlinedIcon fontSize="small" />
          </IconButton>
        </Paper>
      </Box>
      <Drawer
        variant={drawerVariant}
        open={isSidebarOpen}
        onClose={toggleMobileDrawer()}
        anchor="left"
        PaperProps={{
          sx: {
            transform: isSidebarOpen ? 'translateX(0)' : 'translateX(-270px)',
            transition: 'transform 0.2s ease-out',
            backgroundColor: theme.palette.background.paper,
            border: 'none',
            boxShadow: '2',
            width: '270px',
          },
        }}
      >
        <Box
          sx={{
            width: '185px',
            margin: '20px',
            marginBottom: '10px',
            marginX: 'auto',
          }}
        >
          <Image
            alt="ATSPM Logo"
            src="/images/new-atspm-logo.png"
            priority
            width={0}
            height={0}
            sizes="100vw"
            style={{ width: '100%', height: 'auto', cursor: 'pointer' }}
          />
        </Box>
        <List>
          <SubMenu subheader={'Operations'}>
            <NavItem
              icon={<TrafficOutlinedIcon />}
              text={'Locations'}
              url={'/locations'}
            />
            <NavItem
              icon={<ShowChartOutlinedIcon />}
              text={'Time-Space Diagrams'}
              url={'/tools/time-space-diagrams'}
            />
            <NavItem
              icon={<RouteOutlinedIcon />}
              text={'Link Pivot'}
              url={'/tools/link-pivot'}
            />
            {hasLTGRPermission && (
              <NavItem
                icon={<ForkLeftOutlinedIcon />}
                text={'Left Turn Gap Report'}
                url={'/tools/left-turn-gap-report'}
              />
            )}
            <NavItem
              icon={<AddchartOutlinedIcon />}
              text={'Aggregate Charts'}
              url={'/data/aggregate'}
            />
            {hasWatchDogPermission && (
              <NavItem
                icon={
                  <Image
                    style={{
                      width: '21px',
                      height: '21px',
                    }}
                    alt="Guard Dog"
                    src={dogImg}
                  />
                }
                text={'Watchdog'}
                url={'/admin/watchdog'}
              />
            )}
          </SubMenu>
          {hasDataViewPermission && (
            <SubMenu subheader={'Data'}>
              <NavItem
                icon={<FileDownloadIcon />}
                text={'Export'}
                url={'/data/export'}
              />
            </SubMenu>
          )}
        </List>
        <Box
          sx={{
            marginTop: 'auto',
            marginBottom: '40px',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'flex-end',
          }}
        >
          <Sponsor />
        </Box>
      </Drawer>
    </Box>
  )
}
