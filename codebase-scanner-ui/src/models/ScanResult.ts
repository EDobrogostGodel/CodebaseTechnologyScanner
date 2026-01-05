import type { TechnologyInfo } from './TechnologyInfo';

export interface ScanResult {
  id: string;
  timestamp: string;
  projectName: string;
  technologies: TechnologyInfo[];
  languageCounts: { [key: string]: number };
  fileHash?: string;
}
