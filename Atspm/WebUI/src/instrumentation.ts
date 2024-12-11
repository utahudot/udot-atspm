export async function register() {
  if (process.env.NEXT_RUNTIME !== 'nodejs') {
    console.warn('Secrets loading skipped: Not running in Node.js runtime.')
    return
  }

  if (process.env.NEXT_RUNTIME === 'nodejs') {
    const fs = await import('fs') // Safe dynamic import
    const path = await import('path')
    const envDirPath = '/app/Configuration/frontend-env'

    if (fs.existsSync(envDirPath)) {
      // Read all files in the directory
      const files = fs.readdirSync(envDirPath)
      // Filter for .txt files
      const txtFiles = files.filter((file) => file.endsWith('.txt'))
      if (txtFiles.length === 0) {
        console.error('No .txt files found in the directory.')
        return
      }

      // Read and process each .txt file
      txtFiles.forEach((txtFile) => {
        const txtFilePath = path.join(envDirPath, txtFile)
        try {
          const content = fs.readFileSync(txtFilePath, 'utf8')
          const lines = content.split('\n').filter((line) => line.trim() !== '')

          // Parse and set environment variables
          lines.forEach((line) => {
            const [key, value] = line.split('=')
            if (key && value) {
              process.env[key.trim()] = value.trim()
            }
          })
        } catch (error) {
          console.error(`Failed to read file: ${txtFilePath}`, error)
        }
      })
    } else {
      console.error(`Secrets directory not found at ${envDirPath}`)
    }
  }
}
