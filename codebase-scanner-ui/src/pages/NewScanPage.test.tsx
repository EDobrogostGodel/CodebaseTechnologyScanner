import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { NewScanPage } from '../NewScanPage';
import * as scanServiceModule from '../../services/scanService';

// Mock the scanService
vi.mock('../../services/scanService', () => ({
  scanService: {
    uploadAndScan: vi.fn(),
  },
  ScanServiceError: class ScanServiceError extends Error {
    statusCode: number;
    details?: string;
    existingScanId?: string;

    constructor(message: string, statusCode: number, details?: string, existingScanId?: string) {
      super(message);
      this.statusCode = statusCode;
      this.details = details;
      this.existingScanId = existingScanId;
    }
  }
}));

// Mock useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

describe('NewScanPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders upload form', () => {
    render(
      <BrowserRouter>
        <NewScanPage />
      </BrowserRouter>
    );

    expect(screen.getByText('New Project Scan')).toBeInTheDocument();
    expect(screen.getByLabelText(/Project Name/i)).toBeInTheDocument();
    expect(screen.getByText(/Upload ZIP File/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Scan Project/i })).toBeInTheDocument();
  });

  it('disables submit button when no file selected', () => {
    render(
      <BrowserRouter>
        <NewScanPage />
      </BrowserRouter>
    );

    const submitButton = screen.getByRole('button', { name: /Scan Project/i });
    expect(submitButton).toBeDisabled();
  });

  it('displays error for non-ZIP file', async () => {
    render(
      <BrowserRouter>
        <NewScanPage />
      </BrowserRouter>
    );

    const file = new File(['content'], 'test.txt', { type: 'text/plain' });
    const input = screen.getByLabelText(/Upload ZIP File/i).parentElement?.querySelector('input[type="file"]') as HTMLInputElement;

    fireEvent.change(input, { target: { files: [file] } });

    await waitFor(() => {
      expect(screen.getByText(/Please select a ZIP file/i)).toBeInTheDocument();
    });
  });

  it('displays file size after selection', async () => {
    render(
      <BrowserRouter>
        <NewScanPage />
      </BrowserRouter>
    );

    const file = new File(['a'.repeat(1024 * 1024)], 'test.zip', { type: 'application/zip' });
    const input = screen.getByLabelText(/Upload ZIP File/i).parentElement?.querySelector('input[type="file"]') as HTMLInputElement;

    fireEvent.change(input, { target: { files: [file] } });

    await waitFor(() => {
      expect(screen.getByText(/File size:/i)).toBeInTheDocument();
      expect(screen.getByText(/1.00 MB/i)).toBeInTheDocument();
    });
  });

  it('calls scanService on form submit', async () => {
    const mockScanResult = {
      id: 'test-id',
      timestamp: new Date().toISOString(),
      projectName: 'Test Project',
      technologies: [],
      languageCounts: {},
    };

    vi.mocked(scanServiceModule.scanService.uploadAndScan).mockResolvedValue(mockScanResult);

    render(
      <BrowserRouter>
        <NewScanPage />
      </BrowserRouter>
    );

    const file = new File(['content'], 'test.zip', { type: 'application/zip' });
    const input = screen.getByLabelText(/Upload ZIP File/i).parentElement?.querySelector('input[type="file"]') as HTMLInputElement;
    const projectNameInput = screen.getByLabelText(/Project Name/i);

    fireEvent.change(input, { target: { files: [file] } });
    fireEvent.change(projectNameInput, { target: { value: 'My Project' } });

    const submitButton = screen.getByRole('button', { name: /Scan Project/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(scanServiceModule.scanService.uploadAndScan).toHaveBeenCalledWith(file, 'My Project');
      expect(mockNavigate).toHaveBeenCalledWith('/results/test-id');
    });
  });

  it('shows duplicate dialog on 409 error', async () => {
    const mockError = new scanServiceModule.ScanServiceError(
      'Duplicate scan',
      409,
      'Already scanned',
      'existing-id'
    );

    vi.mocked(scanServiceModule.scanService.uploadAndScan).mockRejectedValue(mockError);

    render(
      <BrowserRouter>
        <NewScanPage />
      </BrowserRouter>
    );

    const file = new File(['content'], 'test.zip', { type: 'application/zip' });
    const input = screen.getByLabelText(/Upload ZIP File/i).parentElement?.querySelector('input[type="file"]') as HTMLInputElement;

    fireEvent.change(input, { target: { files: [file] } });

    const submitButton = screen.getByRole('button', { name: /Scan Project/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Duplicate Scan Detected')).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /View Existing Scan/i })).toBeInTheDocument();
    });
  });

  it('navigates to existing scan from duplicate dialog', async () => {
    const mockError = new scanServiceModule.ScanServiceError(
      'Duplicate scan',
      409,
      'Already scanned',
      'existing-id'
    );

    vi.mocked(scanServiceModule.scanService.uploadAndScan).mockRejectedValue(mockError);

    render(
      <BrowserRouter>
        <NewScanPage />
      </BrowserRouter>
    );

    const file = new File(['content'], 'test.zip', { type: 'application/zip' });
    const input = screen.getByLabelText(/Upload ZIP File/i).parentElement?.querySelector('input[type="file"]') as HTMLInputElement;

    fireEvent.change(input, { target: { files: [file] } });
    fireEvent.click(screen.getByRole('button', { name: /Scan Project/i }));

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /View Existing Scan/i })).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole('button', { name: /View Existing Scan/i }));

    expect(mockNavigate).toHaveBeenCalledWith('/results/existing-id');
  });
});
