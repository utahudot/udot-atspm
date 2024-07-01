import { Box, List, ListItem, Typography } from '@mui/material'

interface SubMenuProps {
  subheader: string
  children: React.ReactNode
}

/****************************************
When the menu grows too large, uncomment out the code
****************************************/
const SubMenu = ({ children, subheader }: SubMenuProps) => {
  // const [open, setOpen] = useState(true)
  return (
    <ListItem disablePadding>
      <Box width={'100%'}>
        {/* <Button
          onClick={() => setOpen(!open)}
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            width: '100%',
            paddingY: '10px',
            paddingX: '20px',
          }}
        > */}
        <Typography
          sx={{ flexGrow: 1, paddingLeft: 2 }}
          variant="subtitle2"
          color={'textSecondary'}
        >
          {subheader}
        </Typography>
        {/* {open ? (
          <ExpandLess fontSize="small" />
        ) : (
          <ExpandMore fontSize="small" />
        )} */}
        {/* </Button> */}
        {/* <Collapse in={open}> */}
        <List>{children}</List>
        {/* </Collapse> */}
      </Box>
    </ListItem>
  )
}

export default SubMenu
