/**
 * Application configuration loaded from environment variables.
 * Uses Vite's import.meta.env for environment variable access.
 */
export const config = {
  /** Base URL for the backend API */
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api',
  
  /** Maximum allowed file upload size in bytes */
  maxFileSize: Number(import.meta.env.VITE_MAX_FILE_SIZE) || 52428800, // 50 MB
} as const;
