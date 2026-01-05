import { describe, it, expect } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useErrorHandler } from '../useErrorHandler';
import { ScanServiceError } from '../../services/scanService';

describe('useErrorHandler', () => {
  it('initializes with no error', () => {
    const { result } = renderHook(() => useErrorHandler());
    
    expect(result.current.error).toBeNull();
  });

  it('handles ScanServiceError with details', () => {
    const { result } = renderHook(() => useErrorHandler());
    
    const error = new ScanServiceError('Error message', 400, 'Detailed info');
    
    act(() => {
      result.current.handleError(error);
    });
    
    expect(result.current.error).toBe('Detailed info');
  });

  it('handles ScanServiceError without details', () => {
    const { result } = renderHook(() => useErrorHandler());
    
    const error = new ScanServiceError('Error message', 400);
    
    act(() => {
      result.current.handleError(error);
    });
    
    expect(result.current.error).toBe('Error message');
  });

  it('handles generic Error', () => {
    const { result } = renderHook(() => useErrorHandler());
    
    const error = new Error('Generic error');
    
    act(() => {
      result.current.handleError(error);
    });
    
    expect(result.current.error).toBe('Generic error');
  });

  it('handles unknown error type with default message', () => {
    const { result } = renderHook(() => useErrorHandler('Custom default'));
    
    const error = 'plain string error';
    
    act(() => {
      result.current.handleError(error);
    });
    
    expect(result.current.error).toBe('Custom default');
  });

  it('clears error', () => {
    const { result } = renderHook(() => useErrorHandler());
    
    act(() => {
      result.current.handleError(new Error('Test error'));
    });
    
    expect(result.current.error).toBe('Test error');
    
    act(() => {
      result.current.clearError();
    });
    
    expect(result.current.error).toBeNull();
  });

  it('uses provided default message', () => {
    const customDefault = 'Something went wrong';
    const { result } = renderHook(() => useErrorHandler(customDefault));
    
    act(() => {
      result.current.handleError({ unknownType: true });
    });
    
    expect(result.current.error).toBe(customDefault);
  });
});
