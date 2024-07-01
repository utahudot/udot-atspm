import dynamic from 'next/dynamic'

const JoditEditor = dynamic(() => import('jodit-react'), {
  ssr: false,
})

type TextEditorProps = {
  data: string
  onChange: (newContent: string) => void
}

export default function TextEditor({ data, onChange }: TextEditorProps) {
  const handleContentChange = (newContent: string) => {
    onChange(newContent)
  }
  return (
    <JoditEditor
      value={data}
      onBlur={handleContentChange}
      config={{
        height: '29em',
        buttons: [
          'source',
          '|',
          'bold',
          'strikethrough',
          'underline',
          'italic',
          '|',
          'ul',
          'ol',
          '|',
          'outdent',
          'indent',
          '|',
          'font',
          'fontsize',
          'paragraph',
          '|',
          'table',
          'align',
          'undo',
          'redo',
          '|',
          'hr',
          'eraser',
          '|',
          'selectall',
          'print',
        ],
        removeButtons: [
          'link',
          'image',
          'fullsize',
          'brush',
          'paint format',
          'copyformat',
        ],
      }}
    />
  )
}
