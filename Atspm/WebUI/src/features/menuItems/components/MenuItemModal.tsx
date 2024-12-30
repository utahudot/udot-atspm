import { useGetMenuItems } from '@/features/menuItems/api/getMenuItems'
import { MenuItems } from '@/features/menuItems/types/linkDto'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  FormControlLabel,
} from '@mui/material'
import React, { useMemo } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import ParentDropdownTab from './ParentDropdownTabForm'
import MenuItemDropdownLink from './MenuItemDropdownLink'
import NoParentLinkButton from './NoParentLinkButton'

interface ModalProps {
  isOpen: boolean
  data?: MenuItems
  onSave: (menuItem: MenuItems) => void
  onClose: () => void
}

const menuItemSchema = z.object({
  id: z.number().optional(),
  name: z.string().min(1, 'Name is required'),
  icon: z.string().optional(),
  displayOrder: z.coerce
    .number()
    .int()
    .min(0, 'Display order must be a non-negative integer'),
  link: z.string().optional(),
  parentId: z.number().nullable().optional(),
  children: z.array(z.any()).optional(),
}).refine(
  (data) => {
    if (data.parentId === null && data.link && data.link.length > 0) {
      return data.link.startsWith('http://') || data.link.startsWith('https://')
    }
    return true
  },
  {
    message: "Link must start with 'http://' or 'https://' when no parent is selected",
    path: ['link'],
  }
)

type MenuItemFormData = z.infer<typeof menuItemSchema>

const MenuItemsModal: React.FC<ModalProps> = ({
  isOpen,
  onClose,
  data,
  onSave,
}) => {
  const [selectedForm, setSelectedForm] = React.useState<string>('')
  const { data: menuItemsData } = useGetMenuItems()
  const { control, handleSubmit, watch, reset } = useForm<MenuItemFormData>({
    resolver: zodResolver(menuItemSchema),
    defaultValues: {
      id: 0,
      name: '',
      icon: '',
      displayOrder: 0,
      link: '',
      parentId: null,
      children: [],
    },
  })

  // Reset form when modal is opened/closed
  React.useEffect(() => {
    if (isOpen) {
      reset(data)
    } else {
      reset()
      setSelectedForm('')
    }
  }, [isOpen, data, reset])

  const parentItems = useMemo(() =>
    menuItemsData?.value.filter(
      (item: MenuItems) => item.parentId === null && !item.link
    ) || [],
    [menuItemsData]
  )

  const parentLinks = useMemo(() =>
    menuItemsData?.value.filter(
      (item: MenuItems) => item.parentId === null && item.link
    ) || [],
    [menuItemsData]
  )

  const selectedParentId = watch('parentId')
  const selectedParentChildren = useMemo(
    () =>
      menuItemsData?.value.filter(
        (item: MenuItems) => item.parentId === selectedParentId
      ) || [],
    [menuItemsData, selectedParentId]
  )

  const onSubmit = async (formData: MenuItemFormData) => {
    try {
      const sanitizedMenuItem: MenuItems = {
        ...formData,
        displayOrder: parseInt(formData.displayOrder.toString(), 10),
      }
      onSave(sanitizedMenuItem)
      onClose()
    } catch (error) {
      console.error('Error occurred while saving menu item:', error)
    }
  }

  return (
    <Dialog open={isOpen} onClose={onClose}>
      <DialogContent>
        <Box sx={{ display: 'flex', flexDirection: 'row', gap: 2, marginY: 2 }}>
          <FormControlLabel
            control={
              <Checkbox
                checked={selectedForm === 'parentDropdown'}
                onChange={(e) => {
                  if (e.target.checked) {
                    setSelectedForm('parentDropdown')
                    reset({
                      id: 0,
                      name: '',
                      displayOrder: 0,
                      parentId: null,
                      link: null,
                      children: [],
                    })
                  } else {
                    setSelectedForm('')
                  }
                }}
              />
            }
            label="New Dropdown"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={selectedForm === 'menuItemDropdown'}
                onChange={(e) => {
                  if (e.target.checked) {
                    setSelectedForm('menuItemDropdown')
                    reset({
                      id: 0,
                      name: '',
                      displayOrder: 0,
                      parentId: null,
                      link: '',
                      children: [],
                    })
                  } else {
                    setSelectedForm('')
                  }
                }}
              />
            }
            label="Add Menu Item to Dropdown"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={selectedForm === 'noParentLink'}
                onChange={(e) => {
                  if (e.target.checked) {
                    setSelectedForm('noParentLink')
                    reset({
                      id: 0,
                      name: '',
                      displayOrder: 0,
                      parentId: null,
                      link: '',
                      children: [],
                    })
                  } else {
                    setSelectedForm('')
                  }
                }}
              />
            }
            label="No Parent Link Button"
          />
        </Box>

        <form onSubmit={handleSubmit(onSubmit)}>
          {selectedForm === 'parentDropdown' && (
            <ParentDropdownTab control={control} />
          )}
          {selectedForm === 'menuItemDropdown' && (
            <MenuItemDropdownLink
              control={control}
              parentItems={parentItems}
              selectedParentChildren={selectedParentChildren}
            />
          )}
          {selectedForm === 'noParentLink' && (
            <NoParentLinkButton 
              control={control}
              parentLinks={parentLinks}
            />
          )}
          
          <DialogActions>
            <Button onClick={onClose}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={!selectedForm}>
              Save
            </Button>
          </DialogActions>
        </form>
      </DialogContent>
    </Dialog>
  )
}

export default MenuItemsModal