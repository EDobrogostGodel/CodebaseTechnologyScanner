import type { ScanResult } from '../models/ScanResult';
import type { PaginatedResponse } from '../models/PaginatedResponse';
import type { DeleteResponse } from '../models/DeleteResponse';
import type { ErrorResponse, DuplicateScanErrorResponse } from '../models/ErrorResponse';
import { config } from '../config/environment';

const API_BASE_URL = config.apiBaseUrl;

export class ScanServiceError extends Error {
  statusCode: number;
  details?: string;
  existingScanId?: string;

  constructor(
    message: string,
    statusCode: number,
    details?: string,
    existingScanId?: string
  ) {
    super(message);
    this.name = 'ScanServiceError';
    this.statusCode = statusCode;
    this.details = details;
    this.existingScanId = existingScanId;
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    
    if (errorData && 'error' in errorData) {
      const err = errorData as ErrorResponse | DuplicateScanErrorResponse;
      throw new ScanServiceError(
        err.error,
        err.statusCode,
        err.details,
        'existingScanId' in err ? err.existingScanId : undefined
      );
    }
    
    throw new ScanServiceError(
      `Request failed with status ${response.status}`,
      response.status
    );
  }
  
  return await response.json();
}

export const scanService = {
  async uploadAndScan(file: File, projectName?: string): Promise<ScanResult> {
    const formData = new FormData();
    formData.append('file', file);
    if (projectName) {
      formData.append('projectName', projectName);
    }

    const response = await fetch(`${API_BASE_URL}/scan/upload`, {
      method: 'POST',
      body: formData,
    });

    return handleResponse<ScanResult>(response);
  },

  async getScanResult(id: string): Promise<ScanResult | null> {
    const response = await fetch(`${API_BASE_URL}/scan/${id}`);
    
    if (response.status === 404) {
      return null;
    }

    return handleResponse<ScanResult>(response);
  },

  async getScanHistory(params?: {
    page?: number;
    pageSize?: number;
    sortBy?: string;
    sortOrder?: string;
    projectName?: string;
    language?: string;
    framework?: string;
  }): Promise<PaginatedResponse> {
    const queryParams = new URLSearchParams();
    
    if (params?.page) queryParams.append('page', params.page.toString());
    if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params?.sortBy) queryParams.append('sortBy', params.sortBy);
    if (params?.sortOrder) queryParams.append('sortOrder', params.sortOrder);
    if (params?.projectName) queryParams.append('projectName', params.projectName);
    if (params?.language) queryParams.append('language', params.language);
    if (params?.framework) queryParams.append('framework', params.framework);

    const url = `${API_BASE_URL}/scan/history${queryParams.toString() ? '?' + queryParams.toString() : ''}`;
    const response = await fetch(url);

    return handleResponse<PaginatedResponse>(response);
  },

  async deleteScanResult(id: string): Promise<DeleteResponse> {
    const response = await fetch(`${API_BASE_URL}/scan/${id}`, {
      method: 'DELETE',
    });

    return handleResponse<DeleteResponse>(response);
  },
};
