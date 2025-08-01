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
import { Backdrop, CircularProgress } from '@mui/material'
import { Markup } from 'interweave'

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
    const { id, header, body, displayOrder } = faqData
    try {
      await editMutation({
        data: faqData,
        key: id,
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
      id: obj.id,
      header: obj.header,
      body: obj.body,
      displayOrder: obj.displayOrder,
    }
  })

  const FaqBodyCell = ({ value }: { value: string }) => {
    return <Markup content={value} />
  }
  const customCellRender = [
    {
      headerKey: 'body',
      component: (value: string) => <FaqBodyCell value={value} />,
    },
  ]

  const headers = ['Header', 'Body', 'Display Order']
  const headerKeys = ['header', 'body', 'displayOrder']

  return (
    <ResponsivePageLayout title="Manage FAQs" noBottomMargin>
      <AdminTable
        pageName="Faqs"
        headers={headers}
        headerKeys={headerKeys}
        data={filteredData}
        customCellRender={customCellRender}
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
