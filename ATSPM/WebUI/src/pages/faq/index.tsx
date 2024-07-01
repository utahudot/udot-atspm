import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useGetFaqs } from '@/features/faq/api'
import { Faq } from '@/features/faq/types'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Divider,
  Typography,
} from '@mui/material'
import { Markup } from 'interweave'

const FAQ = () => {
  const faqQuery = useGetFaqs()

  if (faqQuery.isLoading) return 'Loading...'

  if (faqQuery.error instanceof Error) {
    return 'An error has occurred: ' + faqQuery.error.message
  }

  if (!faqQuery.data) {
    return null
  }

  return (
    <ResponsivePageLayout title="Frequently Asked Questions">
      {faqQuery.data.value.map((faq: Faq) => (
        <Accordion key={faq.id}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="body1" fontWeight="bold">
              {faq.header}
            </Typography>
          </AccordionSummary>
          <Divider />
          <AccordionDetails>
            <Typography>
              <Markup content={faq.body} />
            </Typography>
          </AccordionDetails>
        </Accordion>
      ))}
    </ResponsivePageLayout>
  )
}

export default FAQ
