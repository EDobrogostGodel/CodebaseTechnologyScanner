import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  CircularProgress,
  Alert,
  Box,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Pagination,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Stack,
} from '@mui/material';
import { Visibility, Delete, FilterList } from '@mui/icons-material';
import { scanService } from '../services/scanService';
import type { PaginatedResponse } from '../models/PaginatedResponse';
import { useErrorHandler } from '../hooks/useErrorHandler';
import { formatDate } from '../utils/formatters';

export const ScanHistoryPage = () => {
  const navigate = useNavigate();
  const [data, setData] = useState<PaginatedResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const { error, handleError, clearError } = useErrorHandler('Failed to load scan history');
  
  // Pagination and filters
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [sortBy, setSortBy] = useState('timestamp');
  const [sortOrder, setSortOrder] = useState('desc');
  const [projectNameFilter, setProjectNameFilter] = useState('');
  const [languageFilter, setLanguageFilter] = useState('');
  const [frameworkFilter, setFrameworkFilter] = useState('');
  const [showFilters, setShowFilters] = useState(false);

  // Delete dialog
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [scanToDelete, setScanToDelete] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);

  const fetchHistory = useCallback(async () => {
    setLoading(true);
    clearError();
    try {
      const result = await scanService.getScanHistory({
        page,
        pageSize,
        sortBy,
        sortOrder,
        projectName: projectNameFilter || undefined,
        language: languageFilter || undefined,
        framework: frameworkFilter || undefined,
      });
      setData(result);
    } catch (err) {
      handleError(err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, sortBy, sortOrder, projectNameFilter, languageFilter, frameworkFilter, handleError, clearError]);

  useEffect(() => {
    fetchHistory();
  }, [fetchHistory]);

  const handleApplyFilters = () => {
    setPage(1);
    fetchHistory();
  };

  const handleClearFilters = () => {
    setProjectNameFilter('');
    setLanguageFilter('');
    setFrameworkFilter('');
    setPage(1);
    fetchHistory();
  };

  const handleDeleteClick = (scanId: string) => {
    setScanToDelete(scanId);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!scanToDelete) return;

    setDeleting(true);
    try {
      await scanService.deleteScanResult(scanToDelete);
      setDeleteDialogOpen(false);
      setScanToDelete(null);
      fetchHistory();
    } catch (err) {
      handleError(err);
    } finally {
      setDeleting(false);
    }
  };

  const handleDeleteCancel = () => {
    setDeleteDialogOpen(false);
    setScanToDelete(null);
  };

  if (loading && !data) {
    return (
      <Container maxWidth="lg">
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (error && !data) {
    return (
      <Container maxWidth="lg">
        <Alert severity="error" sx={{ mt: 4 }}>
          {error}
        </Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h4" component="h1">
          Scan History
        </Typography>
        <Button
          variant="outlined"
          startIcon={<FilterList />}
          onClick={() => setShowFilters(!showFilters)}
        >
          {showFilters ? 'Hide Filters' : 'Show Filters'}
        </Button>
      </Box>

      {showFilters && (
        <Paper sx={{ p: 2, mb: 2 }}>
          <Typography variant="h6" gutterBottom>
            Filters & Sorting
          </Typography>
          <Stack spacing={2}>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <TextField
                label="Project Name"
                variant="outlined"
                size="small"
                value={projectNameFilter}
                onChange={(e) => setProjectNameFilter(e.target.value)}
                sx={{ flex: 1, minWidth: 200 }}
              />
              <TextField
                label="Language"
                variant="outlined"
                size="small"
                value={languageFilter}
                onChange={(e) => setLanguageFilter(e.target.value)}
                sx={{ flex: 1, minWidth: 200 }}
              />
              <TextField
                label="Framework"
                variant="outlined"
                size="small"
                value={frameworkFilter}
                onChange={(e) => setFrameworkFilter(e.target.value)}
                sx={{ flex: 1, minWidth: 200 }}
              />
            </Box>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <FormControl size="small" sx={{ minWidth: 150 }}>
                <InputLabel>Sort By</InputLabel>
                <Select
                  value={sortBy}
                  label="Sort By"
                  onChange={(e) => setSortBy(e.target.value)}
                >
                  <MenuItem value="timestamp">Date</MenuItem>
                  <MenuItem value="projectName">Project Name</MenuItem>
                </Select>
              </FormControl>
              <FormControl size="small" sx={{ minWidth: 150 }}>
                <InputLabel>Sort Order</InputLabel>
                <Select
                  value={sortOrder}
                  label="Sort Order"
                  onChange={(e) => setSortOrder(e.target.value)}
                >
                  <MenuItem value="desc">Descending</MenuItem>
                  <MenuItem value="asc">Ascending</MenuItem>
                </Select>
              </FormControl>
              <Button variant="contained" onClick={handleApplyFilters}>
                Apply Filters
              </Button>
              <Button variant="outlined" onClick={handleClearFilters}>
                Clear Filters
              </Button>
            </Box>
          </Stack>
        </Paper>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {data && data.results.length === 0 ? (
        <Paper sx={{ p: 4, mt: 3, textAlign: 'center' }}>
          <Typography variant="body1" color="text.secondary">
            No scans found. Start by creating a new scan.
          </Typography>
          <Button
            variant="contained"
            onClick={() => navigate('/scan')}
            sx={{ mt: 2 }}
          >
            New Scan
          </Button>
        </Paper>
      ) : data ? (
        <>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Project Name</TableCell>
                  <TableCell>Timestamp</TableCell>
                  <TableCell>Languages</TableCell>
                  <TableCell>Technologies</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.results.map((scan) => (
                  <TableRow key={scan.id}>
                    <TableCell>{scan.projectName}</TableCell>
                    <TableCell>{formatDate(scan.timestamp)}</TableCell>
                    <TableCell>{Object.keys(scan.languageCounts).length}</TableCell>
                    <TableCell>{scan.technologies.length}</TableCell>
                    <TableCell>
                      <IconButton
                        onClick={() => navigate(`/results/${scan.id}`)}
                        color="primary"
                        size="small"
                        title="View"
                      >
                        <Visibility />
                      </IconButton>
                      <IconButton
                        onClick={() => handleDeleteClick(scan.id)}
                        color="error"
                        size="small"
                        title="Delete"
                      >
                        <Delete />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
            <Pagination
              count={data.pagination.totalPages}
              page={data.pagination.currentPage}
              onChange={(_, value) => setPage(value)}
              color="primary"
              showFirstButton
              showLastButton
            />
          </Box>

          <Typography variant="body2" color="text.secondary" align="center" sx={{ mt: 1 }}>
            Showing {data.results.length} of {data.pagination.totalResults} results
          </Typography>
        </>
      ) : null}

      <Dialog open={deleteDialogOpen} onClose={handleDeleteCancel}>
        <DialogTitle>Confirm Delete</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete this scan result? This action cannot be undone.
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
