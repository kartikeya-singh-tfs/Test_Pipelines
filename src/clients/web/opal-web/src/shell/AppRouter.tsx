import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import { getModuleRegistrations } from "./ModuleRegistry";
import Home from "./Home";

/**
 * App router that handles all application routing
 * Imports from ModuleRegistry to get route configurations
 */
export const AppRouter: React.FC = () => {
  const moduleRegistrations = getModuleRegistrations();

  return (
    <Routes>
      <Route path="/home" element={<Home />} />
      {moduleRegistrations.map((reg) => (
        <Route
          key={reg.path}
          path={reg.path}
          element={React.createElement(reg.component)}
        />
      ))}
      <Route path="/" element={<Navigate to="/home" replace />} />
    </Routes>
  );
};
