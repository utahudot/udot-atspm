module.exports = {
  // config: {
  //   input: {
  //     target: './orval-api-specs/config-spec.json',
  //   },
  //   output: {
  //     target: './src/api/config',
  //     client: 'react-query',
  //     templates: './orval-templates',
  //     // mock: true,
  //     mode: 'split',
  //     override: {
  //       mutator: {
  //         path: './src/lib/axios.ts',
  //         name: 'configRequest',
  //       },
  //     },
  //   },
  // },
  data: {
    input: {
      target: './config-spec.json',
    },
    output: {
      target: './src/api/config',
      client: 'react-query',
      templates: './orval-templates',
      mock: true,
      mode: 'split',
      override: {
        mutator: {
          path: './src/lib/axios.ts',
          name: 'configRequest',
        },
      },
    },
  },
  // reports: {
  //   input: {
  //     target: './orval-api-specs/reports-spec.json',
  //   },
  //   output: {
  //     target: './src/api/reports',
  //     client: 'react-query',
  //     templates: './orval-templates',
  //     mock: true,
  //     mode: 'split',
  //     override: {
  //       mutator: {
  //         path: './src/lib/axios.ts',
  //         name: 'reportsRequest',
  //       },
  //     },
  //   },
  // },
}
