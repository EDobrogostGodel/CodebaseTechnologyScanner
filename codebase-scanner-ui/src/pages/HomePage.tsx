import { Container, Typography, Button, Box, Paper } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import ScannerIcon from '@mui/icons-material/Scanner';

export const HomePage = () => {
  const navigate = useNavigate();

  return (
    <Container maxWidth="md">
      <Paper elevation={3} sx={{ p: 4, textAlign: 'center' }}>
        <Box sx={{ mb: 3 }}>
          <ScannerIcon sx={{ fontSize: 80, color: 'primary.main' }} />
        </Box>
        <Typography variant="h3" component="h1" gutterBottom>
          Welcome to Codebase Technology Scanner
        </Typography>
        <Typography variant="h6" color="text.secondary" paragraph>
          Analyze your projects and discover the technologies, languages, and frameworks used
        </Typography>
        <Box sx={{ mt: 4 }}>
          <Button
            variant="contained"
            size="large"
            onClick={() => navigate('/scan')}
            sx={{ mr: 2 }}
          >
            Start New Scan
          </Button>
          <Button
            variant="outlined"
            size="large"
            onClick={() => navigate('/history')}
          >
            View History
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};
