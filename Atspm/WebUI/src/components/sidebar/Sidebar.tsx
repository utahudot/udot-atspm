import NavItemWithSubMenu from '@/components/sidebar/NavItemWithSubMenu'
import { topbarHeight } from '@/components/topbar'
import { useSideBarPermission } from '@/features/identity/pagesCheck'
import { useSidebarStore } from '@/stores/sidebar'
import AddchartOutlinedIcon from '@mui/icons-material/AddchartOutlined'
import DescriptionOutlinedIcon from '@mui/icons-material/DescriptionOutlined'
import FileDownloadIcon from '@mui/icons-material/FileDownload'
import RouteOutlinedIcon from '@mui/icons-material/RouteOutlined'
import ShowChartOutlinedIcon from '@mui/icons-material/ShowChartOutlined'
import SignalCellularAltOutlinedIcon from '@mui/icons-material/SignalCellularAltOutlined'
import { Box, Drawer, List, useTheme } from '@mui/material'
import Image from 'next/image'
import React from 'react'
import NavItem from './NavItem'
import Sponsor from './Sponsor'
import SubMenu from './SubMenu'

export const sidebarWidth = 300

export default function Sidebar() {
  const theme = useTheme()
  const { isSidebarOpen, toggleSidebar } = useSidebarStore()
  const hasDataViewPermission = useSideBarPermission('data:view')
  const hasWatchDogPermission = useSideBarPermission('watchdog:view')
  const hasLTGRPermission = useSideBarPermission('Report:view')

  const reportsList = [
    {
      text: 'Transit Signal Priority',
      url: '/reports/transit-signal-priority',
    },
  ]
  if (hasLTGRPermission) {
    reportsList.push({ text: 'Left Turn Gap', url: '/reports/left-turn-gap' })
  }

  const toggleDrawer =
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
        width: isSidebarOpen ? sidebarWidth : '0px',
        transition: 'width 0.2s ease-out',
        overflow: 'hidden',
      }}
    >
      <Drawer
        variant="permanent"
        open={isSidebarOpen}
        onClose={toggleDrawer()}
        PaperProps={{
          sx: {
            height: `calc(100% - ${topbarHeight}px)`,
            top: topbarHeight,
            transform: isSidebarOpen
              ? 'translateX(0)'
              : `translateX(-${sidebarWidth}px)`,
            transition: 'transform 0.2s ease-out',
            backgroundColor: theme.palette.background.paper,
            border: 'none',
            boxShadow: '2',
            width: sidebarWidth,
          },
        }}
      >
        <List>
          <SubMenu subheader={'Operations'}>
            <NavItem
              icon={<SignalCellularAltOutlinedIcon />}
              text={'Performance Measures'}
              url={'/performance-measures'}
            />
            <NavItem
              icon={<ShowChartOutlinedIcon />}
              text={'Time-Space Diagrams'}
              url={'/time-space-diagrams'}
            />
            <NavItem
              icon={<RouteOutlinedIcon />}
              text={'Link Pivot'}
              url={'/link-pivot'}
            />
            {reportsList.length > 0 && (
              <NavItemWithSubMenu
                icon={<DescriptionOutlinedIcon />}
                text="Reports"
                subItems={reportsList}
              />
            )}
            <NavItem
              icon={<AddchartOutlinedIcon />}
              text={'Aggregate Charts'}
              url={'/aggregate-charts'}
            />
            {hasWatchDogPermission && (
              <NavItem
                icon={
                  <Image
                    alt="WatchDog Logo"
                    src="/images/dog.png"
                    width={0}
                    height={0}
                    style={{
                      width: '21px',
                      height: '21px',
                    }}
                  />
                }
                text={'Watchdog'}
                url={'/watchdog'}
              />
            )}
          </SubMenu>
          {hasDataViewPermission && (
            <SubMenu subheader={'Data'}>
              <NavItem
                icon={<FileDownloadIcon />}
                text={'Export'}
                url={'/export'}
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
      {isSidebarOpen && (
        <Box
          sx={{
            position: 'fixed',
            top: topbarHeight,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0, 0, 0, 0.3)',
            zIndex: theme.zIndex.drawer - 1,
          }}
          onClick={toggleDrawer()}
        />
      )}
    </Box>
  )
}
