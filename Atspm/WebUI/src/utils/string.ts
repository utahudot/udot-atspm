// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - string.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
export function addSpaces(s: string): string {
  if (!s) return s

  const result: string[] = []
  let firstCapitalFound = false

  for (const ch of s.toString()) {
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
