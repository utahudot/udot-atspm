type PinIconProps = {
  color?: string
  size?: number
}

export default function PinIcon({ color = 'gray', size = 20 }: PinIconProps) {
  const width = (size * 25) / 40
  const height = size

  return (
    <svg
      width={width}
      height={height}
      viewBox="0 0 902 1444"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        fill={color}
        d="
      M451 0
      C201.541 0 0 201.652 0 451.25
      C0 700.848 225.5 1037.88 451 1444
      C676.5 1037.88 902 700.848 902 451.25
      C902 201.652 700.459 0 451 0
      Z
    "
      />
    </svg>
  )
}
