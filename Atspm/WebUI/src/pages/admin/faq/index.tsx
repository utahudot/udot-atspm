import {
  Faq,
  useDeleteFaqFromKey,
  useGetFaq,
  usePatchFaqFromKey,
  usePostFaq,
} from '@/api/config'
import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import FaqEditorModal from '@/features/faq/components/FaqEditorModal'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { useNotificationStore } from '@/stores/notifications'
import { toUTCDateStamp } from '@/utils/dateTime'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { Backdrop, Box, Button, CircularProgress } from '@mui/material'
import { Markup } from 'interweave'
import { useMemo, useState } from 'react'

const FaqAdmin = () => {
  const pageAccess = useViewPage(PageNames.FAQs)
  const { addNotification } = useNotificationStore()

  const hasGeneralEditClaim = useUserHasClaim('GeneralConfiguration:Edit')
  const hasGeneralDeleteClaim = useUserHasClaim('GeneralConfiguration:Delete')

  const { mutateAsync: createMutation } = usePostFaq()
  const { mutateAsync: deleteMutation } = useDeleteFaqFromKey()
  const { mutateAsync: editMutation } = usePatchFaqFromKey()

  const { data: faqData, isLoading, refetch: refetchFaqData } = useGetFaq()
  const faqs = faqData?.value

  if (pageAccess.isLoading) {
    return
  }

  const HandleCreateFaq = async (faqData: Faq) => {
    try {
      await createMutation({ data: faqData })
      refetchFaqData()
      addNotification({ type: 'success', title: 'FAQ Created' })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({ type: 'error', title: 'Error Creating FAQ' })
    }
  }

  const HandleEditFaq = async (faqData: Faq) => {
    try {
      await editMutation({
        data: faqData,
        key: faqData.id,
      })
      refetchFaqData()
      addNotification({ type: 'success', title: 'FAQ Edited' })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({ type: 'error', title: 'Error Editing FAQ' })
    }
  }

  const HandleDeleteFaq = async (id: number) => {
    try {
      await deleteMutation({ key: id })
      refetchFaqData()
      addNotification({ type: 'success', title: 'FAQ Deleted' })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({ type: 'error', title: 'Error Deleting FAQ' })
    }
  }

  const onModalClose = () => {
    //do something?? potentially just delete
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!faqs) {
    return <div>Error returning data</div>
  }

  const filteredData = faqs.map((obj: Faq) => {
    return {
      ...obj,
      created: obj.created ? toUTCDateStamp(obj.created) : undefined,
      modified: obj.modified ? toUTCDateStamp(obj.modified) : undefined,
    }
  })

  const FaqBodyCell = ({
    value,
    maxChars = 300,
  }: {
    value: string
    maxChars?: number
  }) => {
    const [expanded, setExpanded] = useState(false)

    const { preview, needsTruncate } = useMemo(() => {
      const chars = Array.from(value)
      if (chars.length <= maxChars) {
        return { preview: value, needsTruncate: false }
      }
      const sliced = chars.slice(0, maxChars).join('').replace(/\s+$/, '')
      return { preview: `${sliced}...`, needsTruncate: true }
    }, [value, maxChars])

    return (
      <Box sx={{ minWidth: 400, display: 'flex', flexDirection: 'column' }}>
        <Box sx={{ wordBreak: 'break-word', whiteSpace: 'pre-wrap' }}>
          <Markup content={expanded || !needsTruncate ? value : preview} />
        </Box>

        {needsTruncate && (
          <Button
            size="small"
            endIcon={expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
            onClick={() => setExpanded((v) => !v)}
            sx={{ textTransform: 'none' }}
          >
            {expanded ? 'Show less' : 'Show more'}
          </Button>
        )}
      </Box>
    )
  }

  const cells = [
    { key: 'header', label: 'Header' },
    { key: 'body', label: 'Body', component: FaqBodyCell },
    { key: 'displayOrder', label: 'Display Order', align: 'right' },
  ]

  return (
    <ResponsivePageLayout title="Manage FAQs" noBottomMargin>
      <AdminTable
        pageName="Faqs"
        cells={cells}
        data={filteredData}
        hasEditPrivileges={hasGeneralEditClaim}
        hasDeletePrivileges={hasGeneralDeleteClaim}
        editModal={
          <FaqEditorModal
            isOpen={true}
            onSave={HandleEditFaq}
            onClose={onModalClose}
          />
        }
        createModal={
          <FaqEditorModal
            isOpen={true}
            onSave={HandleCreateFaq}
            onClose={onModalClose}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name={''}
            objectType="Faqs"
            deleteLabel={(selectedRow: Faq) => selectedRow.header}
            open={false}
            onClose={() => {}}
            onConfirm={HandleDeleteFaq}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default FaqAdmin
