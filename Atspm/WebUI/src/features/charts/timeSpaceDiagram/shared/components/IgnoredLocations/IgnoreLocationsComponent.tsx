import { Checkbox, FormControlLabel, FormGroup } from '@mui/material'

interface IgnoreLocationsComponentProps {
  locations: string[]
  ignoredLocations: string[]
  setIgnoredLocations: React.Dispatch<React.SetStateAction<string[]>>
}

export const IgnoreLocationsComponent = ({
  locations,
  ignoredLocations,
  setIgnoredLocations,
}: IgnoreLocationsComponentProps) => {
  const toggleLocation = (location: string) => {
    setIgnoredLocations((prev) =>
      prev.includes(location)
        ? prev.filter((l) => l !== location)
        : [...prev, location]
    )
  }

  return (
    <FormGroup>
      {locations.map((location) => (
        <FormControlLabel
          key={location}
          control={
            <Checkbox
              checked={ignoredLocations.includes(location)}
              onChange={() => toggleLocation(location)}
            />
          }
          label={location}
        />
      ))}
    </FormGroup>
  )
}
