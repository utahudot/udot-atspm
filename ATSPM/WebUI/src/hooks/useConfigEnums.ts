import { useGetRequest } from '@/hooks/useGetRequest'
import { configAxios } from '@/lib/axios'

export enum ConfigEnum {
  LocationVersionActions = 'LocationVersionActions',
  DeviceStatus = 'DeviceStatus',
  DeviceTypes = 'DeviceTypes',
  TransportProtocols = 'TransportProtocols',
  DirectionTypes = 'DirectionTypes',
  MovementTypes = 'MovementTypes',
  LaneTypes = 'LaneTypes',
  DetectionHardwareTypes = 'DetectionHardwareTypes',
  DetectionTypes = 'DetectionTypes',
}

type EnumMember = {
  name: string
  value: number
}

type EnumType = {
  name: string
  members: EnumMember[]
}

const parseEnumsFromXml = (
  xmlData: string,
  enumName: ConfigEnum
): EnumType | undefined => {
  const parser = new DOMParser()
  const xmlDoc = parser.parseFromString(xmlData, 'application/xml')
  const enumTypes = xmlDoc.getElementsByTagName('EnumType')

  const enums = Array.from(enumTypes).map((enumType) => {
    const name = enumType.getAttribute('Name') as string
    const members = Array.from(enumType.getElementsByTagName('Member')).map(
      (member) => ({
        name: member.getAttribute('Name'),
        value: parseInt(member.getAttribute('Value')),
      })
    )

    return { name, members }
  })

  return enums.find((e) => e.name === ConfigEnum[enumName])
}

export function useConfigEnums(enumName: ConfigEnum) {
  const { data, ...rest } = useGetRequest<EnumType>({
    route: '/$metadata',
    configAxios,
    config: {
      select: (xmlData: string) => parseEnumsFromXml(xmlData, enumName),
    },
  })

  const findEnumByNameOrAbbreviation = (
    nameOrAbbreviation: string | number
  ): EnumMember | undefined => {
    if (!data) return undefined
    return data.members.find(
      (member) =>
        member.name === nameOrAbbreviation ||
        member.value === nameOrAbbreviation
    )
  }

  return {
    data: data?.members,
    findEnumByNameOrAbbreviation,
    ...rest,
  }
}
