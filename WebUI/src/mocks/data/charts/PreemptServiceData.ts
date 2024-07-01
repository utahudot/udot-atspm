export const preemptDetailsData = {
  details: [
    {
      preemptionNumber: 1,
      cycles: [
        {
          inputOff: '2023-08-23T09:43:02.2',
          inputOn: '2023-08-23T09:40:52.9',
          gateDown: '0001-01-01T00:00:00',
          callMaxOut: 0,
          delay: 9,
          timeToService: 8.7,
          dwellTime: 0,
          trackClear: 132.2,
        },
        {
          inputOff: '2023-08-23T12:11:31.5',
          inputOn: '2023-08-23T12:09:14',
          gateDown: '0001-01-01T00:00:00',
          callMaxOut: 0,
          delay: 9,
          timeToService: 9.4,
          dwellTime: 0,
          trackClear: 127.2,
        },
      ],
      locationIdentifier: '6395',
      locationDescription: null,
      start: '2023-08-23T00:00:00',
      end: '2023-08-25T00:00:00',
    },
  ],
  summary: {
    requestAndServices: [
      {
        preemptionNumber: 1,
        requests: ['2023-08-23T09:40:52', '2023-08-23T12:09:14'],
        services: ['2023-08-23T09:41:01', '2023-08-23T12:09:23'],
      },
    ],
    locationIdentifier: '6395',
    locationDescription: '#6395 - Geneva Road & 800 North',
    start: '2023-08-23T00:00:00',
    end: '2023-08-25T00:00:00',
  },
}
