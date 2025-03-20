// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - useConfigEnums.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
  WatchDogIssueTypes = 'WatchDogIssueTypes',
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
