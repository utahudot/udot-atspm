import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { PageNames, useUserHasClaim, useViewPage } from '@/features/identity/pagesCheck'
import {
  useCreateProduct,
  useDeleteProduct,
  useEditProduct,
  useGetProducts,
} from '@/features/products/api/index'
import { Product } from '@/features/products/types/index'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import { useEffect, useState } from 'react'

const ProductsAdmin = () => {
  const pageAccess = useViewPage(PageNames.Products)

  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.Products
  ) as GridColDef[]

  const hasEditClaim = useUserHasClaim('LocationConfiguration:Edit');
  const hasDeleteClaim = useUserHasClaim('LocationConfiguration:Delete');

  const { data: productData, isLoading } = useGetProducts()
  const createMutation = useCreateProduct()
  const deleteMutation = useDeleteProduct()
  const editMutation = useEditProduct()

  useEffect(() => {
    if (productData) {
      setData(productData)
    }
  }, [productData])

  if (pageAccess.isLoading) {
    return
  }

  const HandleCreateProduct = async (productData: Product) => {
    const { manufacturer, model, webPage, notes } = productData

    //TODO: is there a better way to do this? Note from Dan: created sanitized object as backend will only accept vars that have value otherwise they shouldn't be passed through.
    const sanitizedProduct: Partial<Product> = {}

    if (manufacturer) sanitizedProduct.manufacturer = manufacturer
    if (model) sanitizedProduct.model = model
    if (webPage) sanitizedProduct.webPage = webPage
    if (notes) sanitizedProduct.notes = notes

    try {
      createMutation.mutateAsync(sanitizedProduct)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteProduct = async (productData: Product) => {
    const { id } = productData
    try {
      deleteMutation.mutateAsync(id)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditProduct = async (productData: Product) => {
    const { id, manufacturer, model, webPage, notes } = productData
    try {
      editMutation.mutateAsync({
        data: {manufacturer,model,webPage, notes},
        id,
      })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const deleteProduct = (data: Product) => {
    HandleDeleteProduct(data)
  }

  const editProduct = (data: Product) => {
    HandleEditProduct(data)
  }

  const createProduct = (data: Product) => {
    HandleCreateProduct(data)
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

  const filteredData = data?.value.map((obj: Product) => {
    return {
      id: obj.id,
      manufacturer: obj.manufacturer,
      model: obj.model,
      webPage: obj.webPage,
      notes: obj.notes,
    }
  })

  const baseType = {
    manufacturer: '',
    model: '',
    webPage: '',
    notes: '',
  }

  return (
    <ResponsivePageLayout title={'Manage Products'} noBottomMargin>
      <GenericAdminChart
        pageName={PageNames.Products}
        headers={headers}
        data={filteredData}
        baseRowType={baseType}
        onDelete={deleteProduct}
        onEdit={editProduct}
        onCreate={createProduct}
        hasEditPrivileges={hasEditClaim}
        hasDeletePrivileges={hasDeleteClaim}
        
      />
    </ResponsivePageLayout>
  )
}

export default ProductsAdmin
