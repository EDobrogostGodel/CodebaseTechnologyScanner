/**
 * Formatting utilities for displaying data in the UI.
 */

/**
 * Formats an ISO timestamp as a localized date and time string.
 * @param timestamp ISO 8601 timestamp string
 * @returns Formatted date string (e.g., "1/22/2025, 3:45:00 PM")
 */
export const formatDate = (timestamp: string): string => {
  return new Date(timestamp).toLocaleString();
};

/**
 * Formats a file size in bytes as a human-readable string.
 * @param bytes File size in bytes
 * @returns Formatted string (e.g., "15.50 MB")
 */
export const formatFileSize = (bytes: number): string => {
  const mb = bytes / 1024 / 1024;
  return `${mb.toFixed(2)} MB`;
};
