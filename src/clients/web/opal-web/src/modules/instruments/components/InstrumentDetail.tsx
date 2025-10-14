import React, { useState, useEffect, useRef, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Box,
  Typography,
  Card,
  CardContent,
  Chip,
  Button,
  Divider,
  Tabs,
  Tab,
  useTheme,
  useMediaQuery,
  IconButton,
  Collapse,
} from "@mui/material";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import { SparkLineChart } from "@mui/x-charts/SparkLineChart";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import StopIcon from "@mui/icons-material/Stop";
import MemoryIcon from "@mui/icons-material/Memory";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import ExpandLessIcon from "@mui/icons-material/ExpandLess";
import InfoIcon from "@mui/icons-material/Info";
import PlaylistPlayIcon from "@mui/icons-material/PlaylistPlay";
import TimelineIcon from "@mui/icons-material/Timeline";
import MainLayout from "../../../shared/components/MainLayout";
import {
  useGlobalSSE,
  type ApiSequence,
  type SampleInfo,
} from "../../../shared/contexts/GlobalSSEContext";
import { Sample } from "../../../shared/models";
import { API_ENDPOINTS } from "../../../shared/constants/navigation-constants";
import { AcquisitionState, SequenceState } from "../../../shared/types/enums";

// Types for instrument data (same as Instruments.tsx)
interface InstrumentData {
  id: string;
  name: string;
  type: string;
  status: "online" | "offline" | "running" | "error" | "maintenance";
  lastSeen: string;
  currentMethod?: string;
  samplesCompleted?: number;
  totalSamples?: number;
  estimatedTimeRemaining?: string;
  ipAddress?: string;
  serialNumber?: string;
}

// Sample data for acquisition grid - use Sample type directly
type AcquisitionSample = Sample;

const InstrumentDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("md"));
  const isTablet = useMediaQuery(theme.breakpoints.between("md", "lg"));

  const {
    subscribeToChromatogramUpdates,
    subscribeToSequenceUpdates,
    subscribeToSampleUpdates,
    subscribeToSparklineUpdates,
    sequenceUpdates,
    sampleUpdates,
    sparklineUpdates,
    startConnection,
    // stopConnection, // Unused - commented out
  } = useGlobalSSE();

  const [instrument, setInstrument] = useState<InstrumentData | null>(null);
  const [acquisitionSamples, setAcquisitionSamples] = useState<Sample[]>([]);
  const [chromatogramUrl, setChromatogramUrl] = useState<string | null>(null);
  const [currentSequenceId, setCurrentSequenceId] = useState<string | null>(
    null,
  );
  const [currentSequence, setCurrentSequence] = useState<ApiSequence | null>(
    null,
  );
  const [currentSampleIndex, setCurrentSampleIndex] = useState<number>(0);

  // Mobile UI state
  const [activeTab, setActiveTab] = useState(0);
  const [infoExpanded, setInfoExpanded] = useState(!isMobile);

  const chromatogramUrlRef = useRef<string | null>(null);
  const sparklineSequenceIdRef = useRef<string | null>(null);

  // Helper function to get persistent sparkline storage key
  const getSparklineStorageKey = (sequenceId: string) =>
    `sparkline_data_${sequenceId}`;

  // Helper function to load persisted sparkline data
  const loadPersistedSparklineData = useCallback(
    (sequenceId: string): Record<string, number[]> => {
      try {
        const storageKey = getSparklineStorageKey(sequenceId);
        const stored = localStorage.getItem(storageKey);
        const parsed = stored ? JSON.parse(stored) : {};
        return parsed;
      } catch {
        return {};
      }
    },
    [],
  );

  // Helper function to save sparkline data
  const saveSparklineData = useCallback(
    (sequenceId: string, sampleId: string, data: number[]) => {
      try {
        // Only save data if we have a current sequence
        if (!currentSequenceId) {
          return;
        }

        // Check if entire sequence is complete (not just individual samples)
        // We want to save completed sample data as long as the sequence is still running
        const sequenceComplete =
          currentSequence?.sequenceStatus?.sequenceState ===
            SequenceState.SequenceComplete ||
          currentSequence?.sequenceStatus?.sequenceState ===
            SequenceState.SequenceCompletedFilesMoved ||
          currentSequence?.sequenceStatus?.sequenceState ===
            SequenceState.SequenceCompletedSomeFilesNotMoved;

        if (sequenceComplete) {
          return;
        }

        const storageKey = getSparklineStorageKey(sequenceId);
        const existing = loadPersistedSparklineData(sequenceId);
        existing[sampleId] = data;
        localStorage.setItem(storageKey, JSON.stringify(existing));
      } catch {
        // Error saving sparkline data
      }
    },
    [currentSequenceId, currentSequence, loadPersistedSparklineData],
  );

  // Helper function to clear sparkline data when sequence completes
  const clearSparklineData = useCallback((sequenceId: string) => {
    try {
      localStorage.removeItem(getSparklineStorageKey(sequenceId));
    } catch {
      // Error clearing sparkline data
    }
  }, []);

  useEffect(() => {
    // For now, use basic instrument info - in real app this would come from SSE device discovery
    // Create basic instrument info based on the ID
    const instrumentInfo: InstrumentData = {
      id: id || "unknown",
      name:
        id === "inst-000" ? "MS Simulator" : `Instrument ${id || "unknown"}`,
      type: id === "inst-000" ? "Simulator" : "Unknown",
      status: "online",
      lastSeen: "1 minute ago",
      ipAddress: "127.0.0.1",
      serialNumber: `SER-${id || "unknown"}`,
    };
    setInstrument(instrumentInfo);

    // Don't clear data on mount - preserve any existing acquisition state
    // Only clear if there's no current sequence data
    // This allows data to persist when navigating back to the page

    // Start SSE connection to get live data
    startConnection();

    // Remove stopConnection on unmount to avoid unnecessary disconnects
    // If you want to handle browser tab close or navigation, use a more robust SSE manager in GlobalSSEContext
  }, [id, startConnection]);

  // Fetch current sequence on mount and periodically
  useEffect(() => {
    // Immediately fetch current sequence to restore state on mount
    const fetchCurrentSequence = async () => {
      try {
        const response = await fetch(API_ENDPOINTS.ACQUISITION_SEQUENCE);
        if (response.ok) {
          const sequences = await response.json();
          // Find the currently running sequence - exclude completed states
          const runningSequence = sequences.find((seq: unknown) => {
            const sequence = seq as Record<string, unknown>;
            const sequenceStatus = sequence.sequenceStatus as
              | Record<string, unknown>
              | undefined;
            const sequenceState = sequenceStatus?.sequenceState;
            return (
              sequenceState !== undefined &&
              sequenceState !== SequenceState.SequenceComplete &&
              sequenceState !== SequenceState.SequenceCompletedFilesMoved &&
              sequenceState !== SequenceState.SequenceCompletedSomeFilesNotMoved
            );
          });

          if (runningSequence) {
            setCurrentSequence(runningSequence);
            setCurrentSequenceId(runningSequence.sequenceId);
          } else {
            setCurrentSequence(null);
            setAcquisitionSamples([]);
            setCurrentSampleIndex(0);
            setChromatogramUrl(null);
          }
        }
      } catch {
        setCurrentSequence(null);
        setAcquisitionSamples([]);
        setCurrentSampleIndex(0);
        setChromatogramUrl(null);
      }
    };

    fetchCurrentSequence();
    const interval = setInterval(fetchCurrentSequence, 5000);
    return () => clearInterval(interval);
  }, []);

  // Initialize sequence ID from existing data on mount - this helps restore sparkline subscriptions
  useEffect(() => {
    if (currentSequence && currentSequence.sequenceId && !currentSequenceId) {
      setCurrentSequenceId(currentSequence.sequenceId);
      sparklineSequenceIdRef.current = currentSequence.sequenceId;
    }
  }, [currentSequence, currentSequenceId]); // Run when currentSequence or currentSequenceId changes

  // Ref to track if we're currently updating sparkline data to prevent race conditions
  const isUpdatingSparklineRef = useRef(false);

  // Track SSE connection status
  useEffect(() => {
    // Listen for SSE connection status changes if available in GlobalSSEContext
    if (typeof window !== "undefined") {
      // Online/offline event handlers
      // Note: These could be used to update UI connectivity status if needed
      // Remove connection is handled elsewhere
    }
  }, []);

  // Build acquisition samples from SSE data
  useEffect(() => {
    // Only show acquisition samples if we have a current sequence from the API
    // Don't create dummy data from SSE updates
    if (!currentSequence || !currentSequence.samples) {
      // Only clear acquisitionSamples if the sequence is complete
      const sequenceComplete =
        currentSequence?.sequenceStatus?.sequenceState ===
          SequenceState.SequenceComplete ||
        currentSequence?.sequenceStatus?.sequenceState ===
          SequenceState.SequenceCompletedFilesMoved ||
        currentSequence?.sequenceStatus?.sequenceState ===
          SequenceState.SequenceCompletedSomeFilesNotMoved;
      if (!sequenceComplete) return;
      setAcquisitionSamples([]);
      setChromatogramUrl(null); // Clear chromatogram if no sequence
      return;
    }

    // Preserve previous sparklineData for each sample unless new data arrives
    setAcquisitionSamples((prevSamples) => {
      const samples: AcquisitionSample[] = (currentSequence.samples ?? []).map(
        (sample: SampleInfo, index: number) => {
          const backendSampleId = sample.sampleData.id || `sample-${index + 1}`;
          // Try to find previous sample by backendSampleId
          const prevSample = prevSamples.find(
            (s) =>
              s.backendSampleId &&
              String(s.backendSampleId).toLowerCase() ===
                String(backendSampleId).toLowerCase(),
          );

          // Load persisted sparkline data for completed samples
          const persistedData = currentSequenceId
            ? loadPersistedSparklineData(currentSequenceId)
            : {};
          const persistedSparklineData =
            persistedData[String(backendSampleId)] || [];

          return {
            id: index + 1,
            sampleType:
              (sample.sampleData.type as
                | "Standard"
                | "Unknown"
                | "QC"
                | "Blank"
                | "") || "Standard",
            rawFileName:
              sample.sampleData.rawFilePath || `${sample.sampleData.name}.raw`,
            sampleName: sample.sampleData.name || `Sample ${index + 1}`,
            instMethod: sample.sampleData.methodFilePath || "Unknown Method",
            position: sample.sampleData.position || `A:${index + 1}`,
            injVol: sample.sampleData.volume || 0,
            status:
              getStatusFromSequenceState(
                sample.sampleStatus.acquisitionState,
              ) || "Pending",
            // Prioritize persisted data over previous state data (which might be empty on page load)
            sparklineData:
              persistedSparklineData.length > 0
                ? persistedSparklineData
                : prevSample?.sparklineData || [],
            backendSampleId: backendSampleId,
          };
        },
      ); // Add fallback for undefined samples

      // Find current sample index
      const runningSampleIndex = samples.findIndex(
        (s) => s.status === "Running",
      );
      setCurrentSampleIndex(runningSampleIndex >= 0 ? runningSampleIndex : 0);
      return samples;
    });
  }, [currentSequenceId, currentSequence, loadPersistedSparklineData]); // Add missing dependency

  // Track current active sequence from SSE data
  useEffect(() => {
    // Look for the most recent sequence update to determine current active sequence
    if (sequenceUpdates.length > 0) {
      const latestSequenceUpdate = sequenceUpdates[sequenceUpdates.length - 1];
      const sequenceId = latestSequenceUpdate.Data?.SequenceId;

      if (sequenceId && sequenceId !== "00000000-0000-0000-0000-000000000000") {
        setCurrentSequenceId(sequenceId);
        sparklineSequenceIdRef.current = sequenceId; // Also set in ref for persistence
      }
    } else if (
      currentSequence &&
      currentSequence.sequenceId &&
      !currentSequenceId
    ) {
      // If we have a sequence from API but no sequence ID from SSE, use the API sequence ID
      setCurrentSequenceId(currentSequence.sequenceId);
      sparklineSequenceIdRef.current = currentSequence.sequenceId; // Also set in ref for persistence
    }
    // Don't clear currentSequenceId when currentSequence becomes false - keep sparkline subscription alive
  }, [sequenceUpdates, currentSequence, currentSequenceId]); // Add missing dependency

  // Debug sparkline updates
  useEffect(() => {
    if (sparklineUpdates.length > 0) {
      // Sparkline updates are being received
    }
  }, [sparklineUpdates, subscribeToChromatogramUpdates]); // Add missing dependency

  useEffect(() => {
    // Subscribe to chromatogram updates
    const unsubscribe = subscribeToChromatogramUpdates((message) => {
      if (message.data && message.data.chromatogramSvg) {
        const svgContent = message.data.chromatogramSvg;

        // Clean up previous URL to prevent memory leaks
        if (chromatogramUrlRef.current) {
          URL.revokeObjectURL(chromatogramUrlRef.current);
        }

        // Create blob URL for the SVG
        const blob = new Blob([svgContent], { type: "image/svg+xml" });
        const url = URL.createObjectURL(blob);

        chromatogramUrlRef.current = url;
        setChromatogramUrl(url);
      }
    });

    return () => {
      unsubscribe();
      // Clean up blob URL on unmount
      if (chromatogramUrlRef.current) {
        URL.revokeObjectURL(chromatogramUrlRef.current);
      }
    };
  }, [subscribeToChromatogramUpdates]); // Add missing dependency

  // Check for active acquisitions from existing SSE data
  useEffect(() => {
    // Check if any samples are currently running or queued
    const hasRunningSamples = acquisitionSamples.some(
      (sample) => sample.status === "Running" || sample.status === "Queued",
    );

    // Check if sequence is marked as complete using enum values
    const sequenceComplete =
      currentSequence?.sequenceStatus?.sequenceState ===
        SequenceState.SequenceComplete ||
      currentSequence?.sequenceStatus?.sequenceState ===
        SequenceState.SequenceCompletedFilesMoved ||
      currentSequence?.sequenceStatus?.sequenceState ===
        SequenceState.SequenceCompletedSomeFilesNotMoved;

    // An acquisition is active if there are running samples and sequence is not complete
    const isActive = hasRunningSamples && !sequenceComplete;
    // Note: Active acquisition state could be tracked here if needed for UI

    // Update instrument status based on acquisition activity
    if (instrument && isActive && instrument.status !== "running") {
      setInstrument((prev) => (prev ? { ...prev, status: "running" } : null));
    } else if (instrument && !isActive && instrument.status === "running") {
      // If acquisition is complete, set status back to online
      setInstrument((prev) => (prev ? { ...prev, status: "online" } : null));
    }

    // Only clear acquisition data when sequence is explicitly complete (not when all samples are done)
    if (sequenceComplete && currentSequence) {
      // Immediately clear persisted sparkline data when sequence completes
      if (currentSequenceId) {
        clearSparklineData(currentSequenceId);
      }

      setTimeout(() => {
        setCurrentSequence(null);
        // Keep currentSequenceId to maintain sparkline subscription until explicitly cleared
        // setCurrentSequenceId(null);
        setAcquisitionSamples([]);
        setCurrentSampleIndex(0);
        setChromatogramUrl(null);
      }, 1000); // Wait 1 second to show completion state briefly
    }
  }, [
    sequenceUpdates,
    sampleUpdates,
    acquisitionSamples,
    instrument,
    currentSequence,
    clearSparklineData,
    currentSequenceId,
  ]);

  // Subscribe to sequence updates from GlobalSSE
  useEffect(() => {
    const unsubscribeSequence = subscribeToSequenceUpdates((message) => {
      const { SequenceId, SampleId, SequenceState, IsError } = message.Data;

      // Only process updates for the current sequence (if we have one)
      if (currentSequenceId && SequenceId !== currentSequenceId) return;

      // Map SequenceState to our status values (these come as string values from SSE)
      const getStatusFromSSESequenceState = (
        state: string,
        isError: boolean,
      ): Sample["status"] | null => {
        if (isError) return "Error";

        switch (state) {
          case "SampleStart":
          case "DataFileCreate":
            return "Running";
          case "SampleComplete":
            return "Complete";
          default:
            return null;
        }
      };

      const newStatus = getStatusFromSSESequenceState(SequenceState, IsError);

      if (
        newStatus &&
        SampleId &&
        SampleId !== "00000000-0000-0000-0000-000000000000"
      ) {
        setAcquisitionSamples((prevSamples) => {
          const updatedSamples = prevSamples.map((sample) => {
            if (sample.backendSampleId === SampleId) {
              return {
                ...sample,
                status: newStatus,
                // Preserve existing sparkline data when updating status
                sparklineData: sample.sparklineData || [],
              };
            }
            return sample;
          });

          // Update current sample index if a sample becomes running
          if (newStatus === "Running") {
            const runningSampleIndex = updatedSamples.findIndex(
              (s) => s.backendSampleId === SampleId,
            );
            if (runningSampleIndex >= 0) {
              setCurrentSampleIndex(runningSampleIndex);
            }
          }

          return updatedSamples;
        });
      }
    });

    return () => {
      unsubscribeSequence();
    };
  }, [currentSequenceId, subscribeToSequenceUpdates]); // Add missing dependency

  // Subscribe to sample updates from GlobalSSE
  useEffect(() => {
    const unsubscribeSample = subscribeToSampleUpdates((message) => {
      const { SequenceId, SampleId, AcquisitionState } = message.Data;

      // Only process updates for the current sequence (if we have one)
      if (currentSequenceId && SequenceId !== currentSequenceId) return;

      // Map AcquisitionState to our status values (these come as string values from SSE)
      const getStatusFromAcquisitionState = (
        state: string,
      ): Sample["status"] | null => {
        switch (state) {
          case "ReadBarcode":
          case "SendMethod":
          case "WaitMethodReady":
          case "WaitStartSlaves":
          case "WaitStartMaster":
          case "WaitContactClosure":
          case "Acquire":
            return "Running";
          case "PostRun":
            return "Complete";
          case "Ready":
            return "Pending";
          default:
            return null;
        }
      };

      const newStatus = getStatusFromAcquisitionState(AcquisitionState);

      if (
        newStatus &&
        SampleId &&
        SampleId !== "00000000-0000-0000-0000-000000000000"
      ) {
        setAcquisitionSamples((prevSamples) => {
          const updatedSamples = prevSamples.map((sample) => {
            if (sample.backendSampleId === SampleId) {
              return {
                ...sample,
                status: newStatus,
                // Preserve existing sparkline data when updating status
                sparklineData: sample.sparklineData || [],
              };
            }
            return sample;
          });

          // Update current sample index if a sample becomes running
          if (newStatus === "Running") {
            const runningSampleIndex = updatedSamples.findIndex(
              (s) => s.backendSampleId === SampleId,
            );
            if (runningSampleIndex >= 0) {
              setCurrentSampleIndex(runningSampleIndex);
            }
          }

          return updatedSamples;
        });
      }
    });

    return () => {
      unsubscribeSample();
    };
  }, [currentSequenceId, subscribeToSampleUpdates]); // Add missing dependency

  // Subscribe to sparkline updates from GlobalSSE - persistent subscription
  useEffect(() => {
    const unsubscribeSparkline = subscribeToSparklineUpdates((message) => {
      const { sampleId, intensities } = message.data;

      // Check if this update is for our current sequence (use ref for persistence)
      const currentSeqId = sparklineSequenceIdRef.current || currentSequenceId;
      if (!currentSeqId) {
        return;
      }

      if (sampleId && intensities && Array.isArray(intensities)) {
        // Save sparkline data to localStorage for persistence
        if (currentSeqId) {
          saveSparklineData(currentSeqId, sampleId, intensities);
        }

        isUpdatingSparklineRef.current = true;
        setAcquisitionSamples((prevSamples) => {
          const updatedSamples = prevSamples.map((sample) => {
            // Match by backendSampleId (GUID) from the SSE event
            if (sample.backendSampleId === sampleId) {
              return {
                ...sample,
                sparklineData: intensities,
              };
            }
            // Explicitly preserve sparkline data for other samples
            return {
              ...sample,
              sparklineData: sample.sparklineData || [],
            };
          });

          // Clear the flag after a short delay to allow the state update to complete
          setTimeout(() => {
            isUpdatingSparklineRef.current = false;
          }, 100);

          return updatedSamples;
        });
      }
    });

    return () => {
      unsubscribeSparkline();
    };
  }, [currentSequenceId, saveSparklineData, subscribeToSparklineUpdates]); // Add missing dependencies

  const handleStartAcquisition = () => {
    navigate("/acquisition");
  };

  // Helper function to map acquisition state to status
  const getStatusFromSequenceState = (
    acquisitionState: number,
  ): Sample["status"] => {
    // Map acquisition state numbers to status strings using the AcquisitionState enum
    switch (acquisitionState) {
      case AcquisitionState.Stopped:
      case AcquisitionState.Ready:
        return "Pending";
      case AcquisitionState.ReadBarcode:
      case AcquisitionState.SendMethod:
      case AcquisitionState.WaitMethodReady:
      case AcquisitionState.WaitStartSlaves:
      case AcquisitionState.WaitStartMaster:
      case AcquisitionState.WaitContactClosure:
      case AcquisitionState.Acquire:
      case AcquisitionState.WaitMethodRun:
      case AcquisitionState.PreAcquisitionProgram:
      case AcquisitionState.WaitPreAcquisitionProgram:
        return "Running";
      case AcquisitionState.PostRun:
      case AcquisitionState.PostAcquisitionProgram:
      case AcquisitionState.WaitPostAcquisitionProgram:
        return "Complete";
      case AcquisitionState.Error:
      case AcquisitionState.Abort:
        return "Error";
      case AcquisitionState.Load:
      case AcquisitionState.Initialize:
      case AcquisitionState.WaitInitialize:
      case AcquisitionState.WaitReady:
      case AcquisitionState.WaitBarcode:
      case AcquisitionState.WaitBarcodeError:
        return "Queued";
      default:
        return "Pending";
    }
  };

  const getStatusColor = (status: InstrumentData["status"]) => {
    switch (status) {
      case "online":
        return "success";
      case "running":
        return "info";
      case "offline":
        return "default";
      case "error":
        return "error";
      case "maintenance":
        return "warning";
      default:
        return "default";
    }
  };

  const getStatusText = (status: InstrumentData["status"]) => {
    switch (status) {
      case "online":
        return "Online";
      case "running":
        return "Running";
      case "offline":
        return "Offline";
      case "error":
        return "Error";
      case "maintenance":
        return "Maintenance";
      default:
        return "Unknown";
    }
  };

  const getSampleStatusColor = (status: Sample["status"]) => {
    switch (status) {
      case "Complete":
        return "success";
      case "Running":
        return "info";
      case "Pending":
        return "default";
      case "Error":
        return "error";
      case "Queued":
        return "warning";
      default:
        return "default";
    }
  };

  // Column definitions for DataGrid (same as Acquisition.tsx)
  const columns: GridColDef[] = [
    {
      field: "sampleType",
      headerName: "Sample Type",
      minWidth: 80,
      flex: 0.6,
      editable: false,
      headerAlign: "center",
      align: "center",
    },
    {
      field: "sampleName",
      headerName: "Sample Name",
      minWidth: 60,
      flex: 0.5,
      editable: false,
      headerAlign: "center",
      align: "center",
    },
    {
      field: "rawFileName",
      headerName: "Raw file name",
      minWidth: 80,
      flex: 1,
      editable: false,
      headerAlign: "center",
      align: "center",
      renderCell: (params) => {
        const rawFilePath = params.value || "";
        const rawFileName = rawFilePath.split("\\").pop() || rawFilePath;
        return <span>{rawFileName}</span>;
      },
    },
    {
      field: "instMethod",
      headerName: "Instrument Method",
      minWidth: 140,
      flex: 1,
      editable: false,
      headerAlign: "center",
      align: "center",
      renderCell: (params) => {
        const methodPath = params.value || "";
        const methodName = methodPath.split("\\").pop() || methodPath;
        return <span>{methodName}</span>;
      },
    },
    {
      field: "position",
      headerName: "Position",
      minWidth: 60,
      flex: 0.4,
      editable: false,
      headerAlign: "center",
      align: "center",
    },
    {
      field: "injVol",
      headerName: "Inj Vol (μL)",
      type: "number",
      minWidth: 60,
      flex: 0.4,
      editable: false,
      headerAlign: "center",
      align: "center",
    },
    {
      field: "status",
      headerName: "Status",
      minWidth: 100,
      flex: 0.6,
      editable: false,
      headerAlign: "center",
      align: "center",
      sortable: true,
      renderCell: (params) => {
        if (!params.value) return null;

        const chipProps = getSampleStatusColor(params.value);

        return (
          <Chip
            label={params.value}
            size="small"
            color={chipProps}
            sx={{
              fontWeight: 500,
              minWidth: 80,
            }}
          />
        );
      },
    },
    {
      field: "sparklineData",
      headerName: "Progress",
      minWidth: 120,
      flex: 0.8,
      editable: false,
      headerAlign: "center",
      align: "center",
      sortable: false,
      renderCell: (params) => {
        const data = params.value || [];
        if (!Array.isArray(data) || data.length === 0) {
          return (
            <Box
              sx={{
                width: "100%",
                height: "100%",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
              }}
            >
              <Typography
                variant="caption"
                color="text.secondary"
                sx={{ fontSize: "0.7rem" }}
              >
                No data
              </Typography>
            </Box>
          );
        }

        return (
          <Box
            sx={{
              width: "100%",
              height: "100%",
              display: "flex",
              alignItems: "center",
            }}
          >
            <SparkLineChart
              data={data}
              width={100}
              height={40}
              plotType="line"
              showHighlight={false}
              showTooltip={false}
              color="#1976d2"
            />
          </Box>
        );
      },
    },
  ];

  if (!instrument) {
    return (
      <MainLayout>
        <Box sx={{ p: 3 }}>
          <Typography variant="h6" color="error">
            Instrument not found
          </Typography>
        </Box>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <Box
        sx={{
          p: 2,
          height: "calc(100vh - 64px)", // Account for header/navbar height
          display: "flex",
          flexDirection: "column",
          gap: 2,
          overflow: "hidden", // Prevent scrollbars
        }}
      >
        {/* Header */}
        <Box
          sx={{
            display: "flex",
            alignItems: "center",
            mb: 1.5, // Reduced margin
            flexWrap: "wrap",
            gap: 2,
            flexShrink: 0, // Prevent header from shrinking
          }}
        >
          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              gap: 2,
              flexGrow: 1,
              minWidth: 0,
            }}
          >
            <MemoryIcon sx={{ color: "primary.main", fontSize: 32 }} />
            <Box sx={{ minWidth: 0 }}>
              <Typography
                variant="h4"
                component="h1"
                sx={{
                  fontWeight: 600,
                  whiteSpace: "nowrap",
                  overflow: "hidden",
                  textOverflow: "ellipsis",
                }}
              >
                {instrument.name}
              </Typography>
              <Typography
                variant="body1"
                color="text.secondary"
                sx={{
                  whiteSpace: "nowrap",
                  overflow: "hidden",
                  textOverflow: "ellipsis",
                }}
              >
                {instrument.type} • {instrument.serialNumber}
              </Typography>
            </Box>
          </Box>
          <Box sx={{ display: "flex", gap: 1, flexShrink: 0 }}>
            {instrument.status === "running" ? (
              <Button variant="outlined" startIcon={<StopIcon />} color="error">
                Stop Acquisition
              </Button>
            ) : (
              <Button
                variant="contained"
                startIcon={<PlayArrowIcon />}
                onClick={handleStartAcquisition}
              >
                Submit Acquisition
              </Button>
            )}
          </Box>
        </Box>

        {/* Main Content Layout - Flex-optimized for Maximum Chromatogram Space */}
        <Box
          sx={{
            display: "flex",
            flexDirection: "column",
            gap: { xs: 1.5, md: 2 }, // Reduced gaps
            flex: 1,
            minHeight: 0, // Flexbox content area
          }}
        >
          {isMobile ? (
            // Mobile Layout: Compact with tabs and collapsible sections
            <>
              {/* Compact Info Bar */}
              <Card sx={{ mb: 1 }}>
                <CardContent sx={{ py: 1.5, "&:last-child": { pb: 1.5 } }}>
                  <Box
                    sx={{
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "space-between",
                      mb: 1,
                    }}
                  >
                    <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                      <Chip
                        label={getStatusText(instrument.status)}
                        color={getStatusColor(instrument.status)}
                        size="small"
                      />
                      {currentSequence && (
                        <Chip
                          label={`${acquisitionSamples.filter((s) => s.status === "Complete").length}/${acquisitionSamples.length}`}
                          size="small"
                          variant="outlined"
                          color="primary"
                        />
                      )}
                    </Box>
                    <IconButton
                      size="small"
                      onClick={() => setInfoExpanded(!infoExpanded)}
                    >
                      {infoExpanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                    </IconButton>
                  </Box>

                  <Collapse in={infoExpanded}>
                    <Box
                      sx={{
                        display: "flex",
                        flexDirection: "column",
                        gap: 1.5,
                      }}
                    >
                      {/* Device Information */}
                      <Box>
                        <Typography
                          variant="caption"
                          color="text.secondary"
                          sx={{ fontWeight: 600, mb: 0.5, display: "block" }}
                        >
                          Device Information
                        </Typography>
                        <Box
                          sx={{
                            display: "flex",
                            flexDirection: "column",
                            gap: 0.3,
                          }}
                        >
                          <Box
                            sx={{
                              display: "flex",
                              justifyContent: "space-between",
                              alignItems: "flex-start",
                            }}
                          >
                            <Typography
                              variant="caption"
                              color="text.secondary"
                              sx={{ fontSize: "0.75rem", flexShrink: 0, mr: 1 }}
                            >
                              Type:
                            </Typography>
                            <Typography
                              variant="caption"
                              sx={{
                                fontSize: "0.75rem",
                                textAlign: "right",
                                wordBreak: "break-word",
                                lineHeight: 1.2,
                                maxWidth: "200px",
                              }}
                              title={instrument.type}
                            >
                              {instrument.type}
                            </Typography>
                          </Box>
                          <Box
                            sx={{
                              display: "flex",
                              justifyContent: "space-between",
                            }}
                          >
                            <Typography
                              variant="caption"
                              color="text.secondary"
                              sx={{ fontSize: "0.75rem" }}
                            >
                              IP:
                            </Typography>
                            <Typography
                              variant="caption"
                              sx={{ fontSize: "0.75rem" }}
                            >
                              {instrument.ipAddress}
                            </Typography>
                          </Box>
                          <Box
                            sx={{
                              display: "flex",
                              justifyContent: "space-between",
                              alignItems: "flex-start",
                            }}
                          >
                            <Typography
                              variant="caption"
                              color="text.secondary"
                              sx={{ fontSize: "0.75rem", flexShrink: 0, mr: 1 }}
                            >
                              Serial:
                            </Typography>
                            <Typography
                              variant="caption"
                              sx={{
                                fontSize: "0.75rem",
                                textAlign: "right",
                                wordBreak: "break-word",
                                lineHeight: 1.2,
                                maxWidth: "200px",
                              }}
                              title={instrument.serialNumber}
                            >
                              {instrument.serialNumber}
                            </Typography>
                          </Box>
                        </Box>
                      </Box>

                      {/* Current Acquisition */}
                      {currentSequence && (
                        <Box>
                          <Typography
                            variant="caption"
                            color="text.secondary"
                            sx={{ fontWeight: 600, mb: 0.5, display: "block" }}
                          >
                            Current Acquisition
                          </Typography>
                          <Box
                            sx={{
                              display: "flex",
                              flexDirection: "column",
                              gap: 0.3,
                            }}
                          >
                            <Box
                              sx={{
                                display: "flex",
                                justifyContent: "space-between",
                                alignItems: "flex-start",
                              }}
                            >
                              <Typography
                                variant="caption"
                                color="text.secondary"
                                sx={{
                                  fontSize: "0.75rem",
                                  flexShrink: 0,
                                  mr: 1,
                                }}
                              >
                                Sequence:
                              </Typography>
                              <Typography
                                variant="caption"
                                sx={{
                                  fontSize: "0.75rem",
                                  textAlign: "right",
                                  wordBreak: "break-word",
                                  hyphens: "auto",
                                  lineHeight: 1.2,
                                  maxWidth: "200px",
                                  display: "-webkit-box",
                                  WebkitLineClamp: 2,
                                  WebkitBoxOrient: "vertical",
                                  overflow: "hidden",
                                }}
                                title={currentSequence.name || "Unnamed"}
                              >
                                {currentSequence.name || "Unnamed"}
                              </Typography>
                            </Box>
                            <Box
                              sx={{
                                display: "flex",
                                justifyContent: "space-between",
                              }}
                            >
                              <Typography
                                variant="caption"
                                color="text.secondary"
                                sx={{ fontSize: "0.75rem" }}
                              >
                                Status:
                              </Typography>
                              <Typography
                                variant="caption"
                                sx={{ fontSize: "0.75rem" }}
                              >
                                {currentSequence.sequenceStatus
                                  ?.sequenceState ===
                                SequenceState.SequenceComplete
                                  ? "Complete"
                                  : "Running"}
                              </Typography>
                            </Box>
                            <Box
                              sx={{
                                display: "flex",
                                justifyContent: "space-between",
                              }}
                            >
                              <Typography
                                variant="caption"
                                color="text.secondary"
                                sx={{ fontSize: "0.75rem" }}
                              >
                                Progress:
                              </Typography>
                              <Typography
                                variant="caption"
                                sx={{ fontSize: "0.75rem" }}
                              >
                                {
                                  acquisitionSamples.filter(
                                    (s) => s.status === "Complete",
                                  ).length
                                }
                                /{acquisitionSamples.length}
                              </Typography>
                            </Box>
                            <Box
                              sx={{
                                display: "flex",
                                justifyContent: "space-between",
                                alignItems: "flex-start",
                              }}
                            >
                              <Typography
                                variant="caption"
                                color="text.secondary"
                                sx={{
                                  fontSize: "0.75rem",
                                  flexShrink: 0,
                                  mr: 1,
                                }}
                              >
                                Sample:
                              </Typography>
                              <Typography
                                variant="caption"
                                sx={{
                                  fontSize: "0.75rem",
                                  textAlign: "right",
                                  wordBreak: "break-word",
                                  hyphens: "auto",
                                  lineHeight: 1.2,
                                  maxWidth: "200px",
                                  display: "-webkit-box",
                                  WebkitLineClamp: 2,
                                  WebkitBoxOrient: "vertical",
                                  overflow: "hidden",
                                }}
                                title={
                                  acquisitionSamples[currentSampleIndex]
                                    ?.sampleName || "Unknown"
                                }
                              >
                                {acquisitionSamples[currentSampleIndex]
                                  ?.sampleName || "Unknown"}
                              </Typography>
                            </Box>
                          </Box>
                        </Box>
                      )}
                    </Box>
                  </Collapse>
                </CardContent>
              </Card>

              {/* Mobile Tabs */}
              <Card>
                <Tabs
                  value={activeTab}
                  onChange={(_, newValue) => setActiveTab(newValue)}
                  variant="fullWidth"
                  sx={{
                    "& .MuiTab-root": {
                      minHeight: 48,
                      fontSize: "0.75rem",
                    },
                  }}
                >
                  <Tab
                    icon={<TimelineIcon />}
                    label="Plot"
                    iconPosition="start"
                    sx={{ gap: 0.5 }}
                  />
                  <Tab
                    icon={<PlaylistPlayIcon />}
                    label="Samples"
                    iconPosition="start"
                    sx={{ gap: 0.5 }}
                  />
                </Tabs>
              </Card>

              {/* Tab Content */}
              <Box sx={{ flex: 1, minHeight: 0 }}>
                {" "}
                {/* Flexbox container for tab content */}
                {activeTab === 0 && (
                  // Chromatogram Plot Tab
                  <Card
                    sx={{
                      height: "100%",
                      display: "flex",
                      flexDirection: "column",
                    }}
                  >
                    <CardContent
                      sx={{
                        flex: 1,
                        display: "flex",
                        flexDirection: "column",
                        minHeight: 0,
                        p: 2,
                      }}
                    >
                      <Typography
                        variant="h6"
                        sx={{ mb: 1, fontWeight: 600, fontSize: "1rem" }}
                      >
                        Chromatogram Plot
                      </Typography>
                      <Box
                        sx={{
                          flex: 1,
                          border: "1px solid #e0e0e0",
                          borderRadius: 1,
                          display: "flex",
                          alignItems: "center",
                          justifyContent: "center",
                          backgroundColor: "#f5f5f5",
                          minHeight: 300, // Added minimum height for better display
                        }}
                      >
                        {chromatogramUrl ? (
                          <img
                            src={chromatogramUrl}
                            alt="Live Chromatogram"
                            style={{
                              maxWidth: "100%",
                              maxHeight: "100%",
                              objectFit: "contain",
                            }}
                          />
                        ) : (
                          <Typography
                            variant="body2"
                            color="text.secondary"
                            sx={{ textAlign: "center", px: 2 }}
                          >
                            {instrument.status === "running"
                              ? "Waiting for chromatogram data..."
                              : "No active acquisition"}
                          </Typography>
                        )}
                      </Box>
                    </CardContent>
                  </Card>
                )}
                {activeTab === 1 && (
                  // Acquisitions Tab
                  <Card
                    sx={{
                      height: "100%",
                      display: "flex",
                      flexDirection: "column",
                    }}
                  >
                    <CardContent
                      sx={{
                        flex: 1,
                        display: "flex",
                        flexDirection: "column",
                        minHeight: 0,
                        p: 2,
                      }}
                    >
                      <Typography
                        variant="h6"
                        sx={{ mb: 1, fontWeight: 600, fontSize: "1rem" }}
                      >
                        Acquisitions
                      </Typography>
                      <Box
                        sx={{
                          flex: 1,
                          minHeight: 0,
                          "& .MuiDataGrid-root": {
                            backgroundColor: "#fff",
                            border: "1px solid #e0e0e0",
                            borderRadius: "8px",
                            boxShadow: "0 2px 4px rgba(0,0,0,0.05)",
                          },
                          "& .MuiDataGrid-cell": {
                            padding: "8px",
                            "&:focus": { outline: "none" },
                            whiteSpace: "nowrap",
                            overflow: "hidden",
                            textOverflow: "ellipsis",
                          },
                          "& .MuiDataGrid-columnHeader": {
                            backgroundColor: "#f5f5f5",
                            fontWeight: "bold",
                            fontSize: "0.875rem",
                            "&:focus": { outline: "none" },
                          },
                          "& .MuiDataGrid-row:hover": {
                            backgroundColor: "rgba(25, 118, 210, 0.08)",
                          },
                          "& .MuiDataGrid-row.running-sample": {
                            backgroundColor: "rgba(33, 150, 243, 0.08)",
                          },
                        }}
                      >
                        <DataGrid
                          rows={acquisitionSamples}
                          columns={columns}
                          hideFooter
                          disableRowSelectionOnClick
                          disableColumnResize={false}
                          columnHeaderHeight={56}
                          rowHeight={52}
                          density="standard"
                          getRowClassName={(params) =>
                            params.row.status === "Running"
                              ? "running-sample"
                              : ""
                          }
                          slots={{
                            noRowsOverlay: () => (
                              <Box
                                sx={{
                                  display: "flex",
                                  justifyContent: "center",
                                  alignItems: "center",
                                  height: "100%",
                                }}
                              >
                                <Typography
                                  variant="body2"
                                  color="text.secondary"
                                >
                                  No active acquisition
                                </Typography>
                              </Box>
                            ),
                          }}
                        />
                      </Box>
                    </CardContent>
                  </Card>
                )}
              </Box>
            </>
          ) : (
            // Desktop/Tablet Layout: Optimized flex layout for maximum chromatogram space
            <>
              {/* Top Row: Flex layout with merged info card and maximized chromatogram */}
              <Box
                sx={{
                  display: "flex",
                  flexDirection: { md: "row" },
                  gap: 1.5,
                  height: { md: isTablet ? "320px" : "360px" }, // Increased height for better chromatogram display
                  flexShrink: 0, // Prevent shrinking
                }}
              >
                {/* Merged Device & Acquisition Info (Vertical stack) */}
                <Card
                  sx={{
                    flex: { md: "0 0 280px", lg: "0 0 320px" }, // Increased width for better info display
                    display: "flex",
                    flexDirection: "column",
                  }}
                >
                  <CardContent
                    sx={{
                      flex: 1,
                      p: 1.5,
                      display: "flex",
                      flexDirection: "column",
                    }}
                  >
                    {/* Device Information Section */}
                    <Box sx={{ mb: 1.5 }}>
                      <Box
                        sx={{
                          display: "flex",
                          alignItems: "center",
                          gap: 1,
                          mb: 1,
                        }}
                      >
                        <InfoIcon sx={{ fontSize: 14 }} />
                        <Typography
                          variant="subtitle1"
                          sx={{ fontWeight: 600, fontSize: "0.8rem" }}
                        >
                          Device Information
                        </Typography>
                      </Box>

                      <Chip
                        label={getStatusText(instrument.status)}
                        color={getStatusColor(instrument.status)}
                        size="small"
                        sx={{ mb: 1, height: 24, fontSize: "0.7rem" }}
                      />

                      <Box sx={{ "& > div": { mb: 0.2 } }}>
                        <Box
                          sx={{
                            display: "flex",
                            justifyContent: "space-between",
                            alignItems: "flex-start",
                          }}
                        >
                          <Typography
                            variant="caption"
                            color="text.secondary"
                            sx={{ fontSize: "0.7rem", flexShrink: 0, mr: 1 }}
                          >
                            Type:
                          </Typography>
                          <Typography
                            variant="caption"
                            sx={{
                              fontSize: "0.7rem",
                              textAlign: "right",
                              wordBreak: "break-word",
                              lineHeight: 1.2,
                              maxWidth: "140px",
                            }}
                            title={instrument.type}
                          >
                            {instrument.type}
                          </Typography>
                        </Box>
                        <Box
                          sx={{
                            display: "flex",
                            justifyContent: "space-between",
                          }}
                        >
                          <Typography
                            variant="caption"
                            color="text.secondary"
                            sx={{ fontSize: "0.7rem" }}
                          >
                            IP:
                          </Typography>
                          <Typography
                            variant="caption"
                            sx={{ fontSize: "0.7rem" }}
                          >
                            {instrument.ipAddress}
                          </Typography>
                        </Box>
                        <Box
                          sx={{
                            display: "flex",
                            justifyContent: "space-between",
                            alignItems: "flex-start",
                          }}
                        >
                          <Typography
                            variant="caption"
                            color="text.secondary"
                            sx={{ fontSize: "0.7rem", flexShrink: 0, mr: 1 }}
                          >
                            Serial:
                          </Typography>
                          <Typography
                            variant="caption"
                            sx={{
                              fontSize: "0.7rem",
                              textAlign: "right",
                              wordBreak: "break-word",
                              lineHeight: 1.2,
                              maxWidth: "140px",
                            }}
                            title={instrument.serialNumber}
                          >
                            {instrument.serialNumber}
                          </Typography>
                        </Box>
                      </Box>
                    </Box>

                    {/* Divider */}
                    <Divider sx={{ my: 1 }} />

                    {/* Current Acquisition Section */}
                    <Box sx={{ flex: 1 }}>
                      <Box
                        sx={{
                          display: "flex",
                          alignItems: "center",
                          gap: 1,
                          mb: 1,
                        }}
                      >
                        <PlaylistPlayIcon sx={{ fontSize: 14 }} />
                        <Typography
                          variant="subtitle1"
                          sx={{ fontWeight: 600, fontSize: "0.8rem" }}
                        >
                          Current Acquisition
                        </Typography>
                      </Box>

                      {currentSequence ? (
                        <Box sx={{ "& > div": { mb: 0.2 } }}>
                          <Box
                            sx={{
                              display: "flex",
                              justifyContent: "space-between",
                              alignItems: "flex-start",
                            }}
                          >
                            <Typography
                              variant="caption"
                              color="text.secondary"
                              sx={{ fontSize: "0.7rem", flexShrink: 0, mr: 1 }}
                            >
                              Sequence:
                            </Typography>
                            <Typography
                              variant="caption"
                              sx={{
                                textAlign: "right",
                                fontSize: "0.7rem",
                                wordBreak: "break-word",
                                hyphens: "auto",
                                lineHeight: 1.2,
                                maxWidth: "140px",
                                display: "-webkit-box",
                                WebkitLineClamp: 2,
                                WebkitBoxOrient: "vertical",
                                overflow: "hidden",
                              }}
                              title={currentSequence.name || "Unnamed"}
                            >
                              {currentSequence.name || "Unnamed"}
                            </Typography>
                          </Box>
                          <Box
                            sx={{
                              display: "flex",
                              justifyContent: "space-between",
                            }}
                          >
                            <Typography
                              variant="caption"
                              color="text.secondary"
                              sx={{ fontSize: "0.7rem" }}
                            >
                              Status:
                            </Typography>
                            <Typography
                              variant="caption"
                              sx={{ fontSize: "0.7rem" }}
                            >
                              {currentSequence.sequenceStatus?.sequenceState ===
                              SequenceState.SequenceComplete
                                ? "Complete"
                                : "Running"}
                            </Typography>
                          </Box>
                          <Box
                            sx={{
                              display: "flex",
                              justifyContent: "space-between",
                            }}
                          >
                            <Typography
                              variant="caption"
                              color="text.secondary"
                              sx={{ fontSize: "0.7rem" }}
                            >
                              Progress:
                            </Typography>
                            <Typography
                              variant="caption"
                              sx={{ fontSize: "0.7rem" }}
                            >
                              {
                                acquisitionSamples.filter(
                                  (s) => s.status === "Complete",
                                ).length
                              }
                              /{acquisitionSamples.length}
                            </Typography>
                          </Box>
                          <Box
                            sx={{
                              display: "flex",
                              justifyContent: "space-between",
                              alignItems: "flex-start",
                            }}
                          >
                            <Typography
                              variant="caption"
                              color="text.secondary"
                              sx={{ fontSize: "0.7rem", flexShrink: 0, mr: 1 }}
                            >
                              Sample:
                            </Typography>
                            <Typography
                              variant="caption"
                              sx={{
                                textAlign: "right",
                                fontSize: "0.7rem",
                                wordBreak: "break-word",
                                hyphens: "auto",
                                lineHeight: 1.2,
                                maxWidth: "140px",
                                display: "-webkit-box",
                                WebkitLineClamp: 2,
                                WebkitBoxOrient: "vertical",
                                overflow: "hidden",
                              }}
                              title={
                                acquisitionSamples[currentSampleIndex]
                                  ?.sampleName || "Unknown"
                              }
                            >
                              {acquisitionSamples[currentSampleIndex]
                                ?.sampleName || "Unknown"}
                            </Typography>
                          </Box>
                        </Box>
                      ) : (
                        <Typography
                          variant="caption"
                          color="text.secondary"
                          sx={{
                            textAlign: "center",
                            mt: 1,
                            fontSize: "0.7rem",
                          }}
                        >
                          No active acquisition
                        </Typography>
                      )}
                    </Box>
                  </CardContent>
                </Card>

                {/* Chromatogram Plot (Maximized space) */}
                <Card
                  sx={{
                    flex: 1,
                    display: "flex",
                    flexDirection: "column",
                    minWidth: 0,
                  }}
                >
                  <CardContent
                    sx={{
                      flex: 1,
                      display: "flex",
                      flexDirection: "column",
                      minHeight: 0,
                      p: 1.5,
                    }}
                  >
                    <Box
                      sx={{
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "space-between",
                        mb: 0.5,
                        flexWrap: "wrap",
                        gap: 0.5,
                      }}
                    >
                      <Box
                        sx={{ display: "flex", alignItems: "center", gap: 1 }}
                      >
                        <TimelineIcon sx={{ fontSize: 14 }} />
                        <Typography
                          variant="subtitle1"
                          sx={{ fontWeight: 600, fontSize: "0.8rem" }}
                        >
                          Chromatogram Plot
                        </Typography>
                      </Box>
                      {currentSequence && (
                        <Box
                          sx={{
                            display: "flex",
                            gap: 0.5,
                            flexWrap: "wrap",
                            justifyContent: "flex-end",
                            maxWidth: "500px",
                          }}
                        >
                          <Chip
                            label={currentSequence.name || "Unnamed"}
                            size="small"
                            color="primary"
                            variant="outlined"
                            sx={{
                              fontSize: "0.7rem",
                              height: 20,
                              maxWidth: "250px",
                              "& .MuiChip-label": {
                                overflow: "hidden",
                                textOverflow: "ellipsis",
                                whiteSpace: "nowrap",
                                paddingX: "8px",
                              },
                            }}
                            title={`Sequence: ${currentSequence.name || "Unnamed"}`}
                          />
                          {acquisitionSamples[currentSampleIndex] && (
                            <Chip
                              label={
                                acquisitionSamples[currentSampleIndex]
                                  .sampleName || "Unknown"
                              }
                              size="small"
                              color="secondary"
                              variant="outlined"
                              sx={{
                                fontSize: "0.7rem",
                                height: 20,
                                maxWidth: "250px",
                                "& .MuiChip-label": {
                                  overflow: "hidden",
                                  textOverflow: "ellipsis",
                                  whiteSpace: "nowrap",
                                  paddingX: "8px",
                                },
                              }}
                              title={`Sample: ${acquisitionSamples[currentSampleIndex].sampleName || "Unknown"}`}
                            />
                          )}
                        </Box>
                      )}
                    </Box>

                    <Box
                      sx={{
                        flex: 1,
                        border: "1px solid #e0e0e0",
                        borderRadius: 1,
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        backgroundColor: "#f5f5f5",
                        minHeight: 0,
                      }}
                    >
                      {chromatogramUrl ? (
                        <img
                          src={chromatogramUrl}
                          alt="Live Chromatogram"
                          style={{
                            maxWidth: "100%",
                            maxHeight: "100%",
                            objectFit: "contain",
                          }}
                        />
                      ) : (
                        <Typography
                          variant="body2"
                          color="text.secondary"
                          sx={{
                            textAlign: "center",
                            px: 2,
                            fontSize: "0.8rem",
                          }}
                        >
                          {instrument.status === "running"
                            ? "Waiting for chromatogram data..."
                            : "No active acquisition"}
                        </Typography>
                      )}
                    </Box>
                  </CardContent>
                </Card>
              </Box>

              {/* Bottom Row: Flexible acquisitions grid using remaining space */}
              <Box sx={{ flex: 1, minHeight: "250px" }}>
                <Card
                  sx={{
                    height: "100%",
                    display: "flex",
                    flexDirection: "column",
                  }}
                >
                  <CardContent
                    sx={{
                      flex: 1,
                      display: "flex",
                      flexDirection: "column",
                      minHeight: 0,
                      p: 1.5,
                    }}
                  >
                    <Typography
                      variant="h6"
                      sx={{ mb: 0.5, fontWeight: 600, fontSize: "0.9rem" }}
                    >
                      Acquisitions
                    </Typography>

                    <Box
                      sx={{
                        flex: 1,
                        minHeight: 0,
                        "& .MuiDataGrid-root": {
                          backgroundColor: "#fff",
                          border: "1px solid #e0e0e0",
                          borderRadius: "8px",
                          boxShadow: "0 2px 4px rgba(0,0,0,0.05)",
                        },
                        "& .MuiDataGrid-cell": {
                          padding: "6px 8px",
                          fontSize: "0.8rem",
                          "&:focus": { outline: "none" },
                          whiteSpace: "nowrap",
                          overflow: "hidden",
                          textOverflow: "ellipsis",
                        },
                        "& .MuiDataGrid-columnHeader": {
                          backgroundColor: "#f5f5f5",
                          fontWeight: "bold",
                          fontSize: "0.8rem",
                          "&:focus": { outline: "none" },
                        },
                        "& .MuiDataGrid-row:hover": {
                          backgroundColor: "rgba(25, 118, 210, 0.06)",
                        },
                        "& .MuiDataGrid-row.running-sample": {
                          backgroundColor: "rgba(33, 150, 243, 0.08)",
                        },
                        "& .MuiDataGrid-footerContainer": {
                          borderTop: "1px solid #e0e0e0",
                          backgroundColor: "#f8f9fa",
                          minHeight: "44px",
                        },
                      }}
                    >
                      <DataGrid
                        rows={acquisitionSamples}
                        columns={columns}
                        initialState={{
                          pagination: {
                            paginationModel: { page: 0, pageSize: 10 },
                          },
                          density: "standard",
                        }}
                        pageSizeOptions={[10, 15, 25]}
                        disableRowSelectionOnClick
                        disableColumnResize={false}
                        columnHeaderHeight={48}
                        rowHeight={44}
                        density="standard"
                        getRowClassName={(params) =>
                          params.row.status === "Running"
                            ? "running-sample"
                            : ""
                        }
                        slots={{
                          noRowsOverlay: () => (
                            <Box
                              sx={{
                                display: "flex",
                                justifyContent: "center",
                                alignItems: "center",
                                height: "100%",
                                flexDirection: "column",
                                gap: 0.5,
                              }}
                            >
                              <Typography
                                variant="body2"
                                color="text.secondary"
                                sx={{ fontSize: "0.8rem" }}
                              >
                                No acquisition sequence running
                              </Typography>
                              <Typography
                                variant="caption"
                                color="text.secondary"
                                sx={{ fontSize: "0.7rem" }}
                              >
                                Sequence details will appear when an acquisition
                                is active
                              </Typography>
                            </Box>
                          ),
                        }}
                      />
                    </Box>
                  </CardContent>
                </Card>
              </Box>
            </>
          )}
        </Box>
      </Box>
    </MainLayout>
  );
};

export default InstrumentDetail;
