import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Alert,
  Box,
  ButtonBase,
  Collapse,
  Typography,
  type AlertProps,
} from '@mui/material'
import { useId, useState } from 'react'

export type ChartMessage =
  | string
  | { message?: string | null }
  | null
  | undefined

export interface ChartMessagesProps {
  messages: readonly ChartMessage[]
  severity?: AlertProps['severity']
  ariaLabel?: string
}

const getMessageText = (message: ChartMessage) =>
  typeof message === 'string'
    ? message.trim()
    : (message?.message?.trim() ?? '')

const getMessageNoun = (severity: AlertProps['severity']) => {
  if (severity === 'warning') return 'warning'
  if (severity === 'error') return 'error'
  if (severity === 'success') return 'confirmation'
  return 'notice'
}

export default function ChartMessages({
  messages,
  severity = 'error',
  ariaLabel = 'Chart messages',
}: ChartMessagesProps) {
  const [expanded, setExpanded] = useState(false)
  const messageListId = useId()
  const messageTexts = messages
    .map(getMessageText)
    .filter((message): message is string => message.length > 0)

  if (messageTexts.length === 0) {
    return null
  }

  const hasMultipleMessages = messageTexts.length > 1
  const messageNoun = getMessageNoun(severity)
  const summary = `${messageTexts.length} ${messageNoun}${hasMultipleMessages ? 's' : ''}`

  return (
    <Alert
      severity={severity}
      aria-label={ariaLabel}
      sx={{
        alignSelf: 'flex-start',
        width: 'fit-content',
        maxWidth: '100%',
        minHeight: 44,
        boxSizing: 'border-box',
        alignItems: 'flex-start',
        py: 0.25,
        '& .MuiAlert-icon': {
          minHeight: hasMultipleMessages ? 40 : undefined,
          alignItems: 'center',
          py: hasMultipleMessages ? 0 : 0.5,
        },
        '& .MuiAlert-message': { minWidth: 0, py: 0.5 },
      }}
    >
      {hasMultipleMessages ? (
        <Box>
          <ButtonBase
            aria-controls={messageListId}
            aria-expanded={expanded}
            onClick={() => setExpanded((isExpanded) => !isExpanded)}
            sx={{
              display: 'flex',
              width: '100%',
              justifyContent: 'space-between',
              gap: 0.75,
              minHeight: 32,
              borderRadius: 0.5,
              textAlign: 'left',
            }}
          >
            <Typography variant="body2" fontWeight={700}>
              {summary}
            </Typography>
            <ExpandMoreIcon
              aria-hidden
              fontSize="small"
              sx={{
                transition: (theme) =>
                  theme.transitions.create('transform', {
                    duration: theme.transitions.duration.shortest,
                  }),
                transform: expanded ? 'rotate(180deg)' : 'rotate(0deg)',
              }}
            />
          </ButtonBase>
          <Collapse in={expanded} unmountOnExit>
            <Box
              component="ul"
              id={messageListId}
              sx={{ m: 0, mt: 0.5, pl: 2.25 }}
            >
              {messageTexts.map((message, index) => (
                <Typography
                  component="li"
                  variant="body2"
                  key={`${message}:${index}`}
                  sx={{ pl: 0.25, '& + &': { mt: 0.25 } }}
                >
                  {message}
                </Typography>
              ))}
            </Box>
          </Collapse>
        </Box>
      ) : (
        <Typography variant="body2">{messageTexts[0]}</Typography>
      )}
    </Alert>
  )
}
