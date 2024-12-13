module.exports = {
  config: {
    input: {
      target: './orval-spec.json',
    },
    output: {
      target: './src/api/config',
      client: 'react-query',
      templates: './orval-templates',
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
