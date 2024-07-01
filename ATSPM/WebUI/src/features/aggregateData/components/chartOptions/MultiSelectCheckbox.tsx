import {
  StyledComponentHeader,
  commonPaperStyle,
} from '@/components/HeaderStyling/StyledComponentHeader'
import { Box, Checkbox, FormControlLabel, Paper } from '@mui/material'

interface MultiSelectCheckboxProps {
  itemList: string[]
  selectedItems: number[]
  setSelectedItems(items: number[]): void
  header: string
}

export const MultiSelectCheckbox = ({
  itemList,
  selectedItems,
  setSelectedItems,
  header,
}: MultiSelectCheckboxProps) => {
  const handleChange = (itemIndex: number, isChecked: boolean) => {
    const arr = isChecked
      ? [...selectedItems, itemIndex]
      : selectedItems.filter((item) => item !== itemIndex)
    setSelectedItems(arr)
  }

  return (
    <Paper
      sx={{
        ...commonPaperStyle,
        minWidth: '170px',
        paddingBottom: '20px',
      }}
    >
      <Box>
        <StyledComponentHeader header={header} />
      </Box>
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-between',
          marginLeft: '25px',
          height: '100%',
        }}
      >
        {itemList.map((item, index) => {
          return (
            <FormControlLabel
              key={index}
              control={
                <Checkbox
                  checked={selectedItems.includes(index)}
                  onChange={(e) => handleChange(index, e.currentTarget.checked)}
                />
              }
              label={item}
            />
          )
        })}
      </Box>
    </Paper>
  )
}
