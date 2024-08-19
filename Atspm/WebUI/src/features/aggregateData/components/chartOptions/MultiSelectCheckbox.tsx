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
  direction?: 'vertical' | 'horizontal'
}

export const MultiSelectCheckbox = ({
  itemList,
  selectedItems,
  setSelectedItems,
  header,
  direction = 'vertical',
}: MultiSelectCheckboxProps) => {
  const handleChange = (itemIndex: number, isChecked: boolean) => {
    const arr = isChecked
      ? [...selectedItems, itemIndex]
      : selectedItems.filter((item) => item !== itemIndex)
    setSelectedItems(arr)
  }

  if (direction === 'vertical')
    return (
      <Paper
        sx={{
          ...commonPaperStyle,
          minWidth: '170px',
          paddingBottom: '20px',
        }}
      >
        <StyledComponentHeader header={header} />
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
                    onChange={(e) =>
                      handleChange(index, e.currentTarget.checked)
                    }
                  />
                }
                label={item}
              />
            )
          })}
        </Box>
      </Paper>
    )

  return (
    <Paper
      sx={{
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
          flexDirection: direction === 'vertical' ? 'column' : 'row',
          justifyContent: 'space-between',
          height: direction === 'vertical' ? '200px' : 'auto',
          px: 3,
        }}
      >
        {itemList.map((item, index) => {
          return (
            <Box
              key={index}
              sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
              }}
            >
              <span>{item}</span>
              <Checkbox
                checked={selectedItems.includes(index)}
                onChange={(e) => handleChange(index, e.currentTarget.checked)}
              />
            </Box>
          )
        })}
      </Box>
    </Paper>
  )
}
