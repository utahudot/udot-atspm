import { Box, Divider, Link, Stack, Typography } from '@mui/material'

const PedestrianActivityInfoPanel = () => {
  return (
    <Stack spacing={2} sx={{ p: 2 }}>
      <Box>
        <Typography variant="body2" color="text.secondary">
          Background and methodology references for this report.
        </Typography>
      </Box>

      <Divider />

      <Box>
        <Typography variant="subtitle2">References</Typography>

        <Typography variant="body2" sx={{ mt: 1 }}>
          Singleton et al. (2020). Report.
          <br />
          <Link
            href="https://rosap.ntl.bts.gov/view/dot/54924"
            target="_blank"
            rel="noopener noreferrer"
            underline="hover"
          >
            https://rosap.ntl.bts.gov/view/dot/54924
          </Link>
        </Typography>

        <Typography variant="body2" sx={{ mt: 1.5 }}>
          Singleton &amp; Runa (2021). Paper.
          <br />
          <Link
            href="https://doi.org/10.1177/0361198121994126"
            target="_blank"
            rel="noopener noreferrer"
            underline="hover"
          >
            https://doi.org/10.1177/0361198121994126
          </Link>
        </Typography>
      </Box>
    </Stack>
  )
}

export default PedestrianActivityInfoPanel
