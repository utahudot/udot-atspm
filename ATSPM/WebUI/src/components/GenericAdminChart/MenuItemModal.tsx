import { useGetMenuItems } from '@/features/links/api/getMenuItems'
import { MenuItems } from '@/features/links/types/linkDto'

import HelpOutlineIcon from '@mui/icons-material/HelpOutline'
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Tooltip,
} from '@mui/material'
import React, { forwardRef, useEffect, useMemo, useState } from 'react'

interface ModalProps {
  open: boolean
  onClose: () => void
  data: MenuItems | null
  onCreate: (menuItem: MenuItems) => void
  onEdit: (menuItem: MenuItems) => void
  onSave: (menuItem: MenuItems) => void
}

const MenuItemsModal = forwardRef(
  ({ open, onClose, data, onCreate, onSave, onEdit }: ModalProps, ref) => {
    const { data: menuItemsData } = useGetMenuItems()

    const [errors, setErrors] = useState({
      name: false,
      link: false,
      parentId: false,
    })

    const [menuItem, setMenuItem] = useState<MenuItems | null>(null)

    useEffect(() => {
      if (data) {
        setMenuItem(data)
      } else {
        setMenuItem({
          id: 0,
          name: '',
          icon: '',
          displayOrder: 0,
          link: '',
          document: null,
          parentId: null,
          children: [],
        })
      }
    }, [data])

    const handleChange = (
      event: React.ChangeEvent<{ name?: string; value: unknown }>
    ) => {
      const { name, value } = event.target
      setMenuItem((prevMenuItem) => {
        if (prevMenuItem) {
          let newValue: unknown = value

          if (name === 'parentId') {
            newValue = value === '' ? null : Number(value)
          } else if (name === 'displayOrder') {
            newValue = value === '' ? 0 : Number(value)
          } else {
            newValue = value
          }

          return {
            ...prevMenuItem,
            [name as keyof MenuItems]: newValue,
          }
        }
        return prevMenuItem
      })

      setErrors((prevErrors) => ({
        ...prevErrors,
        [name as keyof typeof errors]: false,
      }))
    }

    const handleSubmit = async () => {
      if (menuItem) {
        const newErrors = {
          name: !menuItem.name,
          link: false,
          displayOrder: !menuItem.displayOrder,
          parentId: false,
        }
        setErrors(newErrors)

        if (!menuItem.name || !menuItem.displayOrder) {
          return
        }

        // Check if the item has children and parentId is null
        const children = menuItemsData?.value.filter(
          (item: MenuItems) => item.parentId === menuItem.id
        )
        if (
          children &&
          children.length > 0 &&
          menuItem.parentId === null &&
          menuItem.link
        ) {
          setErrors({
            ...newErrors,
            link: true,
          })
          return
        }

        // Check if the item has children and is being added to a parentId
        if (children && children.length > 0 && menuItem.parentId !== null) {
          setErrors({
            ...newErrors,
            parentId: true,
          })
          return
        }

        // Check if the selected parent item has a link
        const parentItem = menuItemsData?.value.find(
          (item: MenuItems) => item.id === menuItem.parentId
        )
        if (parentItem && parentItem.link) {
          setErrors({
            ...newErrors,
            parentId: true,
          })
          return
        }

        if (menuItem.link && children && children.length > 0) {
          setErrors({
            ...newErrors,
            link: true,
          })
          return
        }

        try {
          const sanitizedMenuItem: Partial<MenuItems> = {
            ...menuItem,
            displayOrder: menuItem.displayOrder
              ? parseInt(menuItem.displayOrder.toString())
              : null,
          }

          if (menuItem.id) {
            await onEdit(sanitizedMenuItem as MenuItems)
          } else {
            await onCreate(sanitizedMenuItem as MenuItems)
          }
          onSave(sanitizedMenuItem as MenuItems)
          onClose()
        } catch (error) {
          console.error(
            'Error occurred while editing/creating menu item:',
            error
          )
        }
      }
    }

    const topLevelMenuItems = menuItemsData?.value.filter(
      (item: MenuItems) => item.parentId === null
    )

    const parentItem = useMemo(() => {
      return menuItemsData?.value.find(
        (item: MenuItems) => item.id === menuItem?.parentId
      )
    }, [menuItem?.parentId, menuItemsData])

    return (
      <Dialog open={open} onClose={onClose}>
        <h3 style={{ margin: '15px' }}>
          Menu Item Details
          <Tooltip
            title="Items without a parent ID will appear in the top navbar. They are sorted primarily by display order, then alphabetically. Items without a parent ID and without a link will serve as dropdown buttons. If they contain a link, they will be clickable buttons that redirect the user. Please ensure that the 'Link' field contains a URL link.

          To add clickable items to the dropdown buttons (items without links or parent IDs), create a new menu item and assign its parent ID to the desired dropdown button."
          >
            <IconButton>
              <HelpOutlineIcon style={{ fontSize: '.7em' }} />
            </IconButton>
          </Tooltip>
        </h3>
        <DialogContent>
          {menuItem ? (
            <>
              <TextField
                autoFocus
                margin="dense"
                name="name"
                label="Name"
                type="text"
                fullWidth
                value={menuItem.name}
                onChange={handleChange}
                error={errors.name}
                helperText={errors.name ? 'Name is required' : ''}
              />
              {/* <TextField
                margin="dense"
                name="icon"
                label="Icon"
                type="text"
                fullWidth
                value={menuItem.icon ?? ''}
                onChange={handleChange}
              /> */}
              <TextField
                margin="dense"
                name="displayOrder"
                label="Display Order"
                type="number"
                fullWidth
                value={menuItem.displayOrder ?? ''}
                onChange={handleChange}
                inputProps={{
                  inputMode: 'numeric',
                  pattern: '[0-9]*',
                }}
                error={errors.displayOrder}
                helperText={
                  errors.displayOrder ? 'Display order is required' : ''
                }
              />
              <TextField
                margin="dense"
                name="link"
                label="Link"
                type="text"
                fullWidth
                value={menuItem.link ?? ''}
                onChange={handleChange}
                error={errors.link}
                helperText={
                  errors.link
                    ? 'Please remove children associated with this dropdown first'
                    : ''
                }
              />
              <TextField
                margin="dense"
                name="document"
                label="Document"
                type="text"
                fullWidth
                value={menuItem.document ?? ''}
                onChange={handleChange}
              />
              <FormControl fullWidth margin="dense">
                <InputLabel id="parent-label">Parent</InputLabel>
                <Select
                  labelId="parent-label"
                  id="parent-select"
                  name="parentId"
                  value={menuItem.parentId ?? ''}
                  label="Parent"
                  onChange={handleChange}
                  error={errors.parentId}
                >
                  <MenuItem value="">
                    <em>None</em>
                  </MenuItem>
                  {topLevelMenuItems?.map((item) => (
                    <MenuItem key={item.id} value={item.id}>
                      {item.name}
                    </MenuItem>
                  ))}
                </Select>
                {errors.parentId && (
                  <p style={{ color: 'red', fontSize: '12.5px' }}>
                    {parentItem && parentItem.link
                      ? 'Must remove parentId link first before adding this menu item'
                      : 'Please remove all associated children links before adding a parentId'}
                  </p>
                )}
              </FormControl>
            </>
          ) : (
            <div>Loading...</div>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit}>
            Save
          </Button>
        </DialogActions>
      </Dialog>
    )
  }
)

MenuItemsModal.displayName = 'MenuItemsModal'
export default MenuItemsModal
