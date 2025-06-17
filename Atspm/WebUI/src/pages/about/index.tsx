import { ResponsivePageLayout } from '@/components/ResponsivePage'
import KeyboardDoubleArrowRightIcon from '@mui/icons-material/KeyboardDoubleArrowRight'
import OpenInNewIcon from '@mui/icons-material/OpenInNew'
import { Box, Button, Grid, Paper, Typography } from '@mui/material'
import Image from 'next/image'
import Link from 'next/link'

const GridItem = ({ item }: { item: { path: string; title: string } }) => (
  <Grid item key={item.path} xs={4} sm={4} md={3} lg={2} xl={2}>
    <Paper
      sx={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: '200px',
        position: 'relative',
      }}
    >
      <Image
        src={item.path}
        alt={item.title}
        style={{ objectFit: 'contain', padding: '15px' }}
        sizes="(max-width: 1200px) 15vw"
        fill
      />
    </Paper>
  </Grid>
)

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
      path: '/images/partners/iowa-state-university.png',
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
    {
      path: '/images/partners/portland-oregon.svg',
      title: 'Portland Oregon logo',
    },
    {
      path: '/images/partners/vtrans.png',
      title: 'Vermont Trans logo',
    },
    {
      path: '/images/partners/maricopa-county.png',
      title: 'Maricopa County logo',
    },
    {
      path: '/images/partners/gilbert.png',
      title: 'Gilbert logo',
    },
    {
      path: '/images/partners/frisco.png',
      title: 'Frisco logo',
    },
  ]

  const udotLogo = {
    path: '/images/udot.png',
    title: 'UDOT logo',
  }

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
  ]

  return (
    <ResponsivePageLayout title={'About'} hideTitle>
      <Paper
        sx={{
          padding: 4,
          marginBottom: 4,
          maxWidth: '740px',
          marginX: 'auto',
          textAlign: 'center',
          position: 'relative',
        }}
      >
        {/* Version block in corner */}
        <Paper
          variant="outlined"
          sx={{
            position: 'absolute',
            top: 16,
            left: 16,
            display: 'flex',
            alignItems: 'center',
            gap: 1.5,
            fontFamily: 'monospace',
            fontSize: '0.875rem',
            color: 'text.secondary',
            backgroundColor: 'background.default',
            px: 2,
            py: 1,
          }}
        >
          <span>ATSPM version: {process.env.version}</span>
        </Paper>

        {/* Main content */}
        <Typography variant="h2" sx={{ mt: 4 }} gutterBottom>
          About ATSPM
        </Typography>
        <Typography variant="subtitle1" paragraph>
          Automated Traffic Signal Performance Measures show real-time and
          historical functionality at signalized intersections. This allows
          traffic engineers to directly measure what previously could only be
          estimated and modeled.
        </Typography>

        {/* FAQ and Changelog buttons */}
        <Box display="flex" justifyContent="center" gap={2} mt={2}>
          <Button
            variant="contained"
            href="/faq"
            component={Link}
            endIcon={<KeyboardDoubleArrowRightIcon />}
          >
            View FAQs
          </Button>
          <Button
            variant="outlined"
            href="https://github.com/OpenSourceTransportation/Atspm/blob/main/CHANGELOG.md"
            component={Link}
            endIcon={<OpenInNewIcon />}
          >
            View Changelog
          </Button>
        </Box>
      </Paper>

      <Box marginBottom={2} textAlign={'center'}>
        <Typography variant="h3" fontStyle="italic">
          Presented by
        </Typography>
      </Box>
      <Box
        sx={{
          height: '10vw',
          minHeight: '160px',
        }}
        position="relative"
        display="flex"
        justifyContent="center"
        alignItems="center"
        marginBottom={4}
      >
        <Image
          src={udotLogo.path}
          alt={udotLogo.title}
          style={{
            objectFit: 'contain',
            padding: '40px',
            paddingTop: '0px',
          }}
          sizes="(max-width: 1200px) 100vw"
          fill
        />
      </Box>
      <Box marginBottom={4} textAlign="center">
        <Typography variant="h5" component="p">
          The following are those who have contributed to ATSPM
        </Typography>
      </Box>
      <Box marginBottom={2} textAlign={'center'}>
        <Typography variant="h6" fontWeight="bold">
          Agencies
        </Typography>
      </Box>
      <Grid container spacing={3}>
        {agencyPartners.map((item) => (
          <GridItem key={item.title} item={item} />
        ))}
      </Grid>
      <Box marginBottom={2} textAlign={'center'} marginTop={4}>
        <Typography variant="h6" fontWeight="bold">
          Academics
        </Typography>
      </Box>
      <Grid container spacing={3}>
        {academimcPartners.map((item) => (
          <GridItem key={item.title} item={item} />
        ))}
      </Grid>
      <Box marginBottom={2} textAlign={'center'} marginTop={4}>
        <Typography variant="h6" fontWeight="bold">
          Consultants
        </Typography>
      </Box>
      <Grid container spacing={3}>
        {consultantPartners.map((item) => (
          <GridItem key={item.title} item={item} />
        ))}
      </Grid>
    </ResponsivePageLayout>
  )
}

export default About
