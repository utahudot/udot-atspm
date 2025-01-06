// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - useConfigEnums.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion

import { useGetRequest } from '@/hooks/useGetRequest'
import { configAxios } from '@/lib/axios'

/**
 * 1) Import or define your numeric-union (or "enum-like") types here.
 *    For example, from a generated schema or custom definition:
 */
import {
  DetectionHardwareTypes,
  DetectionTypes,
  DeviceStatus,
  DeviceTypes,
  DirectionTypes,
  LaneTypes,
  LocationVersionActions,
  MovementTypes,
  TransportProtocols,
  WatchDogIssueTypes,
} from '@/api/config/aTSPMConfigurationApi.schemas'

/**
 * 2) The ConfigEnum includes the "keys" for each category
 *    we might fetch from $metadata:
 */
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

/**
 * 3) Mapping from each ConfigEnum to the numeric-union type you want.
 *    So if you call useConfigEnums(ConfigEnum.MovementTypes),
 *    `.value` will be typed as MovementTypes, etc.
 */
export type ConfigEnumToTSUnion = {
  [ConfigEnum.LocationVersionActions]: LocationVersionActions
  [ConfigEnum.DeviceStatus]: DeviceStatus
  [ConfigEnum.DeviceTypes]: DeviceTypes
  [ConfigEnum.TransportProtocols]: TransportProtocols
  [ConfigEnum.DirectionTypes]: DirectionTypes
  [ConfigEnum.MovementTypes]: MovementTypes
  [ConfigEnum.LaneTypes]: LaneTypes
  [ConfigEnum.DetectionHardwareTypes]: DetectionHardwareTypes
  [ConfigEnum.DetectionTypes]: DetectionTypes
  [ConfigEnum.WatchDogIssueTypes]: WatchDogIssueTypes
}

/**
 * 4) The raw shape returned from parsing the $metadata XML
 */
type EnumMember = {
  name: string
  value: number
}

type EnumType = {
  name: string
  members: EnumMember[]
}

/**
 * 5) Parse the $metadata XML to find the matching EnumType
 */
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
        value: parseInt(member.getAttribute('Value') ?? '0', 10),
      })
    )

    return { name, members }
  })

  // Match the string in ConfigEnum to the <EnumType> in the XML
  return enums.find((e) => e.name === ConfigEnum[enumName])
}

/**
 * 6) Our typed member for T extends keyof ConfigEnumToTSUnion.
 *    .value is strongly typed based on whichever ConfigEnum
 *    we requested.
 */
type ReturnMember<T extends ConfigEnum> = {
  name: string
  value: ConfigEnumToTSUnion[T]
}

/**
 * 7) The main hook that fetches the $metadata and returns
 *    typed members + a find function
 */
export function useConfigEnums<T extends keyof ConfigEnumToTSUnion>(
  enumName: T
) {
  // Fetch the data from /$metadata
  const { data, ...rest } = useGetRequest<EnumType>({
    route: '/$metadata',
    configAxios,
    config: {
      select: (xmlData: string) =>
        parseEnumsFromXml(xmlData, enumName as ConfigEnum),
    },
  })

  // Convert the raw members (value: number) into typed members
  const typedMembers: ReturnMember<T>[] | undefined = data
    ? data.members.map((member) => ({
        name: member.name,
        // Cast the numeric value to the numeric-union type
        value: member.value as ConfigEnumToTSUnion[T],
      }))
    : undefined

  /**
   * Helper to look up a typed member by .name or .value
   */
  const findEnumByNameOrAbbreviation = (
    nameOrAbbreviation: string | number
  ): ReturnMember<T> | undefined => {
    if (!typedMembers) return undefined

    return typedMembers.find(
      (m) => m.name === nameOrAbbreviation || m.value === nameOrAbbreviation
    )
  }

  /**
   * Return an object containing:
   * - `data`: The typed array (e.g. { name, value: MovementTypes })
   * - `findEnumByNameOrAbbreviation`: for lookups
   * - The additional fields from the `useGetRequest` hook
   */
  return {
    data: typedMembers,
    findEnumByNameOrAbbreviation,
    ...rest,
  }
}
