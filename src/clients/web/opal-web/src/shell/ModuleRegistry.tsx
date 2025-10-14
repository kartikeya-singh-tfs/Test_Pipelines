import React from "react";
import { AcquisitionModule } from "../modules/acquisition";
import { InstrumentsModule } from "../modules/instruments";
import MemoryIcon from "@mui/icons-material/Memory";
import BiotechOutlinedIcon from "@mui/icons-material/BiotechOutlined";
import QueueIcon from "@mui/icons-material/Queue";
import SettingsOutlinedIcon from "@mui/icons-material/SettingsOutlined";
import VpnKeyOutlinedIcon from "@mui/icons-material/VpnKeyOutlined";
import ManageAccountsOutlinedIcon from "@mui/icons-material/ManageAccountsOutlined";
import AnalyticsOutlinedIcon from "@mui/icons-material/AnalyticsOutlined";
import { ApplicationData } from "../shared/types/application";

interface ModuleRegistration {
  path: string;
  component: React.ComponentType;
  application?: ApplicationData;
}

// Module registrations with both routes and application cards
const moduleRegistrations: ModuleRegistration[] = [
  {
    path: "/instruments/*",
    component: InstrumentsModule,
    application: {
      id: 1,
      name: "Instruments",
      description:
        "Manage, schedule, and monitor data acquisition of all instruments connected to the Ardia platform.",
      icon: React.createElement(MemoryIcon),
      category: "core",
      path: "/instruments",
      showInNavigationBar: false,
    },
  },
  {
    path: "/acquisition/*",
    component: AcquisitionModule,
    application: {
      id: 2,
      name: "Acquisition",
      description: "Prepare, review, and submit your acquisitions.",
      icon: React.createElement(BiotechOutlinedIcon),
      category: "core",
      path: "/acquisition",
    },
  },
  // Note: Acquisition Manager is a sub-route of acquisition module
  // but gets its own application card for discovery
  {
    path: "", // No separate route needed - handled by acquisition module
    component: () => null, // No component needed
    application: {
      id: 3,
      name: "Acquisition Manager",
      description:
        "Create, manage, and monitor multiple analytical acquisitions with real-time acquisition tracking.",
      icon: React.createElement(QueueIcon),
      category: "core",
      path: "/acquisition/list",
      showInNavigationBar: true,
    },
  },

  // Quan application (quantitative analysis)
  {
    path: "", // No route - not implemented yet
    component: () => null,
    application: {
      id: 4,
      name: "Quan",
      description:
        "Quantitative analysis and data processing for analytical results.",
      icon: React.createElement(AnalyticsOutlinedIcon),
      category: "core",
      path: "/quan",
    },
  },

  // Placeholder applications (not yet implemented)
  {
    path: "", // No route - not implemented yet
    component: () => null,
    application: {
      id: 5,
      name: "Global Settings",
      description:
        "Define administrative settings that are applied across the OPAL.",
      icon: React.createElement(SettingsOutlinedIcon),
      category: "utility",
      path: "/settings",
      showInNavigationBar: true,
    },
  },
  {
    path: "", // No route - not implemented yet
    component: () => null,
    application: {
      id: 6,
      name: "IdP Configuration",
      description:
        "Configure, edit, and remove the identity providers used by the OPAL.",
      icon: React.createElement(VpnKeyOutlinedIcon),
      category: "utility",
      path: "/idp-configuration",
    },
  },
  {
    path: "", // No route - not implemented yet
    component: () => null,
    application: {
      id: 7,
      name: "User Management",
      description:
        "Add new users and manage existing users in OPAL. Define roles, users, and configure permissions.",
      icon: React.createElement(ManageAccountsOutlinedIcon),
      category: "utility",
      path: "/user-management",
    },
  },
];

/**
 * Export application cards for use in components
 */
export const getModuleApplications = (): ApplicationData[] => {
  return moduleRegistrations
    .map((reg) => reg.application)
    .filter((app): app is ApplicationData => app !== undefined);
};

/**
 * Export module registrations for routing (without importing Home to avoid circular dependency)
 */
export const getModuleRegistrations = () => {
  return moduleRegistrations.filter((reg) => reg.path); // Only return entries with actual routes
};
