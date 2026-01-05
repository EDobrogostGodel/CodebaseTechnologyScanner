import { Box } from '@mui/material';
import { Outlet } from 'react-router-dom';

export const MinimalLayout = () => {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        minHeight: '100vh',
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: (theme) => theme.palette.grey[50],
      }}
    >
      <Outlet />
    </Box>
  );
};
