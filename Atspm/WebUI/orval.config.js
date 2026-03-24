// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - orval.config.js
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
