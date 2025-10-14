import React, {
  useState,
  useEffect,
  useRef,
  useCallback,
  useMemo,
  useTransition,
} from "react";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import TextField from "@mui/material/TextField";
import {
  Box,
  Typography,
  Button,
  Stack,
  Snackbar,
  Alert,
  Chip,
} from "@mui/material";
import {
  DataGrid,
  GridColDef,
  useGridApiRef,
  GridRowId,
} from "@mui/x-data-grid";
import { SparkLineChart } from "@mui/x-charts/SparkLineChart";
import MainLayout from "../../../shared/components/MainLayout";
import AddIcon from "@mui/icons-material/Add";
import DeleteIcon from "@mui/icons-material/Delete";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import StopIcon from "@mui/icons-material/Stop";
import Tooltip from "@mui/material/Tooltip";
import { Sample } from "../../../shared/models";
import { SAMPLE_TYPES } from "../../../shared/constants/acquisition-constants";
import { API_ENDPOINTS } from "../../../shared/constants/navigation-constants";
import { v4 as uuidv4 } from "uuid";
import { useGlobalSSE } from "../../../shared/contexts/GlobalSSEContext";
import { useNotification } from "../../../shared/contexts/NotificationContext";

const createEmptyRow = (id: number): Sample => ({
  id,
  sampleType: "",
  rawFileName: "",
  sampleName: "",
  instMethod: "",
  position: "",
  injVol: null,
  status: "Pending",
  sparklineData: [],
});

const initialRows: Sample[] = [createEmptyRow(1)];

