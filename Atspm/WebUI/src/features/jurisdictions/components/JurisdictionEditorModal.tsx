import { Jurisdiction } from '@/api/config'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from '@mui/material'
import { SubmitHandler, useForm } from 'react-hook-form'
import { z } from 'zod'

const schema = z.object({
  name: z.string().min(1, { message: 'Name is required' }),
  mpo: z.string().optional(),
  countyParish: z.string().optional(),
  otherPartners: z.string().optional(),
})

type FormData = z.infer<typeof schema>

type JurisdictionEditorModalProps = {
  data?: Jurisdiction
  isOpen: boolean
  onClose: () => void
  onSave: (Jurisdiction: Jurisdiction) => void
}

const JurisdictionEditorModal = ({
  data: jurisdiction,
  isOpen,
  onClose,
  onSave,
}: JurisdictionEditorModalProps) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: jurisdiction?.name || '',
      mpo: jurisdiction?.mpo || '',
      countyParish: jurisdiction?.countyParish || '',
      otherPartners: jurisdiction?.otherPartners || '',
    },
  })

  const onSubmit: SubmitHandler<FormData> = (data) => {
    const updatedJurisdiction = { ...jurisdiction, ...data } as Jurisdiction
    onSave(updatedJurisdiction)
    onClose()
  }

  return (
    <Dialog open={isOpen} onClose={onClose} aria-labelledby="form-dialog-title">
      <DialogTitle id="form-dialog-title">Edit Jurisdiction</DialogTitle>
      <DialogContent>
        <form onSubmit={handleSubmit(onSubmit)}>
          <TextField
            {...register('name')}
            autoFocus
            margin="dense"
            id="name"
            label="Name"
            type="text"
            fullWidth
            error={!!errors.name}
            helperText={errors.name ? errors.name.message : ''}
          />
          <TextField
            {...register('mpo')}
            margin="dense"
            id="mpo"
            label="Mpo"
            type="text"
            fullWidth
            error={!!errors.mpo}
            helperText={errors.mpo ? errors.mpo.message : ''}
          />
          <TextField
            {...register('countyParish')}
            margin="dense"
            id="countyParish"
            label="County Parish"
            type="text"
            fullWidth
            error={!!errors.countyParish}
            helperText={errors.countyParish ? errors.countyParish.message : ''}
          />
          <TextField
            {...register('otherPartners')}
            margin="dense"
            id="otherPartners"
            label="Other Partners"
            type="text"
            fullWidth
            error={!!errors.otherPartners}
            helperText={
              errors.otherPartners ? errors.otherPartners.message : ''
            }
          />
          <DialogActions>
            <Button onClick={onClose} color="primary">
              Cancel
            </Button>
            <Button type="submit" color="primary">
              Save
            </Button>
          </DialogActions>
        </form>
      </DialogContent>
    </Dialog>
  )
}

export default JurisdictionEditorModal
