import { useGetMenuItems } from "@/features/menuItems/api/getMenuItems";
import { MenuItems } from "@/features/menuItems/types/linkDto";
import { zodResolver } from "@hookform/resolvers/zod";
import HelpOutlineIcon from "@mui/icons-material/HelpOutline";
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
import { useEffect, useMemo } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

interface ModalProps {
  isOpen: boolean;
  data?: MenuItems;
  onSave: (menuItem: MenuItems) => void;
  onClose: () => void;
}

const menuItemSchema = z.object({
  id: z.number().optional(),
  name: z.string().min(1, "Name is required"),
  icon: z.string().nullable().optional(),
  displayOrder: z.coerce
    .number()
    .int()
    .min(0, "Display order must be a non-negative integer"),
  link: z.string().nullable().optional(),
  parentId: z.number().nullable().optional(),
  children: z.array(z.any()).optional(), // Adjust this based on actual structure if needed
});

type MenuItemFormData = z.infer<typeof menuItemSchema>;

const MenuItemsModal = ({ isOpen, onClose, data, onSave }: ModalProps) => {
  const { data: menuItemsData } = useGetMenuItems()
  const { control, handleSubmit, setValue, watch } = useForm<MenuItemFormData>({
    resolver: zodResolver(menuItemSchema),
    defaultValues: data || {
      id: 0,
      name: "",
      icon: "",
      displayOrder: 0,
      link: "",
      parentId: null,
      children: [],
    },
  });

  useEffect(() => {
    if (data) {
      Object.entries(data).forEach(([key, value]) => {
        setValue(
          key as keyof MenuItemFormData,
          key === "displayOrder" ? Number(value) : value
        );
      });
    } else {
      setValue("id", 0);
      setValue("name", "");
      setValue("icon", "");
      setValue("displayOrder", 0);
      setValue("link", "");
      setValue("parentId", null);
      setValue("children", []);
    }
  }, [data, setValue]);

  const onSubmit = async (formData: MenuItemFormData) => {
    try {
      const sanitizedMenuItem: MenuItems = {
        ...formData,
        displayOrder: parseInt(formData.displayOrder.toString(), 10),
      };
      onSave(sanitizedMenuItem);
      onClose();
    } catch (error) {
      console.error("Error occurred while saving menu item:", error);
    }
  };

  const topLevelMenuItems = useMemo(
    () =>
      menuItemsData?.value.filter((item: MenuItems) => item.parentId === null),
    [menuItemsData]
  );

  const parentItem = useMemo(
    () =>
      menuItemsData?.value.find(
        (item: MenuItems) => item.id === watch("parentId")
      ),
    [menuItemsData, watch]
  );

  const validateLinkAndChildren = (link: string, id: number | null) => {
    const children = menuItemsData?.value.filter(
      (item: MenuItems) => item.parentId === id
    );
    return !(children && children.length > 0 && link);
  };

  return (
    <Dialog open={isOpen} onClose={onClose}>
      <h3 style={{ margin: "15px" }}>
        Menu Item Details
        <Tooltip
          title="Items without a parent ID will appear in the top navbar. They are sorted primarily by display order, then alphabetically. Items without a parent ID and without a link will serve as dropdown buttons. If they contain a link, they will be clickable buttons that redirect the user. Please ensure that the 'Link' field contains a URL link.

          To add clickable items to the dropdown buttons (items without links or parent IDs), create a new menu item and assign its parent ID to the desired dropdown button."
        >
          <IconButton>
            <HelpOutlineIcon style={{ fontSize: ".7em" }} />
          </IconButton>
        </Tooltip>
      </h3>
      <DialogContent>
        <form
          onSubmit={(e) => {
            e.preventDefault();
            handleSubmit(
              (formData) => {
                onSubmit(formData);
              },
              (errors) => {
                console.error("Validation Errors:", errors); // Log validation errors
              }
            )();
          }}
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
                error={!!fieldState.error}
                helperText={fieldState.error?.message}
              />
            )}
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
                inputProps={{ inputMode: "numeric", pattern: "[0-9]*" }}
                onChange={(e) => field.onChange(Number(e.target.value))} // Convert input to number
                error={!!fieldState.error}
                helperText={fieldState.error?.message}
              />
            )}
          />
          <Controller
            name="link"
            control={control}
            render={({ field, fieldState }) => (
              <TextField
                {...field}
                margin="dense"
                label="Link"
                type="text"
                fullWidth
                error={
                  !!fieldState.error ||
                  !validateLinkAndChildren(field.value, watch("id"))
                }
                helperText={
                  fieldState.error?.message ||
                  (!validateLinkAndChildren(field.value, watch("id"))
                    ? "Please remove children associated with this dropdown first"
                    : "")
                }
              />
            )}
          />

          <Controller
            name="parentId"
            control={control}
            render={({ field, fieldState }) => (
              <FormControl fullWidth margin="dense">
                <InputLabel id="parent-label">Parent</InputLabel>
                <Select
                  {...field}
                  labelId="parent-label"
                  id="parent-select"
                  label="Parent"
                  error={!!fieldState.error}
                >
                  <MenuItem value={null}>
                    <em>None</em>
                  </MenuItem>
                  {topLevelMenuItems?.map((item) => (
                    <MenuItem key={item.id} value={item.id}>
                      {item.name}
                    </MenuItem>
                  ))}
                </Select>
                {fieldState.error && (
                  <p style={{ color: "red", fontSize: "12.5px" }}>
                    {parentItem && parentItem.link
                      ? "Must remove parentId link first before adding this menu item"
                      : "Please remove all associated children links before adding a parentId"}
                  </p>
                )}
              </FormControl>
            )}
          />
          <DialogActions>
            <Button onClick={onClose}>Cancel</Button>
            <Button type="submit" variant="contained">
              Save
            </Button>
          </DialogActions>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default MenuItemsModal;
