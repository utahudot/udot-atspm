import Document, { Head, Html, Main, NextScript } from 'next/document'

class MyDocument extends Document {
  render() {
    return (
      <Html>
        <Head />
        <body>
          <script
            dangerouslySetInnerHTML={{
              __html: `window.__ENV__ = ${JSON.stringify({
                CONFIG_URL: process.env.CONFIG_URL,
                REPORTS_URL: process.env.REPORTS_URL,
                IDENTITY_URL: process.env.IDENTITY_URL,
                DATA_URL: process.env.DATA_URL,
                MAP_DEFAULT_LATITUDE: process.env.MAP_DEFAULT_LATITUDE,
                MAP_DEFAULT_LONGITUDE: process.env.MAP_DEFAULT_LONGITUDE,
                MAP_TILE_LAYER: process.env.MAP_TILE_LAYER,
                MAP_TILE_ATTRIBUTION: process.env.MAP_TILE_ATTRIBUTION,
              })}`,
            }}
          />
          <Main />
          <NextScript />
        </body>
      </Html>
    )
  }
}

export default MyDocument
