import { Box, Checkbox, Paper, Typography } from '@mui/material'
import { useEffect, useState } from 'react'

import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'

type Approach = {
  id: string
  description: string
}

type LeftTurnGapOptionsProps = {
  approaches: Approach[]
  approachIds: string[]
  setApproachIds: (approachIds: string[]) => void
}

export const LeftTurnGapOptions = ({
  approaches,
  approachIds,
  setApproachIds,
}: LeftTurnGapOptionsProps) => {
  const [checked, setChecked] = useState<{ [key: string]: boolean }>({})

  const handleCheckChange = (
    event: React.ChangeEvent<HTMLInputElement>,
    approachId: string
  ) => {
    setChecked({ ...checked, [event.target.name]: event.target.checked })
    if (event.target.checked) {
      setApproachIds([...approachIds, approachId])
    } else {
      setApproachIds(approachIds.filter((id) => id !== approachId))
    }
  }

  useEffect(() => {
    if (approaches) {
      const newApproachIds = approaches.map((item) => item.id)
      setApproachIds(newApproachIds)

      const initialCheckedState: { [key: string]: boolean } = {}
      approaches.forEach((approach) => {
        initialCheckedState[approach.description] = true
      })
      setChecked(initialCheckedState)
    } else {
      setApproachIds([])
      setChecked({})
    }
  }, [approaches, setApproachIds])

  return (
    <Paper
      sx={{
        '@media (max-width: 1200px)': {
          marginBottom: 1,
        },
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'flex-start',
      }}
    >
      <StyledComponentHeader header="Left Turn Gap Options" />
      <Box sx={{ p: 2, minWidth: '250px' }}>
        {approaches && approaches.length > 0 ? (
          <Box sx={{ width: '100%' }}>
            {approaches.map((approach, i) => (
              <Box
                key={i}
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  width: '100%',
                }}
              >
                <Checkbox
                  checked={checked[approach.description] || false}
                  onChange={(event) => handleCheckChange(event, approach.id)}
                  name={approach.description}
                  id={`checkbox-${i}`}
                />
                <label htmlFor={`checkbox-${i}`}>{approach.description}</label>
              </Box>
            ))}
          </Box>
        ) : (
          <Typography>Please select a location</Typography>
        )}
      </Box>
    </Paper>
  )
}
