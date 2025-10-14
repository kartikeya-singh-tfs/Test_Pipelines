import React, { useState, createContext, useContext, useEffect } from "react";
import { useUser } from "../contexts/UserContext";
import { useNavigate, useLocation } from "react-router-dom";
import {
  AppBar,
  Box,
  Toolbar,
  Typography,
  IconButton,
  Badge,
  Menu,
  MenuItem,
  CssBaseline,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Paper,
  Divider,
  Snackbar,
  Alert,
} from "@mui/material";
import {
  Notifications as NotificationsIcon,
  ChevronLeft as ChevronLeftIcon,
} from "@mui/icons-material";
import { Avatar } from "@mui/material";
import { useNotification } from "../contexts/NotificationContext";
import { getModuleApplications } from "../../shell/ModuleRegistry";
import { ApplicationData } from "../types/application";
import DashboardIcon from "@mui/icons-material/Dashboard";

// All applications come from ModuleRegistry (single source of truth)
const APPLICATIONS: ApplicationData[] = getModuleApplications();

// Add Home as a special case
const HOME_APPLICATION: ApplicationData = {
  id: 0,
  name: "Home",
  description: "Applications dashboard and navigation hub",
  icon: React.createElement(DashboardIcon),
  category: "core",
  path: "/home",
};

// Helper function to get page title from path
const getPageTitleFromPath = (pathname: string): string => {
  // Handle root path
  if (pathname === "/") {
    return HOME_APPLICATION.name;
  }

  // Handle home path specifically - always return HOME_APPLICATION name
  if (pathname === "/home") {
    return HOME_APPLICATION.name;
  }

  // Find matching application
  const app = APPLICATIONS.find((app) => app.path === pathname);
  if (app) {
    return app.name;
  }

  // Default fallback
  return "Home";
};

// Helper function to get main navigation items (subset of applications for sidebar navigation)
const getMainNavigationItems = (): ApplicationData[] => {
  const navItems = [];

  // Add home first
  navItems.push(HOME_APPLICATION);

  // Add applications that are marked to show in navigation bar
  const navigationApps = APPLICATIONS.filter(
    (app) => app.showInNavigationBar === true,
  );
  navItems.push(...navigationApps);

  return navItems;
};

// Create context for favorited applications
export const FavoritedAppsContext = createContext<{
  favoritedApps: ApplicationData[];
  addFavoritedApp: (app: ApplicationData) => void;
  removeFavoritedApp: (appId: number) => void;
}>({
  favoritedApps: [],
  addFavoritedApp: () => {},
  removeFavoritedApp: () => {},
});

// Create a provider component for favorited apps
export const FavoritedAppsProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [favoritedApps, setFavoritedApps] = useState<ApplicationData[]>([]);

  const addFavoritedApp = (app: ApplicationData) => {
    setFavoritedApps((prevApps) => {
      // Check if app is already in favorites
      if (prevApps.some((a) => a.id === app.id)) {
        return prevApps;
      }
      return [...prevApps, app];
    });
  };

  const removeFavoritedApp = (appId: number) => {
    setFavoritedApps((prevApps) => prevApps.filter((app) => app.id !== appId));
  };

  return (
    <FavoritedAppsContext.Provider
      value={{ favoritedApps, addFavoritedApp, removeFavoritedApp }}
    >
      {children}
    </FavoritedAppsContext.Provider>
  );
};

// Custom hook to use favorited apps context
export const useFavoritedApps = () => useContext(FavoritedAppsContext);

interface MainLayout2Props {
  children: React.ReactNode;
}

const drawerWidth = 222;

