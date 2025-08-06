import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'
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
      sx={{ mx: direction === 'horizontal' ? 0.5 : null }}
      control={
        <Checkbox
          sx={{ pt: direction === 'horizontal' ? 0.5 : 1 }}
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
      ? { minWidth: '170px', paddingBottom: '20px' }
      : { paddingBottom: '10px' }

  const checkboxContainerStyles =
    direction === 'vertical'
      ? {
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-between',
          px: 2,
        }
      : {
          display: 'flex',
          flexDirection: 'row',
          flexWrap: 'nowrap',
          justifyContent: 'space-between',
          height: 'auto',
          px: 1,
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
