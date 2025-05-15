import dynamic from 'next/dynamic'

const JoditEditor = dynamic(() => import('jodit-react'), {
  ssr: false,
})

type TextEditorProps = {
  data: string
  onChange: (newContent: string) => void
  error?:boolean
  isDisabled?: boolean
}

const errorBorder = {
  border: '1px solid #d32f2f',
  borderRadius: '5px',
}
export default function TextEditor({ data, onChange, error, isDisabled  }: TextEditorProps) {
  

  const handleContentChange = (newContent: string) => {
    if (newContent !== data) {
      onChange(newContent)
    }
  }
  return (
    <div style={error ? errorBorder : {}}>
    <JoditEditor
      value={data}
      onBlur={handleContentChange}
      onChange={handleContentChange} 
      config={{
        height: '29em',
        readonly: isDisabled,
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
    </div>
  )
}
