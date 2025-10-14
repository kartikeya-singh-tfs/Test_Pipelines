// API Base URL
export const API_BASE_URL = "https://localhost:61350/api";

// API Endpoints
export const API_ENDPOINTS = {
  // Acquisition endpoints
  ACQUISITION_SEQUENCE: `${API_BASE_URL}/acquisition/v1/sequence`,
  ACQUISITION_SSE: `${API_BASE_URL}/acquisition/v1/sse`,
  ACQUISITION_SPARKLINE_SSE: `${API_BASE_URL}/acquisition/v1/sseSparklineData`,
  ACQUISITION_CHROMATOGRAM_SSE: `${API_BASE_URL}/acquisition/v1/sseChromatogramSvgData`,
} as const;
