import type { ScanResult } from './ScanResult';

export interface PaginatedResponse {
  results: ScanResult[];
  pagination: PaginationMetadata;
}

export interface PaginationMetadata {
  currentPage: number;
  pageSize: number;
  totalResults: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
