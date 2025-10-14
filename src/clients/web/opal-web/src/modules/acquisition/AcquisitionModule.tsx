import React from "react";
import { Routes, Route } from "react-router-dom";
import { Acquisition, AcquisitionList } from "./components";

/**
 * Acquisition Module - handles all acquisition-related functionality
 */
export const AcquisitionModule: React.FC = () => {
  return (
    <Routes>
      <Route path="/" element={<Acquisition />} />
      <Route path="/list" element={<AcquisitionList />} />
    </Routes>
  );
};
