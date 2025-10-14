import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Card,
  CardContent,
  Typography,
  Box,
  IconButton,
  Menu,
  MenuItem,
  Snackbar,
  Alert,
} from "@mui/material";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import OpenInNewIcon from "@mui/icons-material/OpenInNew";
import PushPinIcon from "@mui/icons-material/PushPin";
import PushPinOutlinedIcon from "@mui/icons-material/PushPinOutlined";
import MainLayout, { useFavoritedApps } from "../shared/components/MainLayout";
import { getModuleApplications } from "./ModuleRegistry";
import { ApplicationData } from "../shared/types/application";

// All applications come from ModuleRegistry (single source of truth)
const APPLICATIONS: ApplicationData[] = getModuleApplications();

// Helper function to get applications by category
const getApplicationsByCategory = (
  category: "core" | "utility",
): ApplicationData[] => {
  return APPLICATIONS.filter((app) => app.category === category);
};

interface Notification {
  id: string;
  message: string;
  severity: "success" | "info" | "warning" | "error";
  timestamp: number;
}

const Home: React.FC = () => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [selectedApp, setSelectedApp] = useState<number | null>(null);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const open = Boolean(anchorEl);

  // Access favorited apps context
  const { favoritedApps, addFavoritedApp, removeFavoritedApp } =
    useFavoritedApps();

  const navigate = useNavigate();

  const handleLaunchApp = (appName: string) => {
    // Find the app by name and navigate to its path
    const app = APPLICATIONS.find((a) => a.name === appName);
    if (app && app.path) {
      // Derive navigation highlight key from the app path
      // Remove leading slash and use the first path segment
      const pathSegment = app.path.replace("/", "").split("/")[0];

      // Navigate with state to highlight the corresponding nav item
      navigate(app.path, { state: { highlightNav: pathSegment } });
    }
    // Add more navigation routes for other apps as needed
  };

  const handleMenuOpen = (
    event: React.MouseEvent<HTMLElement>,
    appId: number,
  ) => {
    event.stopPropagation(); // Prevent card click event from triggering
    setAnchorEl(event.currentTarget);
    setSelectedApp(appId);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
    setSelectedApp(null);
    // Clear focus to prevent aria-hidden issues
    if (
      document.activeElement &&
      document.activeElement instanceof HTMLElement
    ) {
      document.activeElement.blur();
    }
  };

  const handleOpen = () => {
    handleMenuClose();
    const app = APPLICATIONS.find((app) => app.id === selectedApp);
    if (app) {
      // Implement open functionality here
    }
  };

  // Function to add a new notification
  const addNotification = (
    message: string,
    severity: "success" | "info" | "warning" | "error" = "success",
  ) => {
    const newNotification: Notification = {
      id: `notification-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      message,
      severity,
      timestamp: Date.now(),
    };

    setNotifications((current) => [...current, newNotification]);

    // Auto remove notification after 3 seconds
    setTimeout(() => {
      removeNotification(newNotification.id);
    }, 3000);
  };

  // Function to remove a notification by id
  const removeNotification = (id: string) => {
    setNotifications((current) =>
      current.filter((notification) => notification.id !== id),
    );
  };

  const handlePinToFavorites = () => {
    // Prevent multiple calls
    if (!selectedApp) return;

    const app = APPLICATIONS.find((app) => app.id === selectedApp);
    if (app) {
      const isCurrentlyFavorited = favoritedApps.some(
        (fav) => fav.id === app.id,
      );

      if (isCurrentlyFavorited) {
        removeFavoritedApp(app.id);
        addNotification(`${app.name} removed from favorites`, "info");
      } else {
        addFavoritedApp(app);
        addNotification(`${app.name} added to favorites`, "success");
      }
    }

    handleMenuClose();
  };

  const handleRemoveFromFavorites = () => {
    handleMenuClose();
    const app = APPLICATIONS.find((app) => app.id === selectedApp);
    if (app) {
      removeFavoritedApp(app.id);
      addNotification(`${app.name} removed from favorites`, "info");
    }
  };

  // Helper function to determine if an app is favorited
  const isAppFavorited = (appId: number) => {
    return favoritedApps.some((fav) => fav.id === appId);
  };

  // Separate applications by category
  const coreApps = getApplicationsByCategory("core");
  const utilityApps = getApplicationsByCategory("utility");

  const renderApplicationCard = (app: ApplicationData) => (
    <Box
      key={app.id}
      sx={{
        width: { xs: "100%", sm: "100%", md: "50%", lg: "33.333%", xl: "25%" },
        p: 1.5,
      }}
    >
      <Card
        sx={{
          height: 110,
          cursor: "pointer",
          transition: "all 0.2s ease-in-out",
          "&:hover": {
            transform: "translateY(-2px)",
            boxShadow: 3,
          },
        }}
        onClick={() => handleLaunchApp(app.name)}
      >
        <CardContent
          sx={{
            height: "100%",
            display: "flex",
            flexDirection: "column",
            p: 2,
          }}
        >
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "flex-start",
              mb: 1,
            }}
          >
            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
              <Box
                sx={{
                  fontSize: 24,
                  color: "#1976d2",
                  display: "flex",
                  alignItems: "center",
                }}
              >
                {app.icon}
              </Box>
              <Typography
                variant="h6"
                sx={{ fontWeight: 600, fontSize: "1rem" }}
              >
                {app.name}
              </Typography>
            </Box>
            <IconButton
              size="small"
              onClick={(e) => handleMenuOpen(e, app.id)}
              sx={{
                opacity: 0.7,
                "&:hover": { opacity: 1 },
              }}
            >
              <MoreVertIcon fontSize="small" />
            </IconButton>
          </Box>
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              flexGrow: 1,
              display: "-webkit-box",
              WebkitLineClamp: 2,
              WebkitBoxOrient: "vertical",
              overflow: "hidden",
              fontSize: "0.8rem",
              lineHeight: 1.2,
            }}
          >
            {app.description}
          </Typography>
        </CardContent>{" "}
      </Card>
    </Box>
  );

  return (
    <MainLayout>
      <Box sx={{ p: 3 }}>
        {/* Page Header */}
        <Box sx={{ mb: 4 }}>
          <Typography
            variant="h4"
            component="h1"
            sx={{ mb: 1, fontWeight: 600 }}
          >
            Applications
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Select an application to get started with your analytical workflows
          </Typography>
        </Box>

        {/* Favorited Applications Section */}
        {favoritedApps.length > 0 && (
          <Box sx={{ mb: 4 }}>
            <Typography
              variant="h5"
              sx={{
                mb: 2,
                fontWeight: 600,
                display: "flex",
                alignItems: "center",
                gap: 1,
              }}
            >
              <PushPinIcon sx={{ color: "primary.main" }} />
              Favorites
            </Typography>
            <Box sx={{ display: "flex", flexWrap: "wrap", margin: -1.5 }}>
              {favoritedApps.map(renderApplicationCard)}
            </Box>
          </Box>
        )}

        {/* Core Applications Section */}
        <Box sx={{ mb: 4 }}>
          <Typography variant="h5" sx={{ mb: 2, fontWeight: 600 }}>
            Core Applications
          </Typography>
          <Box sx={{ display: "flex", flexWrap: "wrap", margin: -1.5 }}>
            {coreApps.map(renderApplicationCard)}
          </Box>
        </Box>

        {/* Utility Applications Section */}
        <Box>
          <Typography variant="h5" sx={{ mb: 2, fontWeight: 600 }}>
            Utilities
          </Typography>
          <Box sx={{ display: "flex", flexWrap: "wrap", margin: -1.5 }}>
            {utilityApps.map(renderApplicationCard)}
          </Box>
        </Box>

        {/* Context Menu */}
        <Menu
          anchorEl={anchorEl}
          open={open}
          onClose={handleMenuClose}
          anchorOrigin={{
            vertical: "bottom",
            horizontal: "right",
          }}
          transformOrigin={{
            vertical: "top",
            horizontal: "right",
          }}
        >
          <MenuItem onClick={handleOpen}>
            <OpenInNewIcon sx={{ mr: 1, fontSize: 20 }} />
            Open in New Window
          </MenuItem>
          <MenuItem
            onClick={
              selectedApp && isAppFavorited(selectedApp)
                ? handleRemoveFromFavorites
                : handlePinToFavorites
            }
          >
            {selectedApp && isAppFavorited(selectedApp)
              ? [
                  <PushPinOutlinedIcon
                    key="icon"
                    sx={{ mr: 1, fontSize: 20 }}
                  />,
                  "Remove from Favorites",
                ]
              : [
                  <PushPinIcon key="icon" sx={{ mr: 1, fontSize: 20 }} />,
                  "Add to Favorites",
                ]}
          </MenuItem>
        </Menu>

        {/* Notifications */}
        {notifications.map((notification) => (
          <Snackbar
            key={notification.id}
            open={true}
            autoHideDuration={3000}
            onClose={() => removeNotification(notification.id)}
            anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
          >
            <Alert
              onClose={() => removeNotification(notification.id)}
              severity={notification.severity}
              sx={{ width: "100%" }}
            >
              {notification.message}
            </Alert>
          </Snackbar>
        ))}
      </Box>
    </MainLayout>
  );
};

export default Home;
