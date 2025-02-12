import DownloadIcon from '@mui/icons-material/Download'
import PrintIcon from '@mui/icons-material/Print'
import SaveIcon from '@mui/icons-material/Save'
import { Box, Button, Divider, Paper, Typography } from '@mui/material'
import html2canvas from 'html2canvas'
import { jsPDF } from 'jspdf'
import { useRef } from 'react'
import { useReactToPrint } from 'react-to-print'

const TspReport = () => {
  const contentRef = useRef<HTMLDivElement>(null)

  const handlePrint = useReactToPrint({ contentRef })

  // This function captures the ref’s content as an image, then generates a PDF.
  const handleDownloadPdf = async () => {
    if (!contentRef.current) return
    try {
      const canvas = await html2canvas(contentRef.current)
      const imgData = canvas.toDataURL('image/png')
      const pdf = new jsPDF({
        orientation: 'p',
        unit: 'px',
        format: 'a4',
      })
      const pdfWidth = pdf.internal.pageSize.getWidth()
      const pdfHeight = pdf.internal.pageSize.getHeight()

      // Add the image to the PDF, covering the full page.
      pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, pdfHeight)
      pdf.save('TSP_Report.pdf')
    } catch (error) {
      console.error('PDF generation error:', error)
    }
  }

  return (
    <Box sx={{ display: 'flex', justifyContent: 'center' }}>
      <Box>
        <Box
          sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2, gap: 2 }}
        >
          <Button
            size="small"
            variant="outlined"
            startIcon={<PrintIcon />}
            color="primary"
            onClick={handlePrint}
          >
            Print
          </Button>

          <Button
            size="small"
            variant="outlined"
            startIcon={<DownloadIcon />}
            color="primary"
            onClick={handleDownloadPdf}
          >
            Download
          </Button>

          <Button
            size="small"
            variant="outlined"
            startIcon={<SaveIcon />}
            color="primary"
            onClick={handlePrint}
          >
            Save Parameters
          </Button>
        </Box>

        <Paper
          ref={contentRef}
          sx={{
            position: 'relative',
            height: '1100px',
            width: '850px',
            p: 2,
            backgroundColor: 'white',
          }}
        >
          <Box>
            <Typography variant="h3" sx={{ textAlign: 'center', mt: 3 }}>
              TSP Report
            </Typography>
            <Box
              sx={{
                m: 2,
                p: 2,
                pt: 1.5,
                display: 'flex',
                flexDirection: 'column',
                gap: 2,
                backgroundColor: 'background.default',
                borderRadius: 5,
              }}
            >
              <Typography variant="h6">Report Parameters</Typography>
              <Divider />
              <Box sx={{ display: 'flex', flexDirection: 'column' }}>
                <Typography variant="body1">Date Range:</Typography>
                <Typography variant="body1" fontWeight="bold">
                  8:00 AM - 9:00 AM
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', flexDirection: 'column' }}>
                <Typography variant="body1">Days of the Week:</Typography>
                <Typography variant="body1" fontWeight="bold">
                  Mon, Tues, Wed, Thurs, Fri
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', flexDirection: 'column' }}>
                <Typography variant="body1">Selected Locations:</Typography>
                <Typography variant="body1" fontWeight="bold">
                  #7115 - 3000 W & 500 N
                </Typography>
                <Typography variant="body1" fontWeight="bold">
                  #7116 - 3000 W & 5900 N
                </Typography>
                <Typography variant="body1" fontWeight="bold">
                  #7117 - Washington & 5700 N
                </Typography>
              </Box>
            </Box>
          </Box>
        </Paper>
      </Box>
    </Box>
  )
}

export default TspReport
