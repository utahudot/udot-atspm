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
  config: {
    input: {
      target: './config-spec.json',
    },
    output: {
      workspace: './src/api/config',
      target: './',
      client: 'react-query',
      mock: true,
      templates: './orval-templates',
      mode: 'tags-split',
      override: {
        mutator: {
          path: '../../lib/axios.ts',
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