const MainLayout: React.FC<MainLayout2Props> = ({ children }) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const { favoritedApps } = useFavoritedApps();
  const navigate = useNavigate();
  const location = useLocation();

  // Get current user from context
  const { currentUser } = useUser();
  const { notifications, unreadCount, markAllAsRead, clearNotifications } =
    useNotification();
  const [notifAnchorEl, setNotifAnchorEl] = useState<null | HTMLElement>(null);
  const isNotifOpen = Boolean(notifAnchorEl);

  // State for managing toast notifications
  const [toastNotifications, setToastNotifications] = useState<
    Array<{
      id: string;
      message: string;
      type: "info" | "success" | "warning" | "error";
    }>
  >([]);

  const handleNotifOpen = (event: React.MouseEvent<HTMLElement>) => {
    setNotifAnchorEl(event.currentTarget);
    markAllAsRead();
  };
  const handleNotifClose = () => {
    setNotifAnchorEl(null);
  };

  // Handle removing toast notifications
  const removeToastNotification = (id: string) => {
    setToastNotifications((prev) => prev.filter((n) => n.id !== id));
  };

  // Listen for new notifications and show them as toasts
  useEffect(() => {
    if (notifications.length > 0) {
      const latestNotification = notifications[0];
      // Show as toast if it's unread and contains "completed successfully"
      if (
        !latestNotification.read &&
        latestNotification.message.includes("completed successfully")
      ) {
        const toastId = `toast-${latestNotification.id}`;
        setToastNotifications((prev) => {
          // Don't add duplicate toasts
          if (prev.some((t) => t.id === toastId)) return prev;
          return [
            {
              id: toastId,
              message: latestNotification.message,
              type: latestNotification.type || "success",
            },
            ...prev,
          ];
        });

        // Auto-remove toast after 4 seconds
        setTimeout(() => {
          removeToastNotification(toastId);
        }, 4000);
      }
    }
  }, [notifications]);

  // Instrument detail page: show instrument name and status in header
  let currentPageTitle = getPageTitleFromPath(location.pathname);
  const instrumentDetailMatch = location.pathname.match(
    /^\/instruments\/(.+)$/,
  );
  let instrumentStatus: string | undefined = undefined;
  if (instrumentDetailMatch) {
    const instrumentId = instrumentDetailMatch[1];
    // Try to get instrument name and status from a global state or context if available
    // For now, try to get from localStorage or window (replace with your global state if available)
    let instrumentName = instrumentId;
    let status = undefined;
    try {
      // If you have a global instrument list in context, use it here
      interface Instrument {
        id: string;
        name?: string;
        status?: string;
      }
      interface WindowWithInstruments extends Window {
        instruments?: Instrument[];
      }
      const instruments: Instrument[] =
        (window as WindowWithInstruments).instruments || [];
      const found = instruments.find((inst) => inst.id === instrumentId);
      if (found) {
        instrumentName = found.name || instrumentId;
        status = found.status;
      }
    } catch (error) {
      console.warn("Failed to retrieve instrument info:", error);
    }
    instrumentStatus = status;
    currentPageTitle =
      `Instrument: ${instrumentName}` +
      (instrumentStatus
        ? ` (${instrumentStatus.charAt(0).toUpperCase() + instrumentStatus.slice(1)})`
        : "");
  }

  const navigationItems = getMainNavigationItems();

  // Add current page to navigation if it's not already there (for non-pinned items)
  const getCurrentPageNavItems = () => {
    const baseNavItems = [...navigationItems];

    // Check if current page is already in navigation (exact or as a detail page for a permanent nav item)
    const currentPageInNav = baseNavItems.some((item) => {
      if (!item.path) return false;
      // Exact match
      if (location.pathname === item.path) return true;
      // Detail page: only match parent if it is in the permanent navigation
      if (item.path !== "/" && location.pathname.startsWith(item.path + "/"))
        return true;
      return false;
    });

    // If not in navigation, try to find it in all applications and add it temporarily
    if (!currentPageInNav) {
      // For detail pages, try to find the parent app by matching the base path
      const parentApp = APPLICATIONS.find((app) => {
        if (!app.path || app.path === "/") return false;
        return location.pathname.startsWith(app.path + "/");
      });
      if (parentApp) {
        baseNavItems.push(parentApp);
      } else {
        // Otherwise, try to match the exact path
        const currentApp = APPLICATIONS.find(
          (app) => app.path === location.pathname,
        );
        if (currentApp) {
          baseNavItems.push(currentApp);
        }
      }
    }

    return baseNavItems;
  };

  const currentNavItems = getCurrentPageNavItems();

  // Get selected index based on current route and navigation state
  const getSelectedIndex = (
    pathname: string,
    locationState?: { highlightNav?: string },
  ) => {
    // Check if there's a specific nav highlight request from state
    if (locationState?.highlightNav) {
      // First, try to find by exact path match in current navigation items
      const exactMatchIndex = currentNavItems.findIndex((item) => {
        const pathSegment = item.path?.replace("/", "") || "";
        return pathSegment === locationState.highlightNav;
      });
      if (exactMatchIndex >= 0) return exactMatchIndex;
    }

    // Highlight parent nav item for detail pages (e.g., /instruments/123 highlights /instruments)
    const index = currentNavItems.findIndex((item) => {
      if (!item.path) return false;
      if (item.path === pathname || (pathname === "/" && item.path === "/home"))
        return true;
      // If pathname starts with nav item path and nav item is not just '/'
      if (item.path !== "/" && pathname.startsWith(item.path + "/"))
        return true;
      return false;
    });
    return index >= 0 ? index : 0;
  };

  const selectedIndex = getSelectedIndex(location.pathname, location.state);

  const handleSidebarToggle = () => {
    setSidebarOpen(!sidebarOpen);
  };

  const handleProfileMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    handleMenuClose();
    navigate("/login");
  };

  const isMenuOpen = Boolean(anchorEl);

  const renderMenu = (
    <Menu
      anchorEl={anchorEl}
      anchorOrigin={{ vertical: "top", horizontal: "right" }}
      id="primary-search-account-menu"
      keepMounted
      transformOrigin={{ vertical: "top", horizontal: "right" }}
      open={isMenuOpen}
      onClose={handleMenuClose}
    >
      {currentUser && [
        <MenuItem
          key="profile-info"
          onClick={handleMenuClose}
          sx={{
            py: 2,
            flexDirection: "column",
            alignItems: "flex-start",
            "&:hover": {
              backgroundColor: "action.hover",
            },
          }}
        >
          <Typography
            variant="body2"
            color="text.primary"
            sx={{ fontWeight: 600, lineHeight: 1.2 }}
          >
            {currentUser.username}
          </Typography>
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{ lineHeight: 1.2 }}
          >
            {currentUser.email}
          </Typography>
        </MenuItem>,
        <Divider key="divider" />,
        <MenuItem
          key="profile-settings"
          onClick={() => {
            handleMenuClose(); /* TODO: navigate to profile settings */
          }}
        >
          <Typography variant="body2">Profile Settings</Typography>
        </MenuItem>,
        <MenuItem key="logout" onClick={handleLogout}>
          <Typography variant="body2" color="error">
            Logout
          </Typography>
        </MenuItem>,
      ]}
    </Menu>
  );

  return (
    <>
      <CssBaseline />
      {/* Top AppBar */}
      <AppBar
        position="fixed"
        sx={{
          width: "100%",
          zIndex: (theme) => theme.zIndex.drawer + 1,
          backgroundColor: "#FFFFFF", // White AppBar
          borderBottom: "1px solid #E0E0E0",
          boxShadow: "0 1px 3px rgba(0,0,0,0.1)",
        }}
      >
        <Toolbar>
          {/* Logo */}
          <Box
            sx={{
              mr: 2,
              cursor: "pointer",
              "&:hover": {
                opacity: 0.8,
              },
            }}
            onClick={() => navigate("/home")}
          >
            <svg
              width="32"
              height="32"
              viewBox="0 0 32 32"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path d="M32 0H0V32H32V0Z" fill="#EE3235"></path>
              <g opacity="0.53" style={{ mixBlendMode: "multiply" }}>
                <path
                  d="M28.6499 3.3501H3.3501V28.6498H28.6499V3.3501Z"
                  fill="#231F20"
                ></path>
              </g>
              <g filter="url(#filter0_d_17216_2910)">
                <path
                  d="M17.4301 8.30032C15.518 8.30032 14.3111 9.88649 14.3111 11.4193C14.3111 12.9522 13.6533 14.6687 12.4859 15.8341C11.3284 16.9917 9.65332 17.5092 8.32789 17.5092C5.82517 17.5092 5.04492 19.4154 5.04492 20.6282C5.04492 22.2835 6.41579 23.6524 8.12443 23.6524C9.83307 23.6524 11.1901 22.2756 11.1901 20.4919C11.1901 18.9117 11.8459 17.2564 12.9876 16.1166C14.1432 14.965 15.6326 14.4692 17.1318 14.4692C20 14.4692 21.1476 16.7961 21.1476 18.3349C21.1476 19.4272 20.7941 19.6643 20.7941 20.6144C20.7941 22.4119 22.3131 23.6939 23.8736 23.6939C25.6988 23.6939 26.9531 22.1611 26.9531 20.6144C26.9531 18.8307 25.5348 17.5013 23.8933 17.5013C22.2518 17.5013 20.156 16.1048 20.156 13.6159C20.156 12.5235 20.482 12.3339 20.482 11.4055C20.482 9.52698 18.9432 8.29834 17.4301 8.29834"
                  fill="#EE3235"
                ></path>
              </g>
              <defs>
                <filter
                  id="filter0_d_17216_2910"
                  x="1.04492"
                  y="4.29834"
                  width="29.9082"
                  height="23.3955"
                  filterUnits="userSpaceOnUse"
                  colorInterpolationFilters="sRGB"
                >
                  <feFlood
                    floodOpacity="0"
                    result="BackgroundImageFix"
                  ></feFlood>
                  <feColorMatrix
                    in="SourceAlpha"
                    type="matrix"
                    values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
                    result="hardAlpha"
                  ></feColorMatrix>
                  <feOffset></feOffset>
                  <feGaussianBlur stdDeviation="2"></feGaussianBlur>
                  <feComposite in2="hardAlpha" operator="out"></feComposite>
                  <feColorMatrix
                    type="matrix"
                    values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.5 0"
                  ></feColorMatrix>
                  <feBlend
                    mode="normal"
                    in2="BackgroundImageFix"
                    result="effect1_dropShadow_17216_2910"
                  ></feBlend>
                  <feBlend
                    mode="normal"
                    in="SourceGraphic"
                    in2="effect1_dropShadow_17216_2910"
                    result="shape"
                  ></feBlend>
                </filter>
              </defs>
            </svg>
          </Box>

          {/* Brand section */}
          <Box
            sx={{
              display: "flex",
              flexDirection: "column",
              mr: 2,
              alignItems: "flex-start",
              cursor: "pointer",
              "&:hover": {
                opacity: 0.8,
              },
            }}
            onClick={() => navigate("/home")}
          >
            <Typography
              variant="body1"
              noWrap
              component="div"
              sx={{
                color: "#333",
                fontWeight: 700,
                lineHeight: 1.1,
                fontSize: "16px",
              }}
            >
              Thermo Scientific
            </Typography>
            <Typography
              variant="body2"
              noWrap
              component="div"
              sx={{
                color: "#333",
                fontWeight: 600,
                lineHeight: 1.1,
                fontSize: "14px",
                alignSelf: "center",
              }}
            >
              OPAL
            </Typography>
          </Box>

          {/* Page Indicator */}
          <Typography
            variant="body1"
            noWrap
            component="div"
            sx={{
              color: "#666",
              fontWeight: 400,
              fontSize: "16px",
              mr: 2,
              display: "flex",
              alignItems: "center",
              "&::before": {
                content: '"|"',
                marginRight: "8px",
                color: "#ccc",
              },
            }}
          >
            {currentPageTitle}
          </Typography>

          {/* Spacer */}
          <Box sx={{ flexGrow: 1 }} />

          {/* Notification Icon */}
          <IconButton
            size="large"
            aria-label="show notifications"
            sx={{ mr: 2, color: "#666" }}
            onClick={handleNotifOpen}
          >
            <Badge badgeContent={unreadCount} color="error">
              <NotificationsIcon />
            </Badge>
          </IconButton>
          <Menu
            anchorEl={notifAnchorEl}
            open={isNotifOpen}
            onClose={handleNotifClose}
            anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
            transformOrigin={{ vertical: "top", horizontal: "right" }}
            PaperProps={{ sx: { minWidth: 320, maxHeight: 400 } }}
          >
            <Box sx={{ px: 2, py: 1, fontWeight: 600 }}>Notifications</Box>
            <Divider />
            {notifications.length === 0 ? (
              <MenuItem disabled>No notifications</MenuItem>
            ) : (
              notifications.map((n) => (
                <MenuItem
                  key={n.id}
                  sx={{
                    whiteSpace: "normal",
                    alignItems: "flex-start",
                    flexDirection: "column",
                  }}
                >
                  <Typography variant="body2" color="text.primary">
                    {n.message}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {new Date(n.timestamp).toLocaleString()}
                  </Typography>
                </MenuItem>
              ))
            )}
            {notifications.length > 0 && [
              <Divider key="divider" />,
              <MenuItem
                key="clear-all"
                onClick={() => {
                  clearNotifications();
                  handleNotifClose();
                }}
              >
                <Typography variant="body2" color="error">
                  Clear All
                </Typography>
              </MenuItem>,
            ]}
          </Menu>

          {/* Profile Initials Avatar */}
          <IconButton
            size="large"
            edge="end"
            aria-label="account of current user"
            aria-controls="primary-search-account-menu"
            aria-haspopup="true"
            onClick={handleProfileMenuOpen}
            sx={{ color: "#666" }}
          >
            {currentUser && (
              <Avatar
                sx={{
                  width: 32,
                  height: 32,
                  bgcolor: "#1976d2",
                  fontWeight: 700,
                  fontSize: 16,
                }}
              >
                {currentUser.username?.substring(0, 2).toUpperCase()}
              </Avatar>
            )}
          </IconButton>
        </Toolbar>
      </AppBar>
      <Toolbar /> {/* Spacer for the fixed AppBar */}
      {/* Side Navigation */}
      <AppBar
        position="fixed"
        color="default"
        sx={{
          width: sidebarOpen ? drawerWidth : 64,
          height: "calc(100vh - 64px)",
          top: 64, // Below the top AppBar
          left: 0,
          backgroundColor: "#FFFFFF", // White sidebar
          borderRight: "1px solid #E0E0E0",
          boxShadow: "none",
          transition: "width 225ms cubic-bezier(0.4, 0, 0.6, 1) 0ms",
          overflow: "hidden",
          zIndex: (theme) => theme.zIndex.drawer,
        }}
      >
        <Box
          sx={{
            height: "100%",
            pt: 2,
            display: "flex",
            flexDirection: "column",
          }}
        >
          <List sx={{ flexGrow: 1 }}>
            {/* Default navigation items */}
            {currentNavItems.map((item, index) => (
              <ListItem key={`nav-default-${item.id}`} disablePadding>
                <ListItemButton
                  selected={selectedIndex === index}
                  onClick={() => {
                    if (item.path) {
                      navigate(item.path);
                    }
                  }}
                  sx={{
                    minHeight: 56,
                    px: sidebarOpen ? 3 : 2,
                    py: 2,
                    justifyContent: sidebarOpen ? "initial" : "center",
                    "&.Mui-selected": {
                      backgroundColor: "rgba(25, 118, 210, 0.12)",
                      "&:hover": {
                        backgroundColor: "rgba(25, 118, 210, 0.2)",
                      },
                    },
                    "&:hover": {
                      backgroundColor: "rgba(0, 0, 0, 0.04)",
                    },
                  }}
                >
                  <ListItemIcon
                    sx={{
                      color:
                        selectedIndex === index
                          ? "#1976d2"
                          : "rgba(0, 0, 0, 0.6)",
                      minWidth: sidebarOpen ? 40 : 0,
                      mr: sidebarOpen ? 2 : "auto",
                      justifyContent: "center",
                    }}
                  >
                    {item.icon}
                  </ListItemIcon>
                  <ListItemText
                    primary={item.name}
                    sx={{
                      opacity: sidebarOpen ? 1 : 0,
                      "& .MuiListItemText-primary": {
                        fontSize: "0.875rem",
                        fontWeight: 500,
                        color: "rgba(0, 0, 0, 0.87)",
                      },
                    }}
                  />
                </ListItemButton>
              </ListItem>
            ))}

            {/* Favorited apps that are NOT already in default navigation */}
            {favoritedApps
              .filter(
                (favApp) =>
                  !currentNavItems.some((navItem) => navItem.id === favApp.id),
              )
              .map((item) => (
                <ListItem key={`nav-favorited-${item.id}`} disablePadding>
                  <ListItemButton
                    onClick={() => {
                      if (item.path) {
                        navigate(item.path);
                      }
                    }}
                    sx={{
                      minHeight: 48,
                      px: sidebarOpen ? 3 : 2,
                      py: 1.5,
                      justifyContent: sidebarOpen ? "initial" : "center",
                      "&:hover": {
                        backgroundColor: "rgba(0, 0, 0, 0.04)",
                      },
                    }}
                  >
                    <ListItemIcon
                      sx={{
                        color: "rgba(0, 0, 0, 0.6)",
                        minWidth: sidebarOpen ? 40 : 0,
                        mr: sidebarOpen ? 2 : "auto",
                        justifyContent: "center",
                      }}
                    >
                      {item.icon}
                    </ListItemIcon>
                    <ListItemText
                      primary={item.name}
                      sx={{
                        opacity: sidebarOpen ? 1 : 0,
                        "& .MuiListItemText-primary": {
                          fontSize: "0.875rem",
                          fontWeight: 400,
                          color: "rgba(0, 0, 0, 0.87)",
                        },
                      }}
                    />
                  </ListItemButton>
                </ListItem>
              ))}
          </List>{" "}
          {/* Toggle Button at Bottom Right */}
          <Box
            sx={{
              display: "flex",
              justifyContent: sidebarOpen ? "flex-end" : "right",
              px: sidebarOpen ? -2 : -2,
              pb: 1,
            }}
          >
            <IconButton
              onClick={handleSidebarToggle}
              size="small"
              sx={{
                color: "rgba(0, 0, 0, 0.6)",
                backgroundColor: "rgba(0, 0, 0, 0.04)",
                "&:hover": {
                  backgroundColor: "rgba(0, 0, 0, 0.08)",
                },
              }}
            >
              <ChevronLeftIcon
                sx={{
                  transform: sidebarOpen ? "rotate(0deg)" : "rotate(180deg)",
                  transition:
                    "transform 225ms cubic-bezier(0.4, 0, 0.6, 1) 0ms",
                }}
              />
            </IconButton>
          </Box>
        </Box>
      </AppBar>
      {/* Main Content Area */}
      <Paper
        component="main"
        sx={{
          position: "fixed",
          top: 64, // Below the top AppBar
          left: sidebarOpen ? drawerWidth : 64,
          width: `calc(100% - ${sidebarOpen ? drawerWidth : 64}px)`,
          height: "calc(100vh - 64px)",
          overflow: "auto",
          transition:
            "left 225ms cubic-bezier(0.4, 0, 0.6, 1) 0ms, width 225ms cubic-bezier(0.4, 0, 0.6, 1) 0ms",
          zIndex: 0,
          bgcolor: "#f5f5f5", // Light grey background to match MainLayout
          boxShadow: "none",
          borderRadius: 0,
          display: "grid",
        }}
      >
        {children}
      </Paper>
      {renderMenu}
      {/* Global Toast Notifications */}
      {toastNotifications.map((notification, index) => (
        <Snackbar
          key={notification.id}
          open={true}
          autoHideDuration={4000}
          onClose={() => removeToastNotification(notification.id)}
          anchorOrigin={{
            vertical: "bottom",
            horizontal: "right",
          }}
          sx={{
            // Stack multiple notifications vertically
            bottom: `${index * 70 + 16}px !important`,
          }}
        >
          <Alert
            onClose={() => removeToastNotification(notification.id)}
            severity={notification.type}
            sx={{
              width: "100%",
              minWidth: "300px",
            }}
          >
            {notification.message}
          </Alert>
        </Snackbar>
      ))}
    </>
  );
};

export default MainLayout;
