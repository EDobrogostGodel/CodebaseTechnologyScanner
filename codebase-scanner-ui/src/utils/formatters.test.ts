import { describe, it, expect } from 'vitest';
import { formatDate, formatFileSize } from '../formatters';

describe('formatters', () => {
  describe('formatDate', () => {
    it('formats ISO timestamp correctly', () => {
      const timestamp = '2025-01-22T10:30:00.000Z';
      const result = formatDate(timestamp);
      
      // Check that it's a valid formatted string (exact format varies by locale)
      expect(result).toBeTruthy();
      expect(result).toContain('2025');
    });

    it('handles different date formats', () => {
      const timestamp1 = '2025-01-01T00:00:00.000Z';
      const timestamp2 = '2025-12-31T23:59:59.000Z';
      
      const result1 = formatDate(timestamp1);
      const result2 = formatDate(timestamp2);
      
      expect(result1).toBeTruthy();
      expect(result2).toBeTruthy();
      expect(result1).not.toEqual(result2);
    });
  });

  describe('formatFileSize', () => {
    it('formats zero bytes correctly', () => {
      const result = formatFileSize(0);
      expect(result).toBe('0.00 MB');
    });

    it('formats 1 MB correctly', () => {
      const result = formatFileSize(1048576);
      expect(result).toBe('1.00 MB');
    });

    it('formats 50 MB correctly', () => {
      const result = formatFileSize(52428800);
      expect(result).toBe('50.00 MB');
    });

    it('formats fractional MB correctly', () => {
      const result = formatFileSize(1572864); // 1.5 MB
      expect(result).toBe('1.50 MB');
    });

    it('formats large files correctly', () => {
      const result = formatFileSize(1073741824); // 1 GB = 1024 MB
      expect(result).toBe('1024.00 MB');
    });
  });
});
