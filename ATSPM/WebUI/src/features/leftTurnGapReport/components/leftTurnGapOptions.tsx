import { Box, Checkbox, Paper } from '@mui/material'
import { useEffect, useState } from 'react'

import {
  StyledComponentHeader,
  commonPaperStyle,
} from '@/components/HeaderStyling/StyledComponentHeader'

type LeftTurnGapOptionsProps = {
  locationIdentifier?: string
  approaches: number[]
  approachIds: number[]
  setApproachIds: (approachIds: number[]) => void
}

export const LeftTurnGapOptions = ({
  locationIdentifier,
  approaches,
  approachIds,
  setApproachIds,
}: LeftTurnGapOptionsProps) => {
  const [checked, setChecked] = useState({})
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
      let newApproach = approaches.map((item) => {
        return item.id
      })
      setApproachIds(newApproach)

      let initialCheckedState = {}
      approaches.forEach((approach) => {
        initialCheckedState[approach.description] = true
      })
      setChecked(initialCheckedState)
    }
  }, [approaches])

  useEffect(() => {
    setApproachIds([])
    setChecked({})
  }, [locationIdentifier])

  return (
    <Paper
      sx={{
        ...commonPaperStyle,

        '@media (max-width: 1200px)': {
          marginBottom: 1,
        },
      }}
    >
      <Box>
        <StyledComponentHeader header={'Left Turn Gap Options'} />
      </Box>
      {approaches && approaches.length > 0 ? (
        <>
          {approaches.map((approach, i) => (
            <Box key={i} sx={{ display: 'flex' }}>
              <Checkbox
                checked={checked[approach.description] || false}
                onChange={(event) => handleCheckChange(event, approach.id)}
                name={approach.description}
                id={`checkbox-${i}`}
              />
              <label style={{ marginTop: '.6rem' }} htmlFor={`checkbox-${i}`}>
                {approach.description}
              </label>
              {/* <p>{approach.description}</p> */}
            </Box>
          ))}
        </>
      ) : (
        <p>Please select a location</p>
      )}
    </Paper>
  )
}
