import GenericAdminChart from '@/components/GenericAdminChart'
import {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  useCreateFaqs,
  useDeleteFaqs,
  useEditFaqs,
  useGetFaqs,
} from '@/features/faq/api'
import { Faq } from '@/features/faq/types'
import { PageNames, useUserHasClaim, useViewPage } from '@/features/identity/pagesCheck'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import { useEffect, useState } from 'react'
import FaqModal from '@/components/GenericAdminChart/FaqModal'

const FaqAdmin = () => {
  const pageAccess = useViewPage(PageNames.FAQs) 
  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.FAQs
  ) as GridColDef[]

  const hasEditClaim = useUserHasClaim('GeneralConfiguration:Edit');
  const hasDeleteClaim = useUserHasClaim('GeneralConfiguration:Delete');

  const createMutation = useCreateFaqs()
  const deleteMutation = useDeleteFaqs()
  const editMutation = useEditFaqs()

  const { data: faqData, isLoading, refetch: refetchFaqData } = useGetFaqs()

  useEffect(() => {
    if (faqData) {
      setData(faqData)
    }
  }, [faqData])

  if (pageAccess.isLoading) {
    return
  }


  const HandleCreateFaq = async (faqData: Faq) => {
    const { header, body, displayOrder } = faqData
    try {
      createMutation.mutateAsync({
        header,
        body,
        displayOrder
      })
      refetchFaqData()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteFaq = async (faqData: Faq) => {
    const { id } = faqData
    try {
      deleteMutation.mutateAsync(id)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditFaq = async (faqData: Faq) => {
    const { id, header, body, displayOrder } = faqData
    try {
      editMutation.mutateAsync({
        data: { header, body, displayOrder },
        id,
      })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const deleteFaq = (data: Faq) => {
    HandleDeleteFaq(data)
  }

  const editFaq = (data: Faq) => {
    HandleEditFaq(data)
  }

  const createFaq = (data: Faq) => {
    HandleCreateFaq(data)
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!data) {
    return <div>Error returning data</div>
  }

  const filteredData = data.value.map((obj: Faq) => {
    return {
      id: obj.id,
      header: obj.header,
      body: obj.body,
      displayOrder: obj.displayOrder,
    }
  })

  const baseType = {
    header: '',
    body: '',
    displayOrder: '',
  }

  return (
    <ResponsivePageLayout title={'Manage FAQs'} noBottomMargin>
      <GenericAdminChart
        headers={headers}
        data={filteredData}
        baseRowType={baseType}
        onDelete={deleteFaq}
        onEdit={editFaq}
        onCreate={createFaq}
        pageName={PageNames.FAQs}
        hasEditPrivileges={hasEditClaim}
        hasDeletePrivileges={hasDeleteClaim}
        customModal={
          <FaqModal
            onCreate={HandleCreateFaq}
            onEdit={editFaq}
            onDelete={deleteFaq}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default FaqAdmin
