import { useState } from 'react';
import { ScanServiceError } from '../services/scanService';

/**
 * Custom hook for standardized error handling across components.
 * @param defaultMessage Default error message if error type is unknown
 * @returns Object with error state and handler functions
 */
export const useErrorHandler = (defaultMessage = 'An error occurred') => {
  const [error, setError] = useState<string | null>(null);

  /**
   * Handles errors from async operations with type-specific messages.
   */
  const handleError = (err: unknown) => {
    if (err instanceof ScanServiceError) {
      setError(err.details || err.message);
    } else if (err instanceof Error) {
      setError(err.message);
    } else {
      setError(defaultMessage);
    }
  };

  /**
   * Clears the current error state.
   */
  const clearError = () => setError(null);

  return { error, handleError, clearError };
};
