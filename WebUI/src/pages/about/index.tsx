import Header from '@/components/header'
import KeyboardDoubleArrowRightIcon from '@mui/icons-material/KeyboardDoubleArrowRight'
import { Box, Button, Grid, Typography } from '@mui/material'
import Head from 'next/head'
import Image from 'next/image'
import Link from 'next/link'
const About = () => {
  const partners = [
    {
      path: '/images/udot.png',
      title: 'UDOT logo',
    },
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
  ]

  return (
    <>
      <Head>
        <title>About</title>
      </Head>

      <Box>
        <Header title="About ATSPM" />
        <Box marginBottom={2}>
          <Typography variant="h4" component={'p'}>
            Automated Traffic Location Performance Measures&apos;s show
            real-time and historical functionality at locationized
            intersections. This allows traffic engineers to directly measure
            what previously could only be estimated and modeled.
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
          {partners.map((item) => (
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
    </>
  )
}

export default About