const Acquisition: React.FC = () => {
  // Use GlobalSSE context
  const {
    isConnected: sseConnected,
    startConnection,
    stopConnection,
    subscribeToSequenceUpdates,
    subscribeToSampleUpdates,
    subscribeToSparklineUpdates,
  } = useGlobalSSE();

  // Use notification context for notification center
  const { addNotification, registerSequence } = useNotification();

  const [rows, setRows] = useState<Sample[]>(initialRows);
  const [nextId, setNextId] = useState(initialRows.length + 1);
  const [snackbarOpen, setSnackbarOpen] = useState(false);
  const [snackbarMessage, setSnackbarMessage] = useState("");
  const [selectedRowIds, setSelectedRowIds] = useState<GridRowId[]>([]);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [acqListName, setAcqListName] = useState("");
  const [acqListDesc, setAcqListDesc] = useState("");
  // Persist acquisition name for notifications
  const acqNameRef = useRef("");
  const [acquisitionSubmitted, setAcquisitionSubmitted] = useState(false);

  const apiRef = useGridApiRef();

  // Use React 18's useTransition for non-blocking state updates
  const [isPending, startTransition] = useTransition();

  // Validation logic for submit button
  const isSubmitDisabled = useMemo(() => {
    // Check if all rows have all required fields filled
    const allRowsComplete = rows.every((row) => {
      return (
        row.sampleType &&
        row.sampleType.trim() !== "" &&
        row.sampleName &&
        row.sampleName.trim() !== "" &&
        row.rawFileName &&
        row.rawFileName.trim() !== "" &&
        row.instMethod &&
        row.instMethod.trim() !== "" &&
        row.position &&
        row.position.trim() !== "" &&
        row.injVol !== null &&
        row.injVol !== undefined
      );
    });

    return !allRowsComplete;
  }, [rows]);

  // Get tooltip message for submit button
  const getSubmitTooltipMessage = useMemo(() => {
    if (acquisitionSubmitted) return "Acquisition in progress";

    // Check if all rows are complete
    const allRowsComplete = rows.every((row) => {
      return (
        row.sampleType &&
        row.sampleType.trim() !== "" &&
        row.sampleName &&
        row.sampleName.trim() !== "" &&
        row.rawFileName &&
        row.rawFileName.trim() !== "" &&
        row.instMethod &&
        row.instMethod.trim() !== "" &&
        row.position &&
        row.position.trim() !== "" &&
        row.injVol !== null &&
        row.injVol !== undefined
      );
    });

    return allRowsComplete
      ? "Start Analysis"
      : "Complete all rows with required fields";
  }, [rows, acquisitionSubmitted]);

  // Column definitions with responsive widths - memoized for performance
  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: "sampleType",
        headerName: "Sample Type",
        minWidth: 80,
        flex: 0.6,
        editable: true,
        type: "singleSelect",
        valueOptions: SAMPLE_TYPES,
        headerAlign: "center",
        align: "center",
        renderCell: (params) => {
          return (
            <Box
              sx={{
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                width: "100%",
                gap: 0.5,
              }}
            >
              <span>{params.value || "Select..."}</span>
              <span
                style={{
                  color: "#666",
                  fontSize: "12px",
                  opacity: params.value ? 0.6 : 0.8,
                }}
              >
                ▼
              </span>
            </Box>
          );
        },
      },
      {
        field: "sampleName",
        headerName: "Sample Name",
        minWidth: 60,
        flex: 0.5,
        editable: true,
        headerAlign: "center",
        align: "center",
      },
      {
        field: "rawFileName",
        headerName: "Raw file name",
        minWidth: 80,
        flex: 1,
        editable: true,
        headerAlign: "center",
        align: "center",
      },
      {
        field: "instMethod",
        headerName: "Instrument Method",
        minWidth: 140,
        flex: 1,
        editable: true,
        headerAlign: "center",
        align: "center",
      },
      {
        field: "position",
        headerName: "Position",
        minWidth: 60,
        flex: 0.4,
        editable: true,
        headerAlign: "center",
        align: "center",
      },
      {
        field: "injVol",
        headerName: "Inj Vol (μL)",
        type: "number",
        minWidth: 60,
        flex: 0.4,
        editable: true,
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

          // Define colors for different status values
          const getStatusChipProps = (status: string) => {
            switch (status) {
              case "Complete":
                return {
                  color: "success" as const,
                  variant: "filled" as const,
                };
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
            return <span style={{ color: "#999" }}>No data</span>;
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
                height={30}
                plotType="line"
                showHighlight={false}
                showTooltip={false}
                color="#1976d2"
              />
            </Box>
          );
        },
      },
    ],
    [],
  ); // Empty dependency array since columns are static

  // Add a new empty row - optimized with useTransition
  const handleAddRow = useCallback(() => {
    startTransition(() => {
      const newRow = createEmptyRow(nextId);
      setRows((prevRows) => [...prevRows, newRow]);
      setNextId((prevId) => prevId + 1);
      setSnackbarMessage("New row added");
      setSnackbarOpen(true);
    });

    // Scroll to new row after state update
    setTimeout(() => {
      if (apiRef.current) {
        apiRef.current.scrollToIndexes({
          rowIndex: rows.length,
        });
      }
    }, 100);
  }, [nextId, rows.length, apiRef, startTransition]);

  // Delete selected rows - optimized with useTransition
  const handleDeleteRows = useCallback(() => {
    if (selectedRowIds.length === 0) {
      setSnackbarMessage("No rows selected for deletion");
      setSnackbarOpen(true);
      return;
    }

    startTransition(() => {
      const updatedRows = rows.filter(
        (row) => !selectedRowIds.includes(row.id),
      );
      const finalRows =
        updatedRows.length === 0 ? [createEmptyRow(nextId)] : updatedRows;

      setRows(finalRows);
      if (updatedRows.length === 0) {
        setNextId(nextId + 1);
      }
      setSelectedRowIds([]);
      setSnackbarMessage(`${selectedRowIds.length} row(s) deleted`);
      setSnackbarOpen(true);
    });
  }, [selectedRowIds, rows, nextId, startTransition]);

  // Submit acquisition
  const handleSubmitAcquisition = useCallback(() => {
    // Clear any existing focus to prevent aria-hidden issues
    if (
      document.activeElement &&
      document.activeElement instanceof HTMLElement
    ) {
      document.activeElement.blur();
    }
    setDialogOpen(true);
  }, []);

  const handleDialogCancel = useCallback(() => {
    setDialogOpen(false);
    setAcqListName("");
    setAcqListDesc("");
  }, []);

  const handleDialogSubmit = useCallback(async () => {
    startTransition(() => {
      setDialogOpen(false);
      setAcqListName("");
      setAcqListDesc("");
      setAcquisitionSubmitted(true);
    });

    const acqName = acqListName || "Untitled Acquisition";
    const acqDesc = acqListDesc;

    acqNameRef.current = acqName; // Store for later notifications

    // Prepare payload for POST /Acquisition/sequence and store GUIDs for SSE matching
    const samplesWithIds = rows.map((row) => ({
      id: uuidv4(),
      name: row.sampleName || "",
      type: row.sampleType || "",
      methodFilePath: row.instMethod || "",
      rawFilePath: row.rawFileName || "",
      volume: row.injVol || 0,
      position: row.position || "",
      originalRowId: row.id, // Keep reference to original row
    }));

    const sequenceId = uuidv4();
    const payload = {
      id: sequenceId,
      name: acqName,
      description: acqDesc,
      samples: samplesWithIds.map(
        ({ originalRowId: _originalRowId, ...sample }) => sample,
      ),
    };

    try {
      const response = await fetch(API_ENDPOINTS.ACQUISITION_SEQUENCE, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });
      if (!response.ok) {
        throw new Error("Failed to submit acquisition");
      }

      // Register sequence for notifications (this shows submission notification and stores name for completion)
      registerSequence(sequenceId, acqName);

      // Update snackbar for immediate feedback
      const submitMsg = `Acquisition ${acqName} was submitted successfully.`;
      startTransition(() => {
        setSnackbarMessage(submitMsg);
        setSnackbarOpen(true);
      });

      // Update rows with backend GUIDs and set status to Queued
      startTransition(() => {
        setRows((prevRows) => {
          const updatedRows = prevRows.map((row) => {
            const matchingSample = samplesWithIds.find(
              (s) => s.originalRowId === row.id,
            );
            const updatedRow = matchingSample
              ? {
                  ...row,
                  backendSampleId: matchingSample.id,
                  status: "Queued" as const, // Set to Queued after submission
                }
              : row;
            return updatedRow;
          });
          return updatedRows;
        });
      });
    } catch {
      // Error handling is done by the notification system
      const failMsg = `Failed to submit acquisition ${acqName}.`;
      startTransition(() => {
        setSnackbarMessage(failMsg);
        setSnackbarOpen(true);
      });
      addNotification({ message: failMsg, type: "error" });
    }
  }, [
    acqListName,
    acqListDesc,
    rows,
    registerSequence,
    addNotification,
    startTransition,
  ]);

  const handleStopAcquisition = useCallback(() => {
    startTransition(() => {
      setAcquisitionSubmitted(false);
      setSnackbarMessage("Acquisition stopped.");
      setSnackbarOpen(true);
    });
    // Stop GlobalSSE connections if no longer needed
    stopConnection();
  }, [stopConnection, startTransition]);

  // Start/stop SSE connection based on acquisition status
  useEffect(() => {
    if (acquisitionSubmitted && !sseConnected) {
      startConnection();
    } else if (!acquisitionSubmitted && sseConnected) {
      // Don't stop connection immediately, other components might need it
      // The GlobalSSE context will manage disconnection when no active sequences exist
    }
  }, [acquisitionSubmitted, sseConnected, startConnection]);

  // Subscribe to sequence updates from GlobalSSE
  useEffect(() => {
    if (!acquisitionSubmitted) return;

    const unsubscribeSequence = subscribeToSequenceUpdates((message) => {
      const { SampleId, SequenceState, IsError } = message.Data;

      // Map SequenceState to our status values
      const getStatusFromSequenceState = (
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
          case "SampleQueued":
            return "Queued";
          case "SequenceComplete":
          case "SequenceCompletedFilesMoved":
            // Only show notification for sequence-level event (not for every sample)
            if (SampleId === "00000000-0000-0000-0000-000000000000") {
              // Use startTransition for non-blocking updates
              startTransition(() => {
                setAcquisitionSubmitted(false); // Complete the acquisition
                const acqName = acqNameRef.current || "Untitled Acquisition";
                const completeMsg = `Acquisition ${acqName} completed successfully.`;
                setSnackbarMessage(completeMsg);
                setSnackbarOpen(true);
              });
            }
            return "Complete";
          default:
            return null; // Don't update status for unknown states
        }
      };

      const newStatus = getStatusFromSequenceState(SequenceState, IsError);

      // Handle sequence-level events (SequenceComplete)
      if (
        (SequenceState === "SequenceComplete" ||
          SequenceState === "SequenceCompletedFilesMoved") &&
        SampleId === "00000000-0000-0000-0000-000000000000"
      ) {
        // Sequence is complete - don't update individual rows, just set acquisition as complete
        return;
      }

      // Handle sample-level events - use startTransition for non-blocking updates
      if (
        newStatus &&
        SampleId &&
        SampleId !== "00000000-0000-0000-0000-000000000000"
      ) {
        startTransition(() => {
          setRows((prevRows) => {
            const updatedRows = prevRows.map((row) => {
              // Match by backendSampleId (GUID) from the SSE event
              if (row.backendSampleId === SampleId) {
                return {
                  ...row,
                  status: newStatus,
                };
              }
              return row;
            });
            return updatedRows;
          });
        });
      }
    });

    return unsubscribeSequence;
  }, [acquisitionSubmitted, subscribeToSequenceUpdates, startTransition]);

  // Subscribe to sample updates from GlobalSSE
  useEffect(() => {
    if (!acquisitionSubmitted) return;

    const unsubscribeSample = subscribeToSampleUpdates((message) => {
      const { SampleId, AcquisitionState } = message.Data;

      // Map AcquisitionState to our status values
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
            return null; // Don't update status for unknown states
        }
      };

      const newStatus = getStatusFromAcquisitionState(AcquisitionState);

      // Use startTransition for non-blocking updates
      if (
        newStatus &&
        SampleId &&
        SampleId !== "00000000-0000-0000-0000-000000000000"
      ) {
        startTransition(() => {
          setRows((prevRows) => {
            const updatedRows = prevRows.map((row) => {
              if (row.backendSampleId === SampleId) {
                return {
                  ...row,
                  status: newStatus,
                };
              }
              return row;
            });
            return updatedRows;
          });
        });
      }
    });

    return unsubscribeSample;
  }, [acquisitionSubmitted, subscribeToSampleUpdates, startTransition]);

  // Subscribe to sparkline updates from GlobalSSE
  useEffect(() => {
    if (!acquisitionSubmitted) return;

    const unsubscribeSparkline = subscribeToSparklineUpdates((message) => {
      const { sampleId, intensities } = message.data;

      // Use startTransition for non-blocking updates
      if (sampleId && intensities && Array.isArray(intensities)) {
        startTransition(() => {
          setRows((prevRows) => {
            const updatedRows = prevRows.map((row) => {
              // Match by backendSampleId (GUID) from the SSE event
              if (row.backendSampleId === sampleId) {
                return {
                  ...row,
                  sparklineData: intensities,
                };
              }
              return row;
            });
            return updatedRows;
          });
        });
      }
    });

    return unsubscribeSparkline;
  }, [acquisitionSubmitted, subscribeToSparklineUpdates, startTransition]);

  // Optimize row selection handler
  const handleRowSelectionChange = useCallback((newSelectionModel: unknown) => {
    // Extract IDs from the selection model structure
    let selectedIds: GridRowId[] = [];

    if (newSelectionModel && typeof newSelectionModel === "object") {
      // Check if it has the structure {type: 'include', ids: Set(...)}
      if ("ids" in newSelectionModel && newSelectionModel.ids instanceof Set) {
        selectedIds = Array.from(newSelectionModel.ids);
      } else if (
        "ids" in newSelectionModel &&
        Array.isArray(newSelectionModel.ids)
      ) {
        selectedIds = newSelectionModel.ids;
      }
    }

    setSelectedRowIds(selectedIds);
  }, []);

  // Handle closing snackbar
  const handleSnackbarClose = useCallback(() => {
    setSnackbarOpen(false);
  }, []);

  // Handle cell edit
  const processRowUpdate = useCallback(
    (newRow: Sample) => {
      const updatedRows = rows.map((row) =>
        row.id === newRow.id ? newRow : row,
      );
      setRows(updatedRows);
      return newRow;
    },
    [rows],
  );

  // Handle errors during row updates
  const handleProcessRowUpdateError = useCallback((_error: unknown) => {
    setSnackbarMessage("Error updating row");
    setSnackbarOpen(true);
  }, []);

  return (
    <MainLayout>
      <Box
        sx={{
          p: { xs: 2, sm: 3 },
          height: "100%",
          display: "flex",
          flexDirection: "column",
          overflow: "hidden",
        }}
      >
        {/* Acquisition List Name/Description Dialog */}
        <Dialog
          open={dialogOpen}
          onClose={handleDialogCancel}
          maxWidth="xs"
          fullWidth
          disableEscapeKeyDown={false}
          keepMounted={false}
          aria-labelledby="acquisition-dialog-title"
          aria-describedby="acquisition-dialog-description"
        >
          <DialogTitle id="acquisition-dialog-title">
            Submit Acquisition List
          </DialogTitle>
          <DialogContent id="acquisition-dialog-description">
            <TextField
              autoFocus
              margin="dense"
              label="Acquisition List Name"
              type="text"
              fullWidth
              value={acqListName}
              onChange={(e) => setAcqListName(e.target.value)}
              required
            />
            <TextField
              margin="dense"
              label="Description"
              type="text"
              fullWidth
              multiline
              minRows={2}
              value={acqListDesc}
              onChange={(e) => setAcqListDesc(e.target.value)}
            />
          </DialogContent>
          <DialogActions>
            <Button
              onClick={(e) => {
                e.currentTarget.blur();
                startTransition(() => {
                  handleDialogCancel();
                });
              }}
              variant="outlined"
              disabled={isPending}
            >
              Cancel
            </Button>
            <Button
              onClick={(e) => {
                e.currentTarget.blur();
                startTransition(() => {
                  handleDialogSubmit();
                });
              }}
              color="primary"
              variant="contained"
              disabled={!acqListName.trim() || isPending}
            >
              Submit
            </Button>
          </DialogActions>
        </Dialog>
        {/* Header */}
        <Box sx={{ mb: { xs: 2, sm: 3 } }}>
          <Typography
            variant="h4"
            component="h1"
            sx={{
              mb: 1,
              fontWeight: 600,
              fontSize: { xs: "1.5rem", sm: "2rem", md: "2.125rem" },
            }}
          >
            Acquisition
          </Typography>
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{ fontSize: { xs: "0.8rem", sm: "0.875rem" } }}
          >
            Prepare, review, and submit your acquisitions.
          </Typography>
        </Box>

        {/* Action Buttons */}
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={2}
          sx={{
            mb: 2,
            "& .MuiButton-root": {
              minWidth: { xs: "100%", sm: "auto" },
            },
          }}
        >
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={handleAddRow}
          >
            Add Row
          </Button>
          <Button
            variant="outlined"
            color="error"
            startIcon={<DeleteIcon />}
            onClick={handleDeleteRows}
          >
            Delete Rows ({selectedRowIds.length})
          </Button>
          <Tooltip title={getSubmitTooltipMessage}>
            <span>
              <Button
                variant="contained"
                color="success"
                onClick={handleSubmitAcquisition}
                sx={{ ml: { xs: 0, sm: "auto" } }}
                disabled={acquisitionSubmitted || isSubmitDisabled}
              >
                <PlayArrowIcon />
              </Button>
            </span>
          </Tooltip>
          <Tooltip title="Stop Analysis">
            <span>
              <Button
                variant="outlined"
                color="warning"
                onClick={handleStopAcquisition}
                sx={{ ml: { xs: 0, sm: 0 } }}
                disabled={!acquisitionSubmitted}
              >
                <StopIcon />
              </Button>
            </span>
          </Tooltip>
        </Stack>

        {/* Data Grid */}
        <Box
          sx={{
            flexGrow: 1,
            width: "100%",
            minHeight: 0, // Important for flex children
            "& .MuiDataGrid-root": {
              backgroundColor: "#fff",
              border: "1px solid #e0e0e0",
              borderRadius: "8px",
              boxShadow: "0 2px 4px rgba(0,0,0,0.05)",
              marginRight: 0,
              minHeight: "400px",
            },
            "& .MuiDataGrid-cell": {
              padding: "8px",
              "&:focus": {
                outline: "none",
              },
              whiteSpace: "nowrap",
              overflow: "hidden",
              textOverflow: "ellipsis",
            },
            "& .MuiDataGrid-columnHeader": {
              backgroundColor: "#f5f5f5",
              fontWeight: "bold",
              fontSize: "0.875rem",
              "&:focus": {
                outline: "none",
              },
            },
            "& .MuiDataGrid-row:hover": {
              backgroundColor: "rgba(25, 118, 210, 0.08)",
            },
            "& .MuiDataGrid-footerContainer": {
              borderTop: "1px solid #e0e0e0",
              backgroundColor: "#fafafa",
            },
            "& .MuiDataGrid-virtualScroller": {
              overflow: "auto",
            },
          }}
        >
          <DataGrid
            rows={rows}
            columns={columns}
            apiRef={apiRef}
            initialState={{
              pagination: {
                paginationModel: { page: 0, pageSize: 25 },
              },
              density: "standard",
            }}
            pageSizeOptions={[25, 50, 100]}
            checkboxSelection
            disableRowSelectionOnClick
            disableColumnResize={false}
            disableVirtualization={false}
            scrollbarSize={8}
            processRowUpdate={processRowUpdate}
            onProcessRowUpdateError={handleProcessRowUpdateError}
            onRowSelectionModelChange={handleRowSelectionChange}
            editMode="cell"
            columnHeaderHeight={56}
            rowHeight={52}
            density="standard"
            // Performance optimizations
            hideFooterSelectedRowCount
            disableColumnMenu
            sx={{
              "& .MuiDataGrid-cell--editing": {
                bgcolor: "rgb(255,215,115, 0.19)",
                color: "#1a3e72",
              },
              "& .Mui-error": {
                bgcolor: (theme) =>
                  `rgb(126,10,15, ${theme.palette.mode === "dark" ? 0 : 0.1})`,
                color: (theme) =>
                  theme.palette.mode === "dark" ? "#ff4343" : "#750f0f",
              },
              "& .MuiDataGrid-virtualScroller": {
                overflowX: "hidden !important", // Prevent horizontal scroll
              },
              "& .MuiDataGrid-main": {
                overflowX: "hidden !important",
              },
            }}
          />
        </Box>

        {/* Notification Snackbar */}
        <Snackbar
          open={snackbarOpen}
          autoHideDuration={4000}
          onClose={handleSnackbarClose}
          anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
        >
          <Alert
            onClose={handleSnackbarClose}
            severity="success"
            sx={{ width: "100%" }}
          >
            {snackbarMessage}
          </Alert>
        </Snackbar>
      </Box>
    </MainLayout>
  );
};

export default Acquisition;
