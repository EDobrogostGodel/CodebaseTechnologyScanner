export interface ErrorResponse {
  error: string;
  statusCode: number;
  details?: string;
}

export interface DuplicateScanErrorResponse extends ErrorResponse {
  existingScanId?: string;
}
