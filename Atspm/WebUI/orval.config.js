module.exports = {
  config: {
    input: {
      target:
        'https://config-api-bdppc3riba-wm.a.run.app/swagger/v1/swagger.json',
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
}
