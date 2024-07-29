import { Autocomplete, TextField } from '@mui/material'
import React from 'react'

interface IssueType {
  Id: number
  name: string
}

interface IssueTypeSelectProps {
  issueTypeData: Record<number, string> | null
  setSelectedIssueTypeData: (id: number) => void
}

export const IssueTypeSelect: React.FC<IssueTypeSelectProps> = ({
  issueTypeData,
  setSelectedIssueTypeData,
}) => {
  const handleChange = (event: any, newValue: IssueType | null) => {
    if (newValue) {
      setSelectedIssueTypeData(newValue.id)
    } else {
      setSelectedIssueTypeData(null)
    }
  }

  const options = issueTypeData
    ? Object.keys(issueTypeData).map((key) => ({
        id: Number(key),
        name: issueTypeData[key],
      }))
    : []

  return (
    <Autocomplete
      options={options}
      getOptionLabel={(option) => `${option.id} - ${option.name}`}
      renderInput={(params) => (
        <TextField {...params} label="Issue Type" variant="outlined" />
      )}
      onChange={handleChange}
    />
  )
}
