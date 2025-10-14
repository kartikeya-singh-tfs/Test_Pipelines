import React from "react";
import { Routes, Route } from "react-router-dom";
import { Instruments, InstrumentDetail } from "./components";

/**
 * Instruments Module - handles all instrument-related functionality
 */
export const InstrumentsModule: React.FC = () => {
  return (
    <Routes>
      <Route path="/" element={<Instruments />} />
      <Route path="/:id" element={<InstrumentDetail />} />
    </Routes>
  );
};
