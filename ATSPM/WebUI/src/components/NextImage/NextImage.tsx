import Image from 'next/image'

export default function NextImage({
  path,
  alt,
}: {
  path: string
  alt: string
}) {
  return (
    <Image
      alt={alt}
      src={path}
      width={0}
      height={0}
      sizes="100vw"
      style={{
        width: '100%',
        height: 'auto',
        maxWidth: '100%',
        maxHeight: '100%',
      }}
    />
  )
}
