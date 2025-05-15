import TextEditor from "@/components/TextEditor/JoditTextEditor";
import { Faq } from "@/features/faq/types";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  OutlinedInput,
} from "@mui/material";
import { useEffect, useState } from "react";
import { SubmitHandler, useForm } from "react-hook-form";
import sanitizeHtml from "sanitize-html";
import { z } from "zod";

const schema = z.object({
  header: z.string().min(1, { message: "Name is required" }),
  body: z
    .string()
    .transform((str) => {
      const cleanHTML = sanitizeHtml(str, {
        allowedTags: sanitizeHtml.defaults.allowedTags,
        allowedAttributes: {
          ...sanitizeHtml.defaults.allowedAttributes,
          "*": ["style", "class"], // Allow style and class attributes
        },
        disallowedTagsMode: "discard",
      });
      return cleanHTML;
    })
    .refine((str) => str.length > 0, {
      message: "Body content is required",
    }),
  displayOrder: z.number().optional(),
});

type FormData = z.infer<typeof schema>;

interface ModalProps {
  data?: Faq;
  isOpen: boolean;
  onClose: () => void;
  onSave: (faq: Faq) => void;
}

const FaqEditorModal = ({ data: faq, isOpen, onClose, onSave }: ModalProps) => {
  const [createOrEditText, setCreateOrEditText] = useState('')
  const [isClosing, setIsClosing] = useState(false) // Add this state

  const {
    register,
    handleSubmit,
    formState: { errors, touchedFields, isSubmitted },
    setValue,
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      header: faq?.header || "",
      body: faq?.body || "",
      displayOrder: faq?.displayOrder || 0,
    },
  });

  useEffect(() => {
    if (faq?.id !== undefined) {
      setCreateOrEditText("Edit");
    } else {
      setCreateOrEditText("Create");
    }
  }, [faq]);

  const onSubmit: SubmitHandler<FormData> = (data) => {
    const updatedfaq = { ...faq, ...data } as Faq;
    onSave(updatedfaq);
    onClose();
  };

  //NEEDED for dialog to properly close without TextEditor errors.
  const handleClose = (event: {}, reason: string) => {
    if (reason === "backdropClick") {
      setIsClosing(true);
      setTimeout(() => {
        onClose();
        setIsClosing(false);
      }, 0);
    } else {
      onClose();
    }
  };

  return (
    <Dialog
      open={isOpen}
      onClose={handleClose}
      key={isOpen ? "open" : "closed"}
      maxWidth="md"
      fullWidth
    >
      <DialogTitle id="form-dialog-title">{createOrEditText} FAQ</DialogTitle>

      <DialogContent>
        <Box
          sx={{
            display: "flex",
            flexDirection: "row",
            gap: 2,
            marginBottom: 2,
          }}
        >
          <FormControl
            fullWidth
            margin="normal"
            sx={{ flex: 7 }}
            error={!!errors.header}
          >
            <InputLabel htmlFor="faq-header">Header</InputLabel>
            <OutlinedInput
              id="faq-header"
              {...register("header")}
              label="Header"
              error={!!errors.header}
            />
          </FormControl>
          <FormControl
            fullWidth
            margin="normal"
            sx={{ flex: 1 }}
            error={!!errors.displayOrder}
          >
            <InputLabel htmlFor="faq-display-order">Display Order</InputLabel>
            <OutlinedInput
              id="faq-display-order"
              {...register("displayOrder", { valueAsNumber: true })}
              label="Display Order"
              type="number"
              inputProps={{ style: { textAlign: "center" } }}
            />
          </FormControl>
        </Box>
        <TextEditor
          data={faq?.body || ""}
          onChange={(value: string) => {
            setValue("body", value, {
              shouldValidate: isSubmitted,
            });
          }}
          error={isSubmitted && !!errors.body}
          isDisabled={isClosing}
        />
        {errors.body && (
          <span
            style={{ color: "#d32f2f", fontSize: "0.75rem", marginTop: "3px" }}
          >
            {errors.body.message}
          </span>
        )}
      </DialogContent>
      <DialogActions>
        <Box sx={{ marginRight: "1rem", marginBottom: ".5rem" }}>
          <Button onClick={onClose}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit(onSubmit)}>
            {faq?.id ? "Edit FAQ" : "Create FAQ"}
          </Button>
        </Box>
      </DialogActions>
    </Dialog>
  );
};

export default FaqEditorModal;
