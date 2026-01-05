import { AppBar, Toolbar, Typography, Button, Box } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import ScannerIcon from '@mui/icons-material/Scanner';

export const Header = () => {
  return (
    <AppBar position="static">
      <Toolbar>
        <ScannerIcon sx={{ mr: 2 }} />
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          Codebase Technology Scanner
        </Typography>
        <Box>
          <Button color="inherit" component={RouterLink} to="/">
            Home
          </Button>
          <Button color="inherit" component={RouterLink} to="/scan">
            New Scan
          </Button>
          <Button color="inherit" component={RouterLink} to="/history">
            History
          </Button>
        </Box>
      </Toolbar>
    </AppBar>
  );
};
