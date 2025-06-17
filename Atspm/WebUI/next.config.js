// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - next.config.js
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
const { version } = require('./package.json')

const withBundleAnalyzer = require('@next/bundle-analyzer')({
  enabled: process.env.ANALYZE === 'true',
})

module.exports = withBundleAnalyzer({
  reactStrictMode: false,
  transpilePackages: ['react-leaflet'],
  output: 'standalone',
  env: {
    version,
  },
  webpack: (config) => {
    config.experiments = { ...config.experiments, topLevelAwait: true }
    return config
  },
  async redirects() {
    return [
      {
        source: '/',
        destination: '/performance-measures',
        permanent: true,
      },
      {
        source: '/signals',
        destination: '/performance-measures',
        permanent: true,
      },
      {
        source: '/locations',
        destination: '/performance-measures',
        permanent: true,
      },
    ]
  },
  typescript: {
    ignoreBuildErrors: true,
  },
  eslint: {
    ignoreDuringBuilds: true,
  },
  experimental: {
    instrumentationHook: true,
  },
})
