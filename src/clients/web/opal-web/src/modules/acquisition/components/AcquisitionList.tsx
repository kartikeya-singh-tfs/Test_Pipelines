import React, { useState, useEffect, useCallback, useRef } from "react";
import {
  Box,
  Typography,
  Paper,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Chip,
  CircularProgress,
  LinearProgress,
  IconButton,
  TextField,
  InputAdornment,
  Alert,
} from "@mui/material";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import MainLayout from "../../../shared/components/MainLayout";
import FolderIcon from "@mui/icons-material/Folder";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import ErrorIcon from "@mui/icons-material/Error";
import PendingIcon from "@mui/icons-material/Pending";
import SearchIcon from "@mui/icons-material/Search";
import ClearIcon from "@mui/icons-material/Clear";
import { API_ENDPOINTS } from "../../../shared/constants/navigation-constants";
import { useGlobalSSE } from "../../../shared/contexts/GlobalSSEContext";
import type {
  ApiSequence,
  SampleInfo,
} from "../../../shared/contexts/GlobalSSEContext";

// UI-friendly sequence interface
interface AcquisitionSequence {
  id: string;
  name: string;
  description?: string; // Optional description field
  status: "running" | "completed" | "pending" | "error";
  progress: number;
  totalSamples: number;
  completedSamples: number;
  createdDate: string;
  apiData: ApiSequence;
}

// UI-friendly sample interface
interface UISample {
  id: number;
  sampleType: string;
  rawFileName: string;
  sampleName: string;
  instMethod: string;
  position: string;
  injVol: number;
  status: "Complete" | "Running" | "Pending" | "Error" | "Queued";
  backendSampleId: string;
}

