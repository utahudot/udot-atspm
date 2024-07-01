import { useGetRequest } from '@/hooks/useGetRequest'
import { configAxios } from '@/lib/axios'
import { AxiosInstance } from 'axios'

export enum ConfigEnum {
  LocationVersionActions,
  DeviceStatus,
  DeviceTypes,
  TransportProtocols,
  DirectionTypes,
  MovementTypes,
  LaneTypes,
  DetectionHardwareTypes,
  DetectionTypes,
}

type EnumMember = {
  name: string
  value: string
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
        name: member.getAttribute('Name') as string,
        value: member.getAttribute('Value') as string,
      })
    )

    return { name, members }
  })

  return enums.find((e) => e.name === ConfigEnum[enumName])
}

export function useConfigEnums(
  enumName: ConfigEnum,
  axiosInstance: AxiosInstance = configAxios
) {
  return useGetRequest<EnumType>({
    route: '/$metadata',
    axiosInstance,
    config: {
      select: (xmlData: string) => parseEnumsFromXml(xmlData, enumName),
    },
  })
}
