module.exports = {
  // config: {
  //   input: {
  //     target: './api-specs/config-spec.json',
  //   },
  //   output: {
  //     workspace: './src/api/config',
  //     target: './config-api.ts',
  //     client: 'react-query',
  //     mock: true,
  //     templates: './orval-templates',
  //     mode: 'tags-split',
  //     override: {
  //       mutator: {
  //         path: '../../lib/axios.ts',
  //         name: 'configRequest',
  //       },
  //     },
  //   },
  // },
  // reports: {
  //   input: {
  //     target: './api-specs/reports-spec.json',
  //   },
  //   output: {
  //     workspace: './src/api/reports',
  //     target: './report-api.ts',
  //     client: 'react-query',
  //     mock: true,
  //     templates: './orval-templates',
  //     mode: 'tags-split',
  //     override: {
  //       mutator: {
  //         path: '../../lib/axios.ts',
  //         name: 'reportsRequest',
  //       },
  //     },
  //   },
  // },
  data: {
    input: {
      target: './data-spec.json',
    },
    output: {
      workspace: './src/api/data',
      target: './data-api.ts',
      client: 'react-query',
      mock: true,
      templates: './orval-templates',
      mode: 'tags-split',
      override: {
        mutator: {
          path: '../../lib/axios.ts',
          name: 'dataRequest',
        },
      },
    },
  },
}
