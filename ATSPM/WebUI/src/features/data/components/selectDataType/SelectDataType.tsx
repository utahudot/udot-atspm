//Data Type

// import HelpOutlineIcon from '@mui/icons-material/HelpOutline'
import {
  Box,
  Divider,
  List,
  ListItem,
  ListItemText,
  Paper,
  // TextField,
  Typography,
} from '@mui/material'
import Tooltip from '@mui/material/Tooltip'
import { useState } from 'react'
import { useGetAggDataTypes } from '../../exportData/api/getAggDataTypes'

interface SelectDataTypeProps {
  selectedDataType: string | null
  setSelectedDataType: (dataType: string | null) => void
  eventCodes: string | null
  setEventCodes: (codes: string | null) => void
  eventParams: string
  setEventParams: (params: string) => void
  start: Date
  end: Date
}

const headerStyling = {
  padding: '15px',
  fontSize: '15px',
  fontWeight:'bold',
  borderRadius: '5px 5px 0 0',
  width: '100%',
  textAlign: 'center',
}

const commonPaperStyle = {
  flexGrow: 1,
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'left',
  paddingBottom: '30px',
  marginBottom: '20px',
}

export const SelectDataType = ({
  selectedDataType,
  setSelectedDataType,
  // eventCodes,
  // setEventCodes,
  // eventParams,
  // setEventParams,
}: SelectDataTypeProps) => {
  const [isAllRawDataSelected, setIsAllRawDataSelected] =
    useState<boolean>(true)
  let dataWithRawData: string[] = []

  const { data: AggData, isLoading: dataTypeIsLoading } = useGetAggDataTypes()

  if (AggData) {
    dataWithRawData = ['All Raw Data', ...AggData]
  }

  const formatPascalCase = (option: string) => {
return option.replace(/Aggregation/g, '').replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  const handleLabelSelect = (label: string) => {
    setSelectedDataType(label)
    setIsAllRawDataSelected(label === 'All Raw Data')
  }
  // const HandleEventCodes = (event: React.ChangeEvent<HTMLInputElement>) => {
  //   const value = event.target.value
  //   // Allow only numbers and commas
  //   if (/^[0-9,]*$/.test(value)) {
  //     setEventCodes(value)
  //   }
  // }
  // const HandleEventParams = (event: React.ChangeEvent<HTMLInputElement>) => {
  //   const value = event.target.value
  //   // Allow only numbers and commas
  //   if (/^[0-9,]*$/.test(value)) {
  //     setEventParams(value)
  //   }
  // }
  const renderDataTypes = () => {
    let maxHeight = '600px';
    return (
      <Paper sx={{ ...commonPaperStyle, maxHeight }}>
        <Typography sx={{ ...headerStyling }}>Data Type</Typography>
        <Box
          sx={{ maxHeight: maxHeight, overflow: 'auto', marginBottom: '-27px' }}
        >
          <List
            sx={{ marginTop: '-8px' }}
            component="nav"
            aria-label="data aggregation labels"
          >
            {dataWithRawData?.map((label, index) => {
              const listItems = [];
  
              // Always add the list item
              listItems.push(
                <ListItem
                  button
                  selected={selectedDataType === label}
                  onClick={() => handleLabelSelect(label)}
                  key={label}
                >
                  <ListItemText primary={formatPascalCase(label)} />
                </ListItem>
              );
  
              // Conditionally add the sticky divider after "All Raw Data"
              if (label === "All Raw Data") {
                listItems.push(
                  <Divider
                    key="aggregations-divider"
                    sx={{
                      bgcolor: 'white',
                      paddingX: '2rem',
                      paddingTop: '.5rem',
                      paddingBottom: '.3rem',
                      position: 'sticky',
                      top: 0,
                      zIndex: 1,
                    }}
                  >
                    <Typography variant="caption">Aggregations</Typography>
                  </Divider>
                );
              }
  
              return listItems;
            })}
          </List>
        </Box>
      </Paper>
    );
  };
  
  // const allDataInputParams = () => {
  //   const tooltipContent = (
  //     <div>
  //       <p>Leave blank for all Event Codes/Event Parameters</p>
  //       <p>
  //         To filter on a list of codes/params, use comma and/or dash. E.g.: 1,
  //         3, 6-8, 10
  //       </p>
  //       <a
  //         href="https://docs.lib.purdue.edu/jtrpdata/3/"
  //         target="_blank"
  //         rel="noopener noreferrer"
  //         style={{ color: 'white', textDecoration: 'underline' }}
  //       >
  //         Indiana Hi Resolution Data Logger Enumerations
  //       </a>
  //     </div>
  //   )

  //   return (
  //     <Paper
  //       sx={{
  //         ...commonPaperStyle,
  //         display: 'flex',
  //         flexDirection: 'column',
  //         justifyContent: 'center', // Centers content vertically
  //         alignItems: 'center', // Centers content horizontally
  //         height: 'auto', // Adjust height as needed
  //       }}
  //     >
  //       <Box
  //         sx={{
  //           display: 'flex',
  //           flexDirection: 'column',
  //           alignItems: 'center',
  //           mt: 2,
  //           // mb: 1, // Margin bottom for spacing between boxes
  //         }}
  //       >
  //         <label htmlFor="evenCodesInput">
  //           Event Codes
  //           <Tooltip title={tooltipContent} arrow>
  //             <HelpOutlineIcon style={{ fontSize: 16, marginLeft: 5 }} />
  //           </Tooltip>
  //         </label>
  //         <TextField
  //           id="evenCodesInput"
  //           type="text"
  //           value={eventCodes}
  //           onChange={HandleEventCodes}
  //           placeholder="Comma Separated List"
  //           // sx={{...textFieldStyle}}
  //         />
  //       </Box>
  //       <Box
  //         sx={{
  //           display: 'flex',
  //           flexDirection: 'column',
  //           alignItems: 'center',
  //         }}
  //       >
  //         <label htmlFor="evenParamInput">Event Parameters</label>
  //         <TextField
  //           id="evenCodesInput"
  //           type="text"
  //           value={eventParams}
  //           onChange={HandleEventParams}
  //           placeholder="Comma Separated List"
  //           // sx={{...textFieldStyle}}
  //         />
  //       </Box>
  //     </Paper>
  //   )
  // }

  return (
    <Box>
      <Box display="flex" flexDirection="column" sx={{ maxHeight: '600px' }}>
        {renderDataTypes()}
        {/* {isAllRawDataSelected && allDataInputParams()} */}
      </Box>
    </Box>
  )
}

export default SelectDataType
