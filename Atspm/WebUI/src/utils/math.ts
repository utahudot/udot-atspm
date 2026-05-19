export function round(num: number, decimalPlaces = 0) {
  num = Math.round(num + 'e' + decimalPlaces)
  return Number(num + 'e' + -decimalPlaces)
}