const AcquisitionList: React.FC = () => {
  // Use GlobalSSE context
  const {
    isConnected: sseConnected,
    lastUpdateTime,
    connectionError,
    startConnection,
    subscribeToSequenceUpdates,
    subscribeToSampleUpdates,
    mapSequenceStateToStatus,
    mapAcquisitionStateToSampleStatus,
    mapSequenceStateStringToNumber,
    mapAcquisitionStateStringToNumber,
  } = useGlobalSSE();

  const [sequences, setSequences] = useState<AcquisitionSequence[]>([]);
  const [selectedSequence, setSelectedSequence] =
    useState<AcquisitionSequence | null>(null);
  const [sampleData, setSampleData] = useState<UISample[]>([]);
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // Helper functions using context utilities
  const mapApiSequenceToUI = useCallback(
    (apiSequence: unknown): AcquisitionSequence => {
      const sequence = apiSequence as Record<string, unknown>;
      // Extract sequenceStatus if present
      const sequenceStatus = sequence.sequenceStatus as
        | Record<string, unknown>
        | undefined;
      const sequenceState = sequenceStatus?.sequenceState as number | undefined;
      const isError = sequenceStatus?.isError as boolean | undefined;

      // Calculate totalSamples and completedSamples from actual samples array if available
      const samples = sequence.samples as
        | Array<Record<string, unknown>>
        | undefined;
      const actualTotalSamples =
        samples?.length || (sequence.totalSamples as number) || 0;
      const actualCompletedSamples =
        samples?.filter((sample) => {
          const sampleStatus = sample.sampleStatus as
            | Record<string, unknown>
            | undefined;
          const acquisitionState = sampleStatus?.acquisitionState as number;
          // Use the same mapping function as mapApiSamplesToUI
          const statusString =
            mapAcquisitionStateToSampleStatus(acquisitionState);
          return statusString === "Complete";
        }).length ||
        (sequence.completedSamples as number) ||
        0;

      return {
        id: (sequence.id as string) || (sequence.sequenceId as string),
        name: (sequence.name as string) || "Unknown",
        status: mapSequenceStateToStatus(sequenceState ?? 0, isError ?? false),
        progress: (sequence.progress as number) || 0,
        createdDate: (sequence.createdDate as string) || "",
        apiData: sequence as unknown as ApiSequence,
        totalSamples: actualTotalSamples,
        completedSamples: actualCompletedSamples,
      };
    },
    [mapSequenceStateToStatus, mapAcquisitionStateToSampleStatus],
  );

  // Fetch sequences from API
  // Only keep one fetchSequences definition

  const mapApiSamplesToUI = useCallback(
    (samples: SampleInfo[]): UISample[] => {
      return samples.map((sample, index) => ({
        id: index + 1,
        sampleType: sample.sampleData.type || "Unknown", // Use type field from API
        rawFileName: sample.sampleData.rawFilePath || "Unknown",
        sampleName: sample.sampleData.name || `Sample-${index + 1}`, // Use name field from API
        instMethod: sample.sampleData.methodFilePath || "Unknown",
        position: sample.sampleData.position || "Unknown",
        injVol: sample.sampleData.volume || 0,
        status: mapAcquisitionStateToSampleStatus(
          sample.sampleStatus.acquisitionState,
        ),
        backendSampleId: sample.sampleData.id,
      }));
    },
    [mapAcquisitionStateToSampleStatus],
  );

  // Fetch sequences from API
  const fetchSequences = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await fetch(API_ENDPOINTS.ACQUISITION_SEQUENCE);
      if (!response.ok) {
        throw new Error("Failed to fetch sequences");
      }

      const apiSequences: unknown[] = await response.json();
      const uiSequences = apiSequences.map(mapApiSequenceToUI);

      setSequences(uiSequences);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to fetch sequences",
      );
    } finally {
      setLoading(false);
    }
  }, [mapApiSequenceToUI]);

  // Handle sequence selection
  const handleSequenceSelect = (sequence: AcquisitionSequence) => {
    setSelectedSequence(sequence);

    // Update sample data based on selected sequence
    if (sequence.apiData.samples) {
      const uiSamples = mapApiSamplesToUI(sequence.apiData.samples);
      setSampleData(uiSamples);
    } else {
      setSampleData([]);
    }
  };

  // SSE connection management using GlobalSSE context
  useEffect(() => {
    // Always maintain SSE connection when component is mounted
    // Don't connect/disconnect based on sequence status to avoid constant toggling
    if (!sseConnected) {
      startConnection();
    }

    // Cleanup on unmount
    return () => {
      // Let the GlobalSSE context handle cleanup when all components unmount
    };
  }, [sseConnected, startConnection]); // Include dependencies

  // Debounced sequence update handler
  const sequenceUpdateTimeoutRef = useRef<number | null>(null);

  const debouncedSequenceUpdate = useCallback(
    (message: {
      Data: { SequenceId: string; SequenceState: string; IsError: boolean };
    }) => {
      if (sequenceUpdateTimeoutRef.current) {
        clearTimeout(sequenceUpdateTimeoutRef.current);
      }

      sequenceUpdateTimeoutRef.current = window.setTimeout(() => {
        // Handle sequence updates - convert from SSE message format to our local format
        const { SequenceId, SequenceState, IsError } = message.Data;

        // Convert string state to number using mapping function
        const sequenceStateNumber =
          mapSequenceStateStringToNumber(SequenceState);

        // Update sequences based on SSE data
        setSequences((prev) => {
          const updated = prev.map((seq) => {
            if (seq.id === SequenceId) {
              // Update the sequence status
              const updatedApiData = {
                ...seq.apiData,
                sequenceStatus: {
                  ...seq.apiData.sequenceStatus,
                  sequenceState: sequenceStateNumber,
                  isError: IsError,
                },
              };
              return mapApiSequenceToUI(updatedApiData);
            }
            return seq;
          });
          return updated;
        });

        // Update selected sequence if it's the one being updated
        setSelectedSequence((prev) => {
          if (prev && prev.id === SequenceId) {
            const updatedApiData = {
              ...prev.apiData,
              sequenceStatus: {
                ...prev.apiData.sequenceStatus,
                sequenceState: sequenceStateNumber,
                isError: IsError,
              },
            };
            const updatedSequence = mapApiSequenceToUI(updatedApiData);

            // Update sample data if needed
            if (updatedApiData.samples) {
              const uiSamples = mapApiSamplesToUI(updatedApiData.samples);
              setSampleData(uiSamples);
            }

            return updatedSequence;
          }
          return prev;
        });
      }, 300); // 300ms debounce
    },
    [mapSequenceStateStringToNumber, mapApiSequenceToUI, mapApiSamplesToUI],
  );

  // Subscribe to sequence updates from GlobalSSE
  useEffect(() => {
    const unsubscribe = subscribeToSequenceUpdates(debouncedSequenceUpdate);
    return unsubscribe;
  }, [subscribeToSequenceUpdates, debouncedSequenceUpdate]);

  // Subscribe to sample updates for individual sample status changes
  useEffect(() => {
    // Capture the mapping functions to avoid stale closures
    const mapStateStringToNumber = mapAcquisitionStateStringToNumber;
    const mapSamplesToUI = mapApiSamplesToUI;
    const mapApiToUI = mapApiSequenceToUI;

    const unsubscribe = subscribeToSampleUpdates((message) => {
      const { SequenceId, SampleId, AcquisitionState } = message.Data;

      // Convert string state to number using mapping function
      const acquisitionStateNumber = mapStateStringToNumber(AcquisitionState);

      // Update sample data in the selected sequence
      setSelectedSequence((prev) => {
        if (prev && prev.id === SequenceId && prev.apiData.samples) {
          const updatedSamples = prev.apiData.samples.map((sample) => {
            // Try both string and number comparison
            if (
              sample.sampleData.id === SampleId ||
              sample.sampleData.id === String(SampleId) ||
              String(sample.sampleData.id) === String(SampleId)
            ) {
              return {
                ...sample,
                sampleStatus: {
                  ...sample.sampleStatus,
                  acquisitionState: acquisitionStateNumber,
                },
              };
            }
            return sample;
          });

          // Return updated sequence
          const updatedApiData = {
            ...prev.apiData,
            samples: updatedSamples,
          };
          const updatedSequence = mapApiToUI(updatedApiData);

          // Update sample data display with the updated samples
          const uiSamples = mapSamplesToUI(updatedSamples);
          setSampleData(uiSamples);

          return updatedSequence;
        }
        return prev;
      });

      // Update sequences array as well - this is important for when no sequence is selected
      setSequences((prev) => {
        const updated = prev.map((seq) => {
          if (seq.id === SequenceId && seq.apiData.samples) {
            const updatedSamples = seq.apiData.samples.map((sample) => {
              if (
                sample.sampleData.id === SampleId ||
                sample.sampleData.id === String(SampleId) ||
                String(sample.sampleData.id) === String(SampleId)
              ) {
                return {
                  ...sample,
                  sampleStatus: {
                    ...sample.sampleStatus,
                    acquisitionState: acquisitionStateNumber,
                  },
                };
              }
              return sample;
            });

            const updatedApiData = {
              ...seq.apiData,
              samples: updatedSamples,
            };
            const updatedSequence = mapApiToUI(updatedApiData);

            // If this is the currently selected sequence, also update the sample data
            if (selectedSequence && selectedSequence.id === SequenceId) {
              const uiSamples = mapSamplesToUI(updatedSamples);
              setSampleData(uiSamples);
            }

            return updatedSequence;
          }
          return seq;
        });
        return updated;
      });
    });

    return unsubscribe;
  }, [
    subscribeToSampleUpdates,
    mapAcquisitionStateStringToNumber,
    mapApiSamplesToUI,
    mapApiSequenceToUI,
    selectedSequence,
  ]);

  // Load sequences on component mount
  useEffect(() => {
    fetchSequences();
  }, [fetchSequences]);

  // Filter and sort sequences based on search term and status priority
  const filteredAndSortedSequences = sequences
    .filter((sequence) =>
      sequence.name.toLowerCase().includes(searchTerm.toLowerCase()),
    )
    .sort((a, b) => {
      // Define priority order: running (1), pending/queued (2), error (3), completed (4)
      const getStatusPriority = (status: string) => {
        switch (status) {
          case "running":
            return 1;
          case "pending":
            return 2;
          case "error":
            return 3;
          case "completed":
            return 4;
          default:
            return 5;
        }
      };

      const priorityA = getStatusPriority(a.status);
      const priorityB = getStatusPriority(b.status);

      // Sort by priority first, then by name alphabetically within same priority
      if (priorityA !== priorityB) {
        return priorityA - priorityB;
      }

      // Secondary sort by name (alphabetical)
      return a.name.localeCompare(b.name);
    });

  // Auto-select sequence based on the filtered and sorted list (this matches what the user sees)
  useEffect(() => {
    if (filteredAndSortedSequences.length > 0 && !selectedSequence) {
      // Select the first sequence from the filtered and sorted list (which matches the UI display order)
      const sequenceToSelect = filteredAndSortedSequences[0];

      setSelectedSequence(sequenceToSelect);

      // Update sample data for the selected sequence
      if (sequenceToSelect.apiData.samples) {
        const uiSamples = mapApiSamplesToUI(sequenceToSelect.apiData.samples);
        setSampleData(uiSamples);
      } else {
        setSampleData([]);
      }
    }
  }, [filteredAndSortedSequences, selectedSequence, mapApiSamplesToUI]);

  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(event.target.value);
  };

  const handleClearSearch = () => {
    setSearchTerm("");
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case "running":
        return <PlayArrowIcon color="info" />;
      case "completed":
        return <CheckCircleIcon color="success" />;
      case "error":
        return <ErrorIcon color="error" />;
      case "pending":
        return <PendingIcon color="warning" />;
      default:
        return <FolderIcon />;
    }
  };

  const getStatusChip = (status: string, _progress?: number) => {
    switch (status) {
      case "running":
        return (
          <Chip label="Running" color="info" size="small" variant="filled" />
        );
      case "completed":
        return (
          <Chip
            label="Completed"
            color="success"
            size="small"
            variant="filled"
          />
        );
      case "error":
        return (
          <Chip label="Error" color="error" size="small" variant="filled" />
        );
      case "pending":
        return (
          <Chip
            label="Pending"
            color="warning"
            size="small"
            variant="outlined"
          />
        );
      default:
        return (
          <Chip
            label="Unknown"
            color="default"
            size="small"
            variant="outlined"
          />
        );
    }
  };

  // Sample grid columns
  const columns: GridColDef[] = [
    {
      field: "sampleType",
      headerName: "Sample Type",
      width: 140,
      headerAlign: "center",
      align: "center",
    },
    {
      field: "sampleName",
      headerName: "Sample Name",
      flex: 1,
      minWidth: 150,
      headerAlign: "center",
      align: "center",
    },
    {
      field: "rawFileName",
      headerName: "Raw File Name",
      flex: 1,
      minWidth: 180,
      headerAlign: "center",
      align: "center",
    },
    {
      field: "instMethod",
      headerName: "Instrument Method",
      flex: 1,
      minWidth: 180,
      headerAlign: "center",
      align: "center",
    },
    {
      field: "position",
      headerName: "Position",
      width: 100,
      headerAlign: "center",
      align: "center",
    },
    {
      field: "injVol",
      headerName: "Inj Vol (μL)",
      width: 120,
      headerAlign: "center",
      align: "center",
    },
    {
      field: "status",
      headerName: "Status",
      width: 140,
      headerAlign: "center",
      align: "center",
      renderCell: (params) => {
        if (!params.value) return null;

        const getStatusChipProps = (status: string) => {
          switch (status) {
            case "Complete":
              return { color: "success" as const, variant: "filled" as const };
            case "Running":
              return { color: "info" as const, variant: "filled" as const };
            case "Error":
              return { color: "error" as const, variant: "filled" as const };
            case "Pending":
              return {
                color: "warning" as const,
                variant: "outlined" as const,
              };
            case "Queued":
              return {
                color: "default" as const,
                variant: "outlined" as const,
              };
            default:
              return {
                color: "default" as const,
                variant: "outlined" as const,
              };
          }
        };

        const chipProps = getStatusChipProps(params.value);

        return (
          <Chip
            label={params.value}
            size="small"
            {...chipProps}
            sx={{
              fontWeight: 500,
              minWidth: 80,
            }}
          />
        );
      },
    },
  ];

  return (
    <MainLayout>
      <Box
        sx={{
          p: 3,
          height: "100%",
          display: "flex",
          flexDirection: "column",
          overflow: "hidden",
        }}
      >
        {/* Header */}
        <Box sx={{ mb: 3 }}>
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              mb: 1,
            }}
          >
            <Typography variant="h4" component="h1" sx={{ fontWeight: 600 }}>
              Acquisition Manager
            </Typography>

            {/* SSE Connection Status */}
            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
              <Box
                sx={{
                  width: 8,
                  height: 8,
                  borderRadius: "50%",
                  backgroundColor: sseConnected
                    ? "#4caf50"
                    : sequences.some(
                          (seq) =>
                            seq.status === "running" ||
                            seq.status === "pending",
                        )
                      ? "#f44336"
                      : "#9e9e9e",
                }}
              />
              <Typography variant="caption" color="text.secondary">
                {sseConnected
                  ? "Live"
                  : sequences.some(
                        (seq) =>
                          seq.status === "running" || seq.status === "pending",
                      )
                    ? "Connecting..."
                    : "Standby"}
              </Typography>
              {lastUpdateTime && sseConnected && (
                <Typography variant="caption" color="text.secondary">
                  • Last update: {lastUpdateTime}
                </Typography>
              )}
            </Box>
          </Box>
          <Typography variant="body2" color="text.secondary">
            Monitor and manage your acquisitions.
            {sseConnected && (
              <Typography
                component="span"
                sx={{ color: "#4caf50", fontWeight: 500 }}
              >
                {" "}
                • Real-time updates active
              </Typography>
            )}
            {sequences.some(
              (seq) => seq.status === "running" || seq.status === "pending",
            ) &&
              !sseConnected && (
                <Typography
                  component="span"
                  sx={{ color: "#f44336", fontWeight: 500 }}
                >
                  {" "}
                  • Connecting to live updates...
                </Typography>
              )}
          </Typography>

          {/* Error Display */}
          {(error || connectionError) && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {error || connectionError}
            </Alert>
          )}
        </Box>

        {/* Main Content Area */}
        <Box
          sx={{
            flex: 1,
            display: "flex",
            gap: 3,
            minHeight: 0,
            overflow: "hidden",
          }}
        >
          {/* Left Panel - Sequences List */}
          <Paper
            sx={{
              flex: "0 0 20%",
              display: "flex",
              flexDirection: "column",
              overflow: "hidden",
              minWidth: 300,
            }}
          >
            <Box
              sx={{
                p: 2,
                borderBottom: "1px solid #e0e0e0",
              }}
            >
              <Typography variant="h6" fontWeight={600}>
                Acquisition Lists
              </Typography>
            </Box>

            {/* Search Field */}
            <Box sx={{ p: 2, borderBottom: "1px solid #e0e0e0" }}>
              <TextField
                fullWidth
                size="small"
                placeholder="Search acquisition lists..."
                value={searchTerm}
                onChange={handleSearchChange}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchIcon color="action" />
                    </InputAdornment>
                  ),
                  endAdornment: searchTerm && (
                    <InputAdornment position="end">
                      <IconButton
                        size="small"
                        onClick={handleClearSearch}
                        edge="end"
                      >
                        <ClearIcon />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                sx={{
                  "& .MuiOutlinedInput-root": {
                    "& fieldset": {
                      borderColor: "#e0e0e0",
                    },
                    "&:hover fieldset": {
                      borderColor: "#1976d2",
                    },
                    "&.Mui-focused fieldset": {
                      borderColor: "#1976d2",
                    },
                  },
                }}
              />
            </Box>

            <List sx={{ flexGrow: 1, overflow: "auto", p: 0 }}>
              {loading ? (
                <Box sx={{ p: 3, textAlign: "center" }}>
                  <CircularProgress />
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    sx={{ mt: 1 }}
                  >
                    Loading sequences...
                  </Typography>
                </Box>
              ) : filteredAndSortedSequences.length === 0 ? (
                <Box sx={{ p: 3, textAlign: "center" }}>
                  <Typography variant="body2" color="text.secondary">
                    {searchTerm
                      ? "No sequences found matching your search."
                      : "No sequences available."}
                  </Typography>
                </Box>
              ) : (
                filteredAndSortedSequences.map((sequence, _index) => {
                  const isSelected = selectedSequence?.id === sequence.id;
                  const displayStatus = sequence.status;
                  return (
                    <React.Fragment
                      key={`${sequence.id}-${isSelected ? "selected" : "unselected"}`}
                    >
                      <ListItem disablePadding>
                        <ListItemButton
                          selected={isSelected}
                          onClick={() => handleSequenceSelect(sequence)}
                          sx={{
                            py: 2,
                            "&.Mui-selected": {
                              backgroundColor:
                                "rgba(25, 118, 210, 0.08) !important",
                              borderRight: "3px solid #1976d2",
                            },
                            "&:hover": {
                              backgroundColor: isSelected
                                ? "rgba(25, 118, 210, 0.08) !important"
                                : "rgba(0, 0, 0, 0.04)",
                            },
                          }}
                        >
                          <ListItemIcon>
                            {getStatusIcon(displayStatus)}
                          </ListItemIcon>
                          <ListItemText
                            primary={
                              <div
                                style={{
                                  display: "flex",
                                  justifyContent: "space-between",
                                  alignItems: "center",
                                  marginBottom: "4px",
                                }}
                              >
                                <div>
                                  <Typography
                                    variant="subtitle2"
                                    sx={{ fontWeight: 600 }}
                                    component="span"
                                  >
                                    {sequence.name || "Unnamed Sequence"}
                                  </Typography>
                                  {sequence.description && (
                                    <Typography
                                      variant="caption"
                                      color="text.secondary"
                                      display="block"
                                      component="span"
                                    >
                                      {sequence.description}
                                    </Typography>
                                  )}
                                </div>
                                {getStatusChip(displayStatus)}
                              </div>
                            }
                            secondary={
                              <>
                                <Typography
                                  variant="caption"
                                  color="text.secondary"
                                  display="block"
                                >
                                  {sequence.createdDate}
                                </Typography>
                                {displayStatus === "running" &&
                                  sequence.totalSamples > 0 && (
                                    <>
                                      <LinearProgress
                                        variant="determinate"
                                        value={
                                          (sequence.completedSamples /
                                            sequence.totalSamples) *
                                          100
                                        }
                                        sx={{
                                          height: 4,
                                          borderRadius: 2,
                                          mt: 1,
                                        }}
                                      />
                                      {!(
                                        sequence.completedSamples === 0 &&
                                        sequence.totalSamples === 1
                                      ) && (
                                        <Typography
                                          variant="caption"
                                          color="text.secondary"
                                        >
                                          {`${sequence.completedSamples}/${sequence.totalSamples} samples`}
                                        </Typography>
                                      )}
                                    </>
                                  )}
                              </>
                            }
                          />
                        </ListItemButton>
                      </ListItem>
                    </React.Fragment>
                  );
                })
              )}
            </List>
          </Paper>

          {/* Right Panel - Sample Data Table */}
          <Box
            sx={{
              flex: 1,
              display: "flex",
              flexDirection: "column",
              gap: 3,
              minWidth: 0,
              minHeight: 0,
            }}
          >
            {/* Top Panel - Sample Grid */}
            <Paper
              sx={{
                flex: 1,
                p: 2,
                display: "flex",
                flexDirection: "column",
                minHeight: 0,
              }}
            >
              <Box
                sx={{
                  display: "flex",
                  justifyContent: "space-between",
                  alignItems: "center",
                  mb: 2,
                }}
              >
                <Typography variant="h6" fontWeight={600}>
                  {selectedSequence?.name || "Select a sequence"}
                </Typography>
                {selectedSequence &&
                  selectedSequence.status === "running" &&
                  selectedSequence.totalSamples > 0 &&
                  !(
                    selectedSequence.completedSamples === 0 &&
                    selectedSequence.totalSamples === 1
                  ) && (
                    <Typography variant="body2" color="text.secondary">
                      {selectedSequence.completedSamples} of{" "}
                      {selectedSequence.totalSamples} samples
                    </Typography>
                  )}
              </Box>
              <Box
                sx={{
                  flexGrow: 1,
                  minHeight: 0,
                  "& .MuiDataGrid-root": {
                    border: "1px solid #e0e0e0",
                    borderRadius: "8px",
                  },
                  "& .MuiDataGrid-cell": {
                    "&:focus": {
                      outline: "none",
                    },
                  },
                  "& .MuiDataGrid-columnHeader": {
                    backgroundColor: "#f5f5f5",
                    fontWeight: "bold",
                    "&:focus": {
                      outline: "none",
                    },
                  },
                  "& .MuiDataGrid-row:hover": {
                    backgroundColor: "rgba(25, 118, 210, 0.08)",
                  },
                }}
              >
                <DataGrid
                  rows={sampleData}
                  columns={columns}
                  initialState={{
                    pagination: {
                      paginationModel: { page: 0, pageSize: 10 },
                    },
                  }}
                  pageSizeOptions={[10, 25, 50]}
                  disableRowSelectionOnClick
                  density="standard"
                  sx={{
                    height: "100%",
                    width: "100%",
                  }}
                />
              </Box>
            </Paper>
          </Box>
        </Box>
      </Box>
    </MainLayout>
  );
};

export default AcquisitionList;
