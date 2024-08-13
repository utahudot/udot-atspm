import { useCallback, useRef } from 'react'

const useScrollToElement = () => {
  const elementRef = useRef<HTMLDivElement | null>(null)

  const scrollToElement = useCallback(() => {
    if (elementRef.current) {
      elementRef.current.scrollIntoView({ behavior: 'smooth', block: 'start' })
    }
  }, [])

  return [elementRef, scrollToElement]
}

export default useScrollToElement
