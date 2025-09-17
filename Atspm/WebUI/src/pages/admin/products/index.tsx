import { Product } from '@/api/config'
import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import {
  useCreateProduct,
  useDeleteProduct,
  useEditProduct,
  useGetProducts,
} from '@/features/products/api/index'
import ProductEditorModal from '@/features/products/components/ProductEditorModal'
import { Backdrop, CircularProgress } from '@mui/material'

const ProductsAdmin = () => {
  const pageAccess = useViewPage(PageNames.Products)

  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDeleteClaim = useUserHasClaim(
    'LocationConfiguration:Delete'
  )

  const { mutateAsync: createMutation } = useCreateProduct()
  const { mutateAsync: deleteMutation } = useDeleteProduct()
  const { mutateAsync: editMutation } = useEditProduct()

  const {
    data: productData,
    isLoading,
    refetch: refetchProducts,
  } = useGetProducts()

  const products = productData?.value

  if (pageAccess.isLoading) {
    return
  }

  const onModalClose = () => {
    //do something?? potentially just delete
  }

  const HandleCreateProduct = async (productData: Product) => {
    const { manufacturer, model, webPage, notes } = productData

    const sanitizedProduct: Partial<Product> = {}

    if (manufacturer) sanitizedProduct.manufacturer = manufacturer
    if (model) sanitizedProduct.model = model
    if (webPage) sanitizedProduct.webPage = webPage
    if (notes) sanitizedProduct.notes = notes

    try {
      await createMutation(sanitizedProduct)
      refetchProducts()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteProduct = async (id: number) => {
    try {
      await deleteMutation(id)
      refetchProducts()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditProduct = async (productData: Product) => {
    const { id, manufacturer, model, webPage, notes } = productData
    try {
      await editMutation({
        data: { manufacturer, model, webPage, notes },
        id,
      })
      refetchProducts()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!products) {
    return <div>Error returning data</div>
  }

  const filteredData = products.map((obj: Product) => {
    return {
      id: obj.id,
      manufacturer: obj.manufacturer,
      model: obj.model,
      webPage: obj.webPage,
      notes: obj.notes,
    }
  })

  const headers = ['Manufacturer', 'Model', 'Web Page', 'Notes']
  const headerKeys = ['manufacturer', 'model', 'webPage', 'notes']

  return (
    <ResponsivePageLayout title="Manage Products" noBottomMargin>
      <AdminTable
        pageName="Product"
        headers={headers}
        headerKeys={headerKeys}
        data={filteredData}
        hasEditPrivileges={hasLocationsEditClaim}
        hasDeletePrivileges={hasLocationsDeleteClaim}
        editModal={
          <ProductEditorModal
            isOpen={true}
            onSave={HandleEditProduct}
            onClose={onModalClose}
          />
        }
        createModal={
          <ProductEditorModal
            isOpen={true}
            onSave={HandleCreateProduct}
            onClose={onModalClose}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name={''}
            objectType="Product"
            deleteLabel={(selectedRow: (typeof filteredData)[number]) =>
              `${selectedRow.manufacturer} - ${selectedRow.model}`
            }
            open={false}
            onClose={() => {}}
            onConfirm={HandleDeleteProduct}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default ProductsAdmin
