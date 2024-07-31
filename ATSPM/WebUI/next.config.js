const { configureRuntimeEnv } = require('next-runtime-env/build/configure')
configureRuntimeEnv()

const withBundleAnalyzer = require('@next/bundle-analyzer')({
  enabled: process.env.ANALYZE === 'true',
})

module.exports = withBundleAnalyzer({
  reactStrictMode: false,
  transpilePackages: ['react-leaflet'],
  output: 'standalone',
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
})
