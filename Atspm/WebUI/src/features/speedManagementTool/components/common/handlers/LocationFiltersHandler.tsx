import {
  useGetApiV1AccessCategory,
  useGetApiV1City,
  useGetApiV1County,
  useGetApiV1FunctionalType,
  useGetApiV1Region,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { useEffect, useMemo, useState } from 'react'

export interface LocationFiltersHandler {
  showWarning: boolean
  region: string[] | null
  county: string[] | null
  city: string[] | null
  accessCategory: string[] | null
  functionalType: string[] | null
  updateRegion(region: string[]): void
  updateCounty(county: string[]): void
  updateCity(city: string[]): void
  updateAccessCategory(accessCategory: string[]): void
  updateFunctionalType(functionalType: string[]): void
  counties: string[]
  accessCategories: string[]
  cities: string[]
  functionalTypes: string[]
  regions: string[]
}

export const useLocationFiltersHandler = (): LocationFiltersHandler => {
  const [region, setRegion] = useState<string[] | null>(null)
  const [county, setCounty] = useState<string[] | null>(null)
  const [city, setCity] = useState<string[] | null>(null)
  const [accessCategory, setAccessCategory] = useState<string[] | null>(null)
  const [functionalType, setFunctionalType] = useState<string[] | null>([
    'Blank',
    'Collector Distributer',
    'Interstate',
    'Major Collector',
    'Minor Arterial',
    'Other Principal Arterial',
    'Proposed Major Collector',
    'System To System',
  ])
  const [showWarning, setShowWarning] = useState<boolean>(true)

  const { data: countiesData } = useGetApiV1County()
  const { data: accessCategoriesData } = useGetApiV1AccessCategory()
  const { data: citiesData } = useGetApiV1City()
  const { data: functionalTypesData } = useGetApiV1FunctionalType()
  const { data: regionsData } = useGetApiV1Region()

  const counties = useMemo(
    () => (countiesData ? countiesData.map((item) => item.name) : []),
    [countiesData]
  )
  const accessCategories = useMemo(() => {
    const raw = accessCategoriesData
      ? accessCategoriesData.map((item) => item.name)
      : []
    return [...raw].sort((a, b) => {
      const numA = parseInt(a.match(/\d+/)?.[0] || '-1', 10)
      const numB = parseInt(b.match(/\d+/)?.[0] || '-1', 10)
      if (numA === -1 && numB === -1) return a.localeCompare(b)
      if (numA === -1) return 1
      if (numB === -1) return -1
      return numA - numB
    })
  }, [accessCategoriesData])
  const cities = useMemo(
    () => (citiesData ? citiesData.map((item) => item.name) : []),
    [citiesData]
  )
  const functionalTypes = useMemo(
    () =>
      functionalTypesData ? functionalTypesData.map((item) => item.name) : [],
    [functionalTypesData]
  )
  const regions = useMemo(
    () => (regionsData ? regionsData.map((item) => item.name) : []),
    [regionsData]
  )

  useEffect(() => {
    if (
      region !== null ||
      county !== null ||
      city !== null ||
      accessCategory !== null ||
      functionalType !== null
    ) {
      setShowWarning(false)
    } else {
      setShowWarning(true)
    }
  }, [accessCategory, city, county, functionalType, region])

  const component: LocationFiltersHandler = {
    showWarning,
    region,
    county,
    city,
    accessCategory,
    functionalType,
    counties: counties ?? [],
    cities: cities ?? [],
    accessCategories: accessCategories ?? [],
    functionalTypes: functionalTypes ?? [],
    regions: regions ?? [],
    updateCity(cities: string[]) {
      setCity(cities?.length === 0 ? null : cities)
    },
    updateCounty(counties: string[]) {
      setCounty(counties?.length === 0 ? null : counties)
    },
    updateRegion(regions: string[]) {
      setRegion(regions?.length === 0 ? null : regions)
    },
    updateAccessCategory(accessCategories: string[]) {
      setAccessCategory(
        accessCategories?.length === 0 ? null : accessCategories
      )
    },
    updateFunctionalType(functionalTypes: string[]) {
      setFunctionalType(functionalTypes?.length === 0 ? null : functionalTypes)
    },
  }

  return component
}
