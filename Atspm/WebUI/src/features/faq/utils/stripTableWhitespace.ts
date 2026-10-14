// Table elements can't hold text nodes, so whitespace between rows/cells in
// stored FAQ HTML (e.g. formatted markup from the editor's source view) makes
// React warn when interweave renders it. Drop whitespace that follows a
// table-structure tag and precedes another tag; cell contents are untouched.
const TABLE_WHITESPACE =
  /(<(?:\/?(?:table|thead|tbody|tfoot|tr|colgroup|col)|\/(?:td|th|caption))\b[^>]*>)\s+(?=<)/gi

export function stripTableWhitespace(html: string): string {
  return html.replace(TABLE_WHITESPACE, '$1')
}
