import { Box, Button, Modal, Typography } from '@mui/material'
import { useRouter } from 'next/router'
import { useEffect, useRef, useState } from 'react'

export interface UseUnsavedGuardOptions {
  isDirty: () => boolean
  commit: () => void
}

export interface UseUnsavedGuardReturn {
  /** Call before *any* programmatic navigation. */
  allowNavigate: (target: string) => Promise<boolean>
  /** React component to render once (contains the dialog) */
  Prompt: React.FC
}

export function useUnsavedGuard({
  isDirty,
  commit,
}: UseUnsavedGuardOptions): UseUnsavedGuardReturn {
  const router = useRouter()

  const [promptOpen, setPromptOpen] = useState(false)
  const pendingTarget = useRef<string | null>(null)
  const lastScrollY = useRef(0)
  const resolver = useRef<(ok: boolean) => void>()

  /** ---- 1. API for callers ---- */
  const allowNavigate = (target: string) => {
    if (!isDirty() || target === router.asPath) return Promise.resolve(true)

    pendingTarget.current = target
    setPromptOpen(true)

    return new Promise<boolean>((res) => {
      resolver.current = res
    })
  }

  /** ---- 2. Route / Back/Forward blocking ---- */
  useEffect(() => {
    const guard = (nextUrl: string) => {
      lastScrollY.current = window.scrollY
      if (isDirty() && nextUrl !== router.asPath) {
        pendingTarget.current = nextUrl
        setPromptOpen(true)

        history.pushState(null, '', router.asPath)

        requestAnimationFrame(() => window.scrollTo(0, lastScrollY.current))

        return false
      }
      return true
    }

    const onPop = ({ as }: { as: string }) => guard(as)
    router.beforePopState(onPop)

    const onPush = (url: string) => {
      if (!guard(url)) {
        router.events.emit('routeChangeError')
        throw new Error('Abort navigation (unsaved changes)')
      }
    }
    router.events.on('routeChangeStart', onPush)

    return () => {
      router.beforePopState(() => true)
      router.events.off('routeChangeStart', onPush)
    }
  }, [isDirty, router, setPromptOpen])

  const proceed = () => {
    commit()
    const target = pendingTarget.current
    pendingTarget.current = null
    setPromptOpen(false)
    resolver.current?.(true)

    if (target) {
      router.beforePopState(() => true)
      router.push(target)
    }
  }

  const cancel = () => {
    pendingTarget.current = null
    setPromptOpen(false)
    resolver.current?.(false)
  }

  /** ---- 3. Dialog component ---- */
  const Prompt: React.FC = () => (
    <Modal open={promptOpen} onClose={cancel}>
      <Box
        sx={{
          position: 'absolute',
          top: '50%',
          left: '50%',
          transform: 'translate(-50%, -50%)',
          width: 400,
          bgcolor: 'background.paper',
          borderRadius: '10px',
          boxShadow: 24,
          p: 4,
        }}
      >
        <Typography fontWeight="bold">Unsaved Changes</Typography>
        <Typography>
          There are unsaved changes. Continue without saving?
        </Typography>
        <Box mt={4} display="flex" justifyContent="flex-end" gap={1}>
          <Button onClick={cancel} color="inherit">
            Cancel
          </Button>
          <Button variant="contained" onClick={proceed}>
            Proceed
          </Button>
        </Box>
      </Box>
    </Modal>
  )

  return { allowNavigate, Prompt }
}
