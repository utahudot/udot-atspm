
export function addSpaces(s: string): string {
    const result: string[] = []
    let firstCapitalFound = false
   
    for (let i = 0; i < s.length; i++) {
      const ch = s[i]
   
      if (ch === ch.toUpperCase() && ch !== ch.toLowerCase()) {
        if (firstCapitalFound) {
          // Subsequent capital letters
          const prevChar = result[result.length - 1]
          // If previous character is uppercase, append without space
          // Otherwise, insert a space before the capital
          if (
            prevChar === prevChar.toUpperCase() &&
            prevChar !== prevChar.toLowerCase()
          ) {
            result.push(ch)
          } else {
            result.push(' ' + ch)
          }
        } else {
          // First capital letter: just append
          result.push(ch)
          firstCapitalFound = true
        }
      } else {
        // Non-capital characters are appended directly
        result.push(ch)
      }
    }
   
    return result.join('')
  }
   