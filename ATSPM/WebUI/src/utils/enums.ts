// find the equal enum value using either the value or the abbreviation
const findConfigEnumValue = (value: string, enumArray: ConfigEnum[]) => {
  return enumArray.find(
    (member) => member.value === value || member.abbreviation === value
  )?.id
}
