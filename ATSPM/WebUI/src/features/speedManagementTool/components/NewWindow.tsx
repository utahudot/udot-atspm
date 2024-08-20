import { useEffect, useRef, useState } from 'react'
import ReactDOM from 'react-dom'

type SeparateWindowProps = {
  children: React.ReactNode
  onClose?: () => void
}

const SeparateWindow = ({ children, onClose }: SeparateWindowProps) => {
  const [container, setContainer] = useState<HTMLElement | null>(null)
  const newWindow = useRef<Window | null>(null)

  useEffect(() => {
    // Open a new window
    newWindow.current = window.open(
      '',
      '_blank',
      'width=800,height=600,scrollbars=yes,resizable=yes'
    )

    // Create a container div for the new window
    const containerDiv = newWindow.current?.document.createElement('div')
    if (containerDiv) {
      newWindow.current?.document.body.appendChild(containerDiv)
      setContainer(containerDiv)
    }

    // Cleanup on window close or component unmount
    const curWindow = newWindow.current
    const handleWindowClose = () => {
      if (onClose) onClose()
      if (curWindow) {
        curWindow.close()
      }
    }

    curWindow?.addEventListener('beforeunload', handleWindowClose)

    return () => {
      curWindow?.removeEventListener('beforeunload', handleWindowClose)
      if (curWindow) {
        curWindow.close()
      }
    }
  }, [onClose])

  return container ? ReactDOM.createPortal(children, container) : null
}

export default SeparateWindow
