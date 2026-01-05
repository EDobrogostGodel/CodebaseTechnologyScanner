import {
  Card,
  CardContent,
  Typography,
  Chip,
  Box,
  Stack,
} from '@mui/material';
import type { TechnologyInfo } from '../models/TechnologyInfo';

interface ScanResultCardProps {
  technologies: TechnologyInfo[];
  languageCounts: { [key: string]: number };
}

export const ScanResultCard = ({ technologies, languageCounts }: ScanResultCardProps) => {
  const groupedTechnologies = technologies.reduce((acc, tech) => {
    if (!acc[tech.type]) {
      acc[tech.type] = [];
    }
    acc[tech.type].push(tech);
    return acc;
  }, {} as { [key: string]: TechnologyInfo[] });

  return (
    <Card sx={{ mb: 2 }}>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Detected Languages
        </Typography>
        <Box sx={{ mb: 3 }}>
          {Object.entries(languageCounts).length > 0 ? (
            <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
              {Object.entries(languageCounts).map(([language, count]) => (
                <Chip
                  key={language}
                  label={`${language}: ${count} files`}
                  color="primary"
                  variant="outlined"
                />
              ))}
            </Stack>
          ) : (
            <Typography variant="body2" color="text.secondary">
              No languages detected
            </Typography>
          )}
        </Box>

        <Typography variant="h6" gutterBottom>
          Detected Technologies
        </Typography>
        {Object.entries(groupedTechnologies).length > 0 ? (
          Object.entries(groupedTechnologies).map(([type, techs]) => (
            <Box key={type} sx={{ mb: 2 }}>
              <Typography variant="subtitle1" color="text.secondary" gutterBottom>
                {type}
              </Typography>
              <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                {techs.map((tech, index) => (
                  <Chip
                    key={index}
                    label={tech.version ? `${tech.name} (${tech.version})` : tech.name}
                    color="secondary"
                  />
                ))}
              </Stack>
            </Box>
          ))
        ) : (
          <Typography variant="body2" color="text.secondary">
            No technologies detected
          </Typography>
        )}
      </CardContent>
    </Card>
  );
};
