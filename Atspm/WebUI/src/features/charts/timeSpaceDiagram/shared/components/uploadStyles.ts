export const UPLOAD_PANEL_BORDER_COLOR = 'rgba(203, 213, 225, 0.92)'
export const UPLOAD_PANEL_BG = '#ffffff'
export const UPLOAD_PANEL_SUBTLE_BG = '#f8fafc'
export const UPLOAD_PANEL_MUTED_BG = '#f1f5f9'

export const UPLOAD_ACCORDION_SX = {
  border: '1px solid',
  borderColor: UPLOAD_PANEL_BORDER_COLOR,
  borderRadius: '12px !important',
  backgroundColor: UPLOAD_PANEL_BG,
  boxShadow: 'none',
  overflow: 'hidden',
  '&:before': {
    display: 'none',
  },
  '&.Mui-expanded': {
    margin: 0,
  },
  '& .MuiAccordionSummary-root': {
    minHeight: 44,
    px: 1.25,
    backgroundColor: UPLOAD_PANEL_SUBTLE_BG,
    borderBottom: '1px solid rgba(226, 232, 240, 0.9)',
    '&.Mui-expanded': {
      minHeight: 44,
    },
  },
  '& .MuiAccordionSummary-content': {
    my: 0.9,
    alignItems: 'center',
    '&.Mui-expanded': {
      my: 0.9,
    },
  },
  '& .MuiAccordionDetails-root': {
    p: 1.25,
  },
}

export const UPLOAD_FILE_BOX_SX = {
  minWidth: 0,
  flex: 1,
  px: 1,
  py: 0.75,
  border: '1px solid',
  borderColor: UPLOAD_PANEL_BORDER_COLOR,
  borderRadius: 1.5,
  backgroundColor: UPLOAD_PANEL_SUBTLE_BG,
}

export const UPLOAD_ALERT_SX = {
  py: 0,
  alignItems: 'center',
  '& .MuiAlert-icon': {
    py: 0.5,
    mr: 0.75,
  },
  '& .MuiAlert-message': {
    py: 0.45,
    fontSize: '0.75rem',
    lineHeight: 1.35,
  },
}
