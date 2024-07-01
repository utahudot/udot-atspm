import { ResponsivePageLayout } from '@/components/ResponsivePage'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Link,
  List,
  ListItemText,
  Typography,
} from '@mui/material'

const GITHUB_BASE_URL = 'https://github.com/udotdevelopment/ATSPM/issues/'

const Changelog = () => {
  const logs = [
    {
      id: 1,
      date: '02-01-23',
      version: '4.3.1',
      changes: [
        {
          content:
            'The ConvertDBForHistoricalConfigurations executable has been added to the executables folder.',
          issues: ['212', '211'],
        },
        {
          content:
            'A bug that can prevent the chart usage report from working has been fixed.',
          issues: ['213'],
        },
        {
          content:
            'The Split Fail measure now shows the full time span selected.',
          issues: [],
        },
      ],
    },
    {
      id: 2,
      date: '05-21-22',
      version: '4.3',
      changes: [
        {
          content:
            'The Split Fail measure now shows the full time span selected.',
          issues: ['212', '211'],
        },
        {
          content:
            'The ConvertDBForHistoricalConfigurations executable has been added to the executables folder.',
          issues: ['212', '211'],
        },
      ],
    },
    {
      id: 3,
      date: '11-11-22',
      version: '4.2',
      changes: [
        {
          content:
            'The Split Fail measure now shows the full time span selected.',
          issues: ['212', '211'],
        },
        {
          content:
            'The ConvertDBForHistoricalConfigurations executable has been added to the executables folder.',
          issues: ['212', '211'],
        },
      ],
    },
  ]
  return (
    <ResponsivePageLayout title="Changelog">
      <Typography variant="h5" component="h2" sx={{ margin: '10px 0px', fontWeight: 'bold' }}>
        Current Version:
      </Typography>
      {logs.map((log, index) => (
        <>
          <Accordion key={log.id} defaultExpanded={index === 0}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography
                variant="h5"
                component="h3"
                // color={colors.blueAccent[400]}
                sx={{ width: '33%', flexShrink: 0 }}
              >
                Version {log.version}
              </Typography>
              <Typography
                variant="h5"
                component="h3"
                // color={colors.blueAccent[400]}
              >
                {log.date}
              </Typography>
            </AccordionSummary>
            <AccordionDetails>
              <List>
                {log.changes.map((change) => (
                  <ListItemText key={change.content}>
                    • {change.content}{' '}
                    {change.issues &&
                      change.issues.map((issue) => (
                        <Link
                          href={GITHUB_BASE_URL + issue}
                          // color={colors.blueAccent[500]}
                          key={issue}
                        >
                          (#{issue}){' '}
                        </Link>
                      ))}
                  </ListItemText>
                ))}
              </List>
            </AccordionDetails>
          </Accordion>
          {index === 0 && (
            <Typography
              variant="h5"
              component="h2"
              sx={{ margin: '40px 0px 10px', fontWeight: 'bold' }}
            >
              Previous Versions:
            </Typography>
          )}
        </>
      ))}
    </ResponsivePageLayout>
  )
}

export default Changelog
