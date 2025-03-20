module.exports = {
  config: {
    input: {
      target: './config-spec.json',
    },
    output: {
      target: './src/api/config',
      client: 'react-query',
      templates: './orval-templates',
      // mock: true,
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
