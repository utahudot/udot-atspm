module.exports = {
  speedManagement: {
    input: {
      target: './speed-spec.json',
    },
    output: {
      target: './src/api/speedManagement',
      client: 'react-query',
      templates: './orval-templates',
      override: {
        mutator: {
          path: './src/lib/axios.ts',
          name: 'speedRequest',
        },
      },
      mock: true,
      mode: 'split',
    },
  },
  speedManagementZod: {
    input: {
      target: './speed-spec.json',
    },
    output: {
      target: './src/api/speedManagement',
      client: 'react-query',
      templates: './orval-templates',
      override: {
        mutator: {
          path: './src/lib/axios.ts',
          name: 'speedRequest',
        },
      },
      mock: true,
      mode: 'split',
      fileExtension: '.zod.ts',
    },
  },
  // config: {
  //   input: 'https://config-api-bdppc3riba-wm.a.run.app/swagger/v1/swagger.json',
  //   output: {
  //     target: './src/api/config',
  //     client: 'react-query',
  //     templates: './orval-templates',
  //     override: {
  //       mutator: {
  //         path: './src/lib/axios.ts',
  //         name: 'configRequest',
  //       },
  //     },
  //     mode: 'split',
  //   },
  // },
}