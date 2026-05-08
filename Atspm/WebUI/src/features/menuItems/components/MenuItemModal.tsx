import ATSPMDialog from '@/components/ATSPMDialog'
import { useGetMenuItems } from '@/features/menuItems/api/getMenuItems'
import { MenuItems } from '@/features/menuItems/types/linkDto'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  FormControl,
  FormHelperText,
  InputLabel,
  MenuItem,
  Select,
  TextField,
} from '@mui/material'
import { useEffect, useMemo } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

interface ModalProps {
  isOpen: boolean
  data?: MenuItems
  onSave: (menuItem: MenuItems) => void
  onClose: () => void
}

const menuItemSchema = z.object({
  id: z.number().optional(),
  name: z.string().min(1, 'Name is required'),
  icon: z.string().nullable().optional(),
  displayOrder: z.coerce
    .number()
    .int()
    .min(0, 'Display order must be a non-negative integer'),
  link: z.string().nullable().optional(),
  parentId: z.preprocess(
    (v) => (v === 0 || v === '0' || v === '' ? null : v),
    z.number().int().positive().nullable()
  ),
  children: z.array(z.any()).optional(),
})

type MenuItemFormData = z.infer<typeof menuItemSchema>

const MenuItemsModal = ({ isOpen, onClose, data, onSave }: ModalProps) => {
  const { data: menuItemsData } = useGetMenuItems()
  const { control, handleSubmit, setValue, watch } = useForm<MenuItemFormData>({
    resolver: zodResolver(menuItemSchema),
    defaultValues: data || {
      id: 0,
      name: '',
      icon: '',
      displayOrder: 0,
      link: '',
      parentId: 0,
      children: [],
    },
  })

  useEffect(() => {
    if (data) {
      Object.entries(data).forEach(([key, value]) => {
        setValue(
          key as keyof MenuItemFormData,
          key === 'displayOrder' ? Number(value) : value
        )
      })
    } else {
      setValue('id', 0)
      setValue('name', '')
      setValue('icon', '')
      setValue('displayOrder', 0)
      setValue('link', '')
      setValue('parentId', 0)
      setValue('children', [])
    }
  }, [data, setValue])

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

  const topLevelMenuItems = useMemo(
    () =>
      menuItemsData?.value.filter((item: MenuItems) => item.parentId === null),
    [menuItemsData]
  )

  const parentItem = useMemo(
    () =>
      menuItemsData?.value.find(
        (item: MenuItems) => item.id === watch('parentId')
      ),
    [menuItemsData, watch]
  )

  const validateLinkAndChildren = (
    link: string | null | undefined,
    id: number | null
  ) => {
    const children = menuItemsData?.value.filter(
      (item: MenuItems) => item.parentId === id
    )
    return !(children && children.length > 0 && link)
  }

  return (
    <ATSPMDialog
      isOpen={isOpen}
      onClose={onClose}
      title="Menu Item Details"
      dialogProps={{ sx: { maxWidth: 'sm', pt: 0 } }}
      onSubmit={handleSubmit(onSubmit)}
    >
      <Controller
        name="name"
        control={control}
        render={({ field, fieldState }) => (
          <TextField
            {...field}
            autoFocus
            margin="dense"
            label="Name"
            type="text"
            fullWidth
            required
            error={!!fieldState.error}
            helperText={fieldState.error?.message}
          />
        )}
      />

      <Controller
        name="parentId"
        control={control}
        render={({ field, fieldState }) => (
          <FormControl fullWidth margin="dense" error={!!fieldState.error}>
            <InputLabel id="parent-label">Parent</InputLabel>
            <Select
              {...field}
              labelId="parent-label"
              id="parent-select"
              label="Parent"
            >
              <MenuItem value={0}>
                <em>None (top level)</em>
              </MenuItem>
              {topLevelMenuItems?.map((item) => (
                <MenuItem key={item.id} value={item.id}>
                  {item.name}
                </MenuItem>
              ))}
            </Select>
            <FormHelperText>
              {fieldState.error
                ? parentItem && parentItem.link
                  ? 'Parent cannot have a link.'
                  : 'Remove this item’s children links before setting a parent.'
                : 'Pick a dropdown header to nest under, or leave as “None” for top level.'}
            </FormHelperText>
          </FormControl>
        )}
      />

      <Controller
        name="link"
        control={control}
        render={({ field, fieldState }) => {
          const linkChildrenConflict = !validateLinkAndChildren(
            field.value,
            watch('id')
          )
          return (
            <TextField
              {...field}
              value={field.value ?? ''}
              margin="dense"
              label="Link (optional)"
              placeholder="https://example.com/path"
              type="url"
              inputProps={{ inputMode: 'url' }}
              fullWidth
              error={!!fieldState.error || linkChildrenConflict}
              helperText={
                fieldState.error?.message ||
                (linkChildrenConflict
                  ? 'Remove child items before adding a link.'
                  : 'Leave blank to make this a non-clickable dropdown header.')
              }
            />
          )
        }}
      />

      <Controller
        name="displayOrder"
        control={control}
        render={({ field, fieldState }) => (
          <TextField
            {...field}
            margin="dense"
            label="Display Order"
            type="number"
            fullWidth
            inputProps={{ inputMode: 'numeric', pattern: '[0-9]*' }}
            onChange={(e) => field.onChange(Number(e.target.value))}
            error={!!fieldState.error}
            helperText={fieldState.error?.message}
          />
        )}
      />
    </ATSPMDialog>
  )
}

export default MenuItemsModal
