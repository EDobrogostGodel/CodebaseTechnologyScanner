import { useState } from 'react';
import {
  Container,
  Typography,
  Paper,
  Box,
  Button,
  TextField,
  Alert,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
} from '@mui/material';
import { CloudUpload } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { scanService, ScanServiceError } from '../services/scanService';
import { useErrorHandler } from '../hooks/useErrorHandler';
import { formatFileSize } from '../utils/formatters';

export const NewScanPage = () => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [projectName, setProjectName] = useState('');
  const [loading, setLoading] = useState(false);
  const { error, handleError, clearError } = useErrorHandler('An error occurred during scanning');
  const [duplicateDialogOpen, setDuplicateDialogOpen] = useState(false);
  const [duplicateScanId, setDuplicateScanId] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      if (!file.name.endsWith('.zip')) {
        handleError(new Error('Please select a ZIP file'));
        setSelectedFile(null);
        return;
      }
      setSelectedFile(file);
      clearError();
    }
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    
    if (!selectedFile) {
      handleError(new Error('Please select a file to upload'));
      return;
    }

    setLoading(true);
    clearError();

    try {
      const result = await scanService.uploadAndScan(
        selectedFile,
        projectName || undefined
      );
      navigate(`/results/${result.id}`);
    } catch (err) {
      if (err instanceof ScanServiceError && err.statusCode === 409 && err.existingScanId) {
        setDuplicateScanId(err.existingScanId);
        setDuplicateDialogOpen(true);
      } else {
        handleError(err);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleViewExistingScan = () => {
    if (duplicateScanId) {
      navigate(`/results/${duplicateScanId}`);
    }
  };

  const handleCloseDuplicateDialog = () => {
    setDuplicateDialogOpen(false);
    setDuplicateScanId(null);
  };

  return (
    <Container maxWidth="md">
      <Typography variant="h4" component="h1" gutterBottom>
        New Project Scan
      </Typography>
      <Paper sx={{ p: 4, mt: 3 }}>
        <form onSubmit={handleSubmit}>
          <Box sx={{ mb: 3 }}>
            <TextField
              fullWidth
              label="Project Name (Optional)"
              variant="outlined"
              value={projectName}
              onChange={(e) => setProjectName(e.target.value)}
              placeholder="My Project"
            />
          </Box>

          <Box sx={{ mb: 3 }}>
            <Button
              variant="outlined"
              component="label"
              startIcon={<CloudUpload />}
              fullWidth
              sx={{ py: 2 }}
            >
              {selectedFile ? selectedFile.name : 'Upload ZIP File'}
              <input
                type="file"
                hidden
                accept=".zip"
                onChange={handleFileChange}
              />
            </Button>
            {selectedFile && (
              <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                File size: {formatFileSize(selectedFile.size)}
              </Typography>
            )}
          </Box>

          {error && (
            <Alert severity="error" sx={{ mb: 3 }}>
              {error}
            </Alert>
          )}

          <Button
            type="submit"
            variant="contained"
            fullWidth
            size="large"
            disabled={!selectedFile || loading}
          >
            {loading ? <CircularProgress size={24} /> : 'Scan Project'}
          </Button>
        </form>
      </Paper>

      {/* Duplicate Scan Dialog */}
      <Dialog open={duplicateDialogOpen} onClose={handleCloseDuplicateDialog}>
        <DialogTitle>Duplicate Scan Detected</DialogTitle>
        <DialogContent>
          <DialogContentText>
            This project has already been scanned. Would you like to view the existing scan results?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDuplicateDialog}>Cancel</Button>
          <Button onClick={handleViewExistingScan} variant="contained" autoFocus>
            View Existing Scan
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};
