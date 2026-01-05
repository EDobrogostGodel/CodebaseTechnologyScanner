import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Typography,
  Paper,
  Box,
  CircularProgress,
  Alert,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  IconButton,
} from '@mui/material';
import { ArrowBack, Delete } from '@mui/icons-material';
import { scanService } from '../services/scanService';
import type { ScanResult } from '../models/ScanResult';
import { ScanResultCard } from '../components/ScanResultCard';
import { useErrorHandler } from '../hooks/useErrorHandler';
import { formatDate } from '../utils/formatters';

export const ScanResultsPage = () => {
  const { scanId } = useParams<{ scanId: string }>();
  const navigate = useNavigate();
  const [scanResult, setScanResult] = useState<ScanResult | null>(null);
  const [loading, setLoading] = useState(true);
  const { error, handleError } = useErrorHandler('Failed to load scan result');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    const fetchScanResult = async () => {
      if (!scanId) {
        handleError(new Error('No scan ID provided'));
        setLoading(false);
        return;
      }

      try {
        const result = await scanService.getScanResult(scanId);
        if (result) {
          setScanResult(result);
        } else {
          handleError(new Error('Scan result not found'));
        }
      } catch (err) {
        handleError(err);
      } finally {
        setLoading(false);
      }
    };

    fetchScanResult();
  }, [scanId, handleError]);

  const handleDeleteClick = () => {
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!scanId) return;

    setDeleting(true);
    try {
      await scanService.deleteScanResult(scanId);
      navigate('/history');
    } catch (err) {
      handleError(err);
      setDeleteDialogOpen(false);
    } finally {
      setDeleting(false);
    }
  };

  const handleDeleteCancel = () => {
    setDeleteDialogOpen(false);
  };

  if (loading) {
    return (
      <Container maxWidth="md">
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (error) {
    return (
      <Container maxWidth="md">
        <Alert severity="error" sx={{ mt: 4 }}>
          {error}
        </Alert>
        <Button
          startIcon={<ArrowBack />}
          onClick={() => navigate('/history')}
          sx={{ mt: 2 }}
        >
          Back to History
        </Button>
      </Container>
    );
  }

  if (!scanResult) {
    return null;
  }

  return (
    <Container maxWidth="md">
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Button
          startIcon={<ArrowBack />}
          onClick={() => navigate('/history')}
        >
          Back to History
        </Button>
        <IconButton
          onClick={handleDeleteClick}
          color="error"
          title="Delete Scan"
        >
          <Delete />
        </IconButton>
      </Box>
      
      <Typography variant="h4" component="h1" gutterBottom>
        Scan Results: {scanResult.projectName}
      </Typography>
      <Paper sx={{ p: 2, mb: 2 }}>
        <Typography variant="body2" color="text.secondary">
          Scan ID: {scanResult.id}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Scanned: {formatDate(scanResult.timestamp)}
        </Typography>
      </Paper>
      <ScanResultCard
        technologies={scanResult.technologies}
        languageCounts={scanResult.languageCounts}
      />

      <Dialog open={deleteDialogOpen} onClose={handleDeleteCancel}>
        <DialogTitle>Confirm Delete</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete this scan result? This action cannot be undone and you will be redirected to the history page.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDeleteCancel} disabled={deleting}>
            Cancel
          </Button>
          <Button onClick={handleDeleteConfirm} color="error" variant="contained" disabled={deleting}>
            {deleting ? <CircularProgress size={24} /> : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};
