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
  const handleChange = (itemIndex: number, isChecked: boolean): void => {
    const updatedItems = isChecked
      ? [...selectedItems, itemIndex]
      : selectedItems.filter((item) => item !== itemIndex)
    setSelectedItems(updatedItems)
  }

  const renderCheckbox = (item: string, index: number) => (
    <FormControlLabel
      key={index}
      control={
        <Checkbox
          checked={selectedItems.includes(index)}
          onChange={(e) => handleChange(index, e.currentTarget.checked)}
        />
      }
      label={item}
      {...(direction === 'horizontal' ? { labelPlacement: 'top' } : {})}
    />
  )

  const paperStyles =
    direction === 'vertical'
      ? { ...commonPaperStyle, minWidth: '170px', paddingBottom: '20px' }
      : { minWidth: '170px', paddingBottom: '20px' }

  const checkboxContainerStyles =
    direction === 'vertical'
      ? {
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-between',
          height: '100%',
        }
      : {
          display: 'flex',
          flexDirection: 'row',
          flexWrap: 'nowrap',
          justifyContent: 'space-between',
          gap: 1,
          height: 'auto',
          px: 3,
        }

  return (
    <Paper sx={paperStyles}>
      {direction === 'vertical' ? (
        <StyledComponentHeader header={header} />
      ) : (
        <Box>
          <StyledComponentHeader header={header} />
        </Box>
      )}
      <Box sx={checkboxContainerStyles}>{itemList.map(renderCheckbox)}</Box>
    </Paper>
  )
}
