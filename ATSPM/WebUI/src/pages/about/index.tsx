import Header from '@/components/header'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import KeyboardDoubleArrowRightIcon from '@mui/icons-material/KeyboardDoubleArrowRight'
import { Box, Button, Grid, Typography } from '@mui/material'
import Image from 'next/image'
import Link from 'next/link'
const About = () => {
  
  const academimcPartners = [
    {
      path: '/images/partners/purdue-university.png',
      title: 'Purdue University logo',
    },
    {
      path: '/images/partners/byu.svg',
      title: 'BYU logo',
    },
    {
      path: '/images/partners/university-of-utah.png',
      title: 'University of Utah logo',
    },
    {
      path: '/images/partners/university-of-texas-at-arlington.png',
      title: 'University of Texas at Arlington',
    },
    {
      path: '/images/partners/iowa-state.png',
      title: 'Iowa State',
    },
    {
      path: '/images/partners/university-of-alabama.png',
      title: 'University of Alabama',
    },
    {
      path: '/images/partners/utah-state-university.png',
      title: 'USU logo',
    },
  ]

  const agencyPartners = [
    {
      path: '/images/partners/gdot.png',
      title: 'GDOT logo',
    },
    {
      path: '/images/partners/vdot.png',
      title: 'VDOT logo',
    },
    {
      path: '/images/partners/indot.png',
      title: 'INDOT logo',
    },
    {
      path: '/images/partners/fdot.png',
      title: 'FDOT logo',
    },
    {
      path: '/images/partners/pennsylvania-dot.png',
      title: 'Pennyslvania DOT logo',
    },
    {
      path: '/images/partners/minnesota-dot.png',
      title: 'Minnesota DOT logo',
    },
    {
      path: '/images/partners/north-carolina-dot.png',
      title: 'North Carolina DOT logo',
    },
    {
      path: '/images/partners/pennsylvania-dot.png',
      title: 'Pennsylvania DOT logo',
    },
  ]

  const udotLogo = [
    {
      path: '/images/udot.png',
      title: 'UDOT logo',
    }
  ]

  const consultantPartners = [
    {
      path: '/images/partners/econolite.png',
      title: 'Econolite logo',
    },
    {
      path: '/images/partners/avenue-consultants.png',
      title: 'Avenue Consultants logo',
    },
    {
      path: '/images/partners/kittleson-and-associates.png',
      title: 'Kittleson and Associates logo',
    },
    {
      path: '/images/partners/atkins.png',
      title: 'Atkins logo',
    },
    {
      path: '/images/partners/kimley-horn.svg',
      title: 'Kimley Horn logo',
    },
    {
      path: '/images/partners/pinetop-engineering.jpg',
      title: 'Pintetop Engineering logo',
    },
    {
      path: '/images/partners/qfree.png',
      title: 'Q Free logo',
    },
    {
      path: '/images/partners/miovision.png',
      title: 'Miovision logo',
    },
    {
      path: '/images/partners/lee-engineering.png',
      title: 'Lee Engineering logo',
    },
    { 
      path: '/images/partners/traffop.png',
      title: 'Traffop logo',
    },
    {
      path: '/images/partners/portland-oregon.svg',
      title: 'Portland Oregon logo',
    },
    {
      path: '/images/partners/maricopa-county.png',
      title: 'Maricopa County logo',
    }
  ]

  return (
    <ResponsivePageLayout title={'About'} hideTitle>
      <Box>
        <Header title="About ATSPM" />
        <Box marginBottom={2}>
          <Typography variant="h4" component={'p'}>
            Automated Traffic Signal Performance Measures show real-time
            and historical functionality at locationized intersections. This
            allows traffic engineers to directly measure what previously could
            only be estimated and modeled.
          </Typography>
        </Box>
        <Box marginBottom={10}>
          <Button
            variant="contained"
            href="/faq"
            component="a"
            LinkComponent={Link}
            endIcon={<KeyboardDoubleArrowRightIcon />}
          >
            Learn more
          </Button>
        </Box>
        <Box marginBottom={2}>
          <Typography variant="h5" component={'p'}>
            The following are those who have contributed changes to the ATSPM
            system:
          </Typography>
        </Box>
        <Grid container spacing={2}>
          {udotLogo.map((item) => (
            <Grid item key={item.path} xs={4} sm={4} md={3} lg={2} xl={2}>
              <Box
                sx={{
                  display: 'flex',
                  justifyContent: 'center',
                  alignItems: 'center',
                  backgroundColor: '#f5f5f5',
                  padding: '8px',
                  height: '10vw',
                  boxShadow: 1,
                }}
                position="relative"
                padding="20px"
              >
                <Image
                  src={item.path}
                  alt={item.title}
                  style={{ objectFit: 'contain', padding: '15px' }}
                  sizes="(max-width: 1200px) 15vw"
                  fill
                />
              </Box>
            </Grid>
          ))}
        </Grid>
        <Grid container spacing={2}>
          {consultantPartners.map((item) => (
            <Grid item key={item.path} xs={4} sm={4} md={3} lg={2} xl={2}>
              <Box
                sx={{
                  display: 'flex',
                  justifyContent: 'center',
                  alignItems: 'center',
                  backgroundColor: '#f5f5f5',
                  padding: '8px',
                  height: '10vw',
                  boxShadow: 1,
                }}
                position="relative"
                padding="20px"
              >
                <Image
                  src={item.path}
                  alt={item.title}
                  style={{ objectFit: 'contain', padding: '15px' }}
                  sizes="(max-width: 1200px) 15vw"
                  fill
                />
              </Box>
            </Grid>
          ))}
        </Grid>
        <Grid container spacing={2}>
          {agencyPartners.map((item) => (
            <Grid item key={item.path} xs={4} sm={4} md={3} lg={2} xl={2}>
              <Box
                sx={{
                  display: 'flex',
                  justifyContent: 'center',
                  alignItems: 'center',
                  backgroundColor: '#f5f5f5',
                  padding: '8px',
                  height: '10vw',
                  boxShadow: 1,
                }}
                position="relative"
                padding="20px"
              >
                <Image
                  src={item.path}
                  alt={item.title}
                  style={{ objectFit: 'contain', padding: '15px' }}
                  sizes="(max-width: 1200px) 15vw"
                  fill
                />
              </Box>
            </Grid>
          ))}
        </Grid>
        <Grid container spacing={2}>
          {academimcPartners.map((item) => (
            <Grid item key={item.path} xs={4} sm={4} md={3} lg={2} xl={2}>
              <Box
                sx={{
                  display: 'flex',
                  justifyContent: 'center',
                  alignItems: 'center',
                  backgroundColor: '#f5f5f5',
                  padding: '8px',
                  height: '10vw',
                  boxShadow: 1,
                }}
                position="relative"
                padding="20px"
              >
                <Image
                  src={item.path}
                  alt={item.title}
                  style={{ objectFit: 'contain', padding: '15px' }}
                  sizes="(max-width: 1200px) 15vw"
                  fill
                />
              </Box>
            </Grid>
          ))}
        </Grid>
      </Box>
    </ResponsivePageLayout>
  )
}

export default About