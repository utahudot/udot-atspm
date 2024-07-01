export const locationVersions = {
  '@odata.context':
    'https://localhost:44315/api/v1/$metadata#Locations(jurisdiction(),region())',
  value: [
    {
      latitude: 40.62398502,
      longitude: -111.9387819,
      note: 'Initial',
      primaryName: 'Redwood Road',
      locationIdentifier: '7115',
      secondaryName: '7000 South',
      jurisdictionId: 35,
      chartEnabled: true,
      versionAction: 'Initial',
      start: '2011-01-01T00:00:00-07:00',
      pedsAre1to1: true,
      locationTypeId: 1,
      regionId: 2,
      id: 1680,
      jurisdiction: {
        countyParish: null,
        name: 'UDOT_Reg_2',
        mpo: 'WFRC',
        otherPartners: null,
        id: 35,
        userJurisdictions: [],
      },
      region: {
        description: 'Region 2',
        id: 2,
        userRegions: [],
      },
    },
  ],
}
