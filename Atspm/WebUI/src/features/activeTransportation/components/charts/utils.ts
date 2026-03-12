import type { ECBasicOption } from 'echarts/types/dist/shared'

export type PrintTweaks =
  | boolean
  | {
      /** default true: remove toolbox */
      removeToolbox?: boolean
      /** default true: remove dataZoom (slider + inside) */
      removeDataZoom?: boolean
      /** default true: disable animation (and in series) */
      disableAnimation?: boolean
      /** optional: run a final mutate on the cloned option */
      mutate?: (opt: any) => void
    }

export function applyPrintMode<T extends ECBasicOption>(
  option: T,
  print: PrintTweaks = true
): T {
  const enabled = typeof print === 'boolean' ? print : true
  if (!enabled) return option

  const cfg =
    typeof print === 'boolean'
      ? {
          removeToolbox: true,
          removeDataZoom: true,
          disableAnimation: true,
        }
      : {
          removeToolbox: print.removeToolbox ?? true,
          removeDataZoom: print.removeDataZoom ?? true,
          disableAnimation: print.disableAnimation ?? true,
          mutate: print.mutate,
        }

  const o = JSON.parse(JSON.stringify(option))

  if (cfg.removeToolbox) delete o.toolbox
  if (cfg.removeDataZoom) delete o.dataZoom

  if (cfg.disableAnimation) {
    o.animation = false
    if (o.series) {
      const arr = Array.isArray(o.series) ? o.series : [o.series]
      arr.forEach((s) => {
        s.animation = false
        if (s.emphasis) s.emphasis.animation = false
      })
    }
  }

  if (cfg.mutate) cfg.mutate(o)
  return o as T
}
