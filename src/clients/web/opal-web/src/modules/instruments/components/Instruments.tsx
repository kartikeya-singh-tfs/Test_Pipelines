import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
  Box,
  Typography,
  Card,
  CardContent,
  Chip,
  IconButton,
  Menu,
  MenuItem,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  SelectChangeEvent,
} from "@mui/material";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import MemoryIcon from "@mui/icons-material/Memory";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import StopIcon from "@mui/icons-material/Stop";
import SettingsIcon from "@mui/icons-material/Settings";
import AddIcon from "@mui/icons-material/Add";
import MainLayout from "../../../shared/components/MainLayout";
import {
  useGlobalSSE,
  mapAcquisitionStateToSampleStatus,
} from "../../../shared/contexts/GlobalSSEContext";
import { API_ENDPOINTS } from "../../../shared/constants/navigation-constants";
import { SequenceState } from "../../../shared/types/enums";

// Types for instrument data
interface InstrumentData {
  id: string;
  name: string;
  type: string;
  status: "online" | "offline" | "running" | "error" | "maintenance";
  lastSeen: string;
  currentMethod?: string;
  samplesCompleted?: number;
  totalSamples?: number;
  ipAddress?: string;
  serialNumber?: string;
}

// Mock instrument data - status will be updated dynamically for MS Simulator
const getInitialInstruments = (): InstrumentData[] => [
  {
    id: "inst-000",
    name: "MS Simulator",
    type: "Simulator",
    status: "online", // Will be updated dynamically
    lastSeen: "1 minute ago",
    ipAddress: "127.0.0.1",
    serialNumber: "SIM-2024-001",
  },
  {
    id: "inst-001",
    name: "TSQ 9610 Triple Quadrupole",
    type: "GC-MS",
    status: "online",
    lastSeen: "2 minutes ago",
    ipAddress: "192.168.1.101",
    serialNumber: "GCMS-2024-001",
  },
  {
    id: "inst-002",
    name: "Orbitrap Ascend",
    type: "LC-MS",
    status: "online",
    lastSeen: "1 minute ago",
    ipAddress: "192.168.1.102",
    serialNumber: "LCMS-2024-001",
  },
  {
    id: "inst-003",
    name: "Vanquish Horizon",
    type: "HPLC",
    status: "offline",
    lastSeen: "15 minutes ago",
    ipAddress: "192.168.1.103",
    serialNumber: "HPLC-2024-002",
  },
  {
    id: "inst-004",
    name: "Orbitrap IQ-X",
    type: "LC-MS",
    status: "error",
    lastSeen: "5 minutes ago",
    ipAddress: "192.168.1.104",
    serialNumber: "LCMS-2024-002",
  },
];

const Instruments: React.FC = () => {
  const navigate = useNavigate();
  const { startConnection, sequenceUpdates, sampleUpdates } = useGlobalSSE();
  const [instruments, setInstruments] = useState<InstrumentData[]>(
    getInitialInstruments,
  );
  const [currentSequence, setCurrentSequence] = useState<unknown>(null);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [addDialogOpen, setAddDialogOpen] = useState(false);
  const [newInstrument, setNewInstrument] = useState({
    name: "",
    type: "",
    ipAddress: "",
    serialNumber: "",
  });

  // Start SSE connection and fetch current sequence on mount
  useEffect(() => {
    startConnection();
    fetchCurrentSequence();
    const interval = setInterval(fetchCurrentSequence, 5000); // Refresh every 5 seconds
    return () => clearInterval(interval);
  }, [startConnection]);

  // Update MS Simulator status based on current acquisition activity
  useEffect(() => {
    const hasActiveAcquisition =
      currentSequence &&
      (
        (currentSequence as Record<string, unknown>).sequenceStatus as Record<
          string,
          unknown
        >
      )?.sequenceState !== SequenceState.SequenceComplete &&
      (
        (currentSequence as Record<string, unknown>).sequenceStatus as Record<
          string,
          unknown
        >
      )?.sequenceState !== SequenceState.DeviceError &&
      (
        (currentSequence as Record<string, unknown>).sequenceStatus as Record<
          string,
          unknown
        >
      )?.sequenceState !== SequenceState.SequenceCompletedFilesMoved;
    setInstruments((prevInstruments) =>
      prevInstruments.map((instrument) => {
        if (instrument.id === "inst-000") {
          // MS Simulator
          return {
            ...instrument,
            status: hasActiveAcquisition ? "running" : "online",
            currentMethod: hasActiveAcquisition
              ? ((currentSequence as Record<string, unknown>)
                  ?.name as string) || "Active Sequence"
              : undefined,
            samplesCompleted:
              hasActiveAcquisition &&
              (currentSequence as Record<string, unknown>)?.samples
                ? (
                    (currentSequence as Record<string, unknown>)
                      .samples as Array<Record<string, unknown>>
                  ).filter((s: Record<string, unknown>) => {
                    const acquisitionState = (
                      s.sampleStatus as Record<string, unknown>
                    )?.acquisitionState as number;
                    const statusString =
                      mapAcquisitionStateToSampleStatus(acquisitionState);
                    return statusString === "Complete";
                  }).length
                : undefined,
            totalSamples:
              hasActiveAcquisition &&
              (currentSequence as Record<string, unknown>)?.samples
                ? (
                    (currentSequence as Record<string, unknown>)
                      .samples as Array<Record<string, unknown>>
                  ).length
                : undefined,
          };
        }
        return instrument;
      }),
    );
  }, [currentSequence, sequenceUpdates, sampleUpdates]);

  // Fetch current sequence details from API
  const fetchCurrentSequence = async () => {
    try {
      const response = await fetch(API_ENDPOINTS.ACQUISITION_SEQUENCE);
      if (response.ok) {
        const sequences = await response.json();
        // Find the currently running sequence - exclude completed states
        const runningSequence = sequences.find(
          (seq: Record<string, unknown>) => {
            const sequenceState = (
              seq.sequenceStatus as Record<string, unknown>
            )?.sequenceState as number;
            return (
              sequenceState !== undefined &&
              sequenceState !== SequenceState.SequenceComplete &&
              sequenceState !== SequenceState.SequenceCompletedFilesMoved &&
              sequenceState !== SequenceState.SequenceCompletedSomeFilesNotMoved
            );
          },
        );

        setCurrentSequence(runningSequence || null);
      }
    } catch {
      // Error handling would go here
      setCurrentSequence(null);
    }
  };

  const handleMenuOpen = (
    event: React.MouseEvent<HTMLElement>,
    _instrumentId: string,
  ) => {
    event.stopPropagation();
    setAnchorEl(event.currentTarget);
    // setSelectedInstrument(instrumentId); // Commented out - unused variable
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
    // setSelectedInstrument(null); // Commented out - unused variable
  };

  const handleAddInstrument = () => {
    setAddDialogOpen(true);
  };

  const handleDialogClose = () => {
    setAddDialogOpen(false);
    setNewInstrument({
      name: "",
      type: "",
      ipAddress: "",
      serialNumber: "",
    });
  };

  const handleInputChange =
    (field: string) => (event: React.ChangeEvent<HTMLInputElement>) => {
      setNewInstrument((prev) => ({
        ...prev,
        [field]: event.target.value,
      }));
    };

  const handleSelectChange = (event: SelectChangeEvent) => {
    setNewInstrument((prev) => ({
      ...prev,
      type: event.target.value,
    }));
  };

  const handleInstrumentClick = (instrumentId: string) => {
    navigate(`/instruments/${instrumentId}`);
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

  const renderInstrumentCard = (instrument: InstrumentData) => (
    <Box
      key={instrument.id}
      sx={{
        width: { xs: "100%", sm: "50%", md: "33.333%", lg: "25%" },
        p: 1.5,
      }}
    >
      <Card
        sx={{
          height: "100%",
          cursor: "pointer",
          transition: "all 0.2s ease-in-out",
          "&:hover": {
            transform: "translateY(-2px)",
            boxShadow: 3,
          },
        }}
        onClick={() => handleInstrumentClick(instrument.id)}
      >
        <CardContent>
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "flex-start",
              mb: 2,
            }}
          >
            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
              <MemoryIcon sx={{ color: "primary.main", fontSize: 24 }} />
              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                {instrument.name}
              </Typography>
            </Box>
            <IconButton
              size="small"
              onClick={(e) => handleMenuOpen(e, instrument.id)}
              sx={{ opacity: 0.7, "&:hover": { opacity: 1 } }}
            >
              <MoreVertIcon fontSize="small" />
            </IconButton>
          </Box>

          <Box sx={{ mb: 2 }}>
            <Chip
              label={getStatusText(instrument.status)}
              color={getStatusColor(instrument.status)}
              size="small"
              sx={{ mb: 1 }}
            />
            <Typography variant="body2" color="text.secondary">
              Type: {instrument.type}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Last seen: {instrument.lastSeen}
            </Typography>
            {instrument.serialNumber && (
              <Typography variant="body2" color="text.secondary">
                S/N: {instrument.serialNumber}
              </Typography>
            )}
          </Box>

          {instrument.status === "running" && instrument.currentMethod && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="body2" sx={{ fontWeight: 500, mb: 1 }}>
                Current Method: {instrument.currentMethod}
              </Typography>
              {instrument.samplesCompleted !== undefined &&
                instrument.totalSamples && (
                  <Typography variant="body2" color="text.secondary">
                    Progress: {instrument.samplesCompleted}/
                    {instrument.totalSamples} samples
                  </Typography>
                )}
            </Box>
          )}
        </CardContent>
      </Card>
    </Box>
  );

  return (
    <MainLayout>
      <Box sx={{ p: 3 }}>
        {/* Page Header */}
        <Box
          sx={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            mb: 4,
          }}
        >
          <Box>
            <Typography
              variant="h4"
              component="h1"
              sx={{ mb: 1, fontWeight: 600 }}
            >
              Instruments
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Manage, schedule, and monitor data acquisition of all connected
              instruments
            </Typography>
          </Box>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleAddInstrument}
            sx={{ ml: 2 }}
          >
            Add Instrument
          </Button>
        </Box>

        {/* Instruments Grid */}
        <Box sx={{ display: "flex", flexWrap: "wrap", margin: -1.5 }}>
          {instruments.map(renderInstrumentCard)}
        </Box>

        {/* Context Menu */}
        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
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
          <MenuItem onClick={handleMenuClose}>
            <PlayArrowIcon sx={{ mr: 1, fontSize: 20 }} />
            Start Acquisition
          </MenuItem>
          <MenuItem onClick={handleMenuClose}>
            <StopIcon sx={{ mr: 1, fontSize: 20 }} />
            Stop Acquisition
          </MenuItem>
          <MenuItem onClick={handleMenuClose}>
            <SettingsIcon sx={{ mr: 1, fontSize: 20 }} />
            Configure
          </MenuItem>
        </Menu>

        {/* Add Instrument Dialog */}
        <Dialog
          open={addDialogOpen}
          onClose={handleDialogClose}
          maxWidth="sm"
          fullWidth
        >
          <DialogTitle>Add New Instrument</DialogTitle>
          <DialogContent>
            <Box sx={{ pt: 1 }}>
              <TextField
                fullWidth
                label="Instrument Name"
                value={newInstrument.name}
                onChange={handleInputChange("name")}
                sx={{ mb: 2 }}
              />

              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>Instrument Type</InputLabel>
                <Select
                  value={newInstrument.type}
                  label="Instrument Type"
                  onChange={handleSelectChange}
                >
                  <MenuItem value="Simulator">Simulator</MenuItem>
                  <MenuItem value="LC-MS">LC-MS</MenuItem>
                  <MenuItem value="GC-MS">GC-MS</MenuItem>
                  <MenuItem value="HPLC">HPLC</MenuItem>
                  <MenuItem value="GC">GC</MenuItem>
                  <MenuItem value="MS">MS</MenuItem>
                </Select>
              </FormControl>

              <TextField
                fullWidth
                label="IP Address"
                value={newInstrument.ipAddress}
                onChange={handleInputChange("ipAddress")}
                sx={{ mb: 2 }}
              />

              <TextField
                fullWidth
                label="Serial Number"
                value={newInstrument.serialNumber}
                onChange={handleInputChange("serialNumber")}
              />
            </Box>
          </DialogContent>
          <DialogActions>
            <Button onClick={handleDialogClose}>Cancel</Button>
            <Button variant="contained" onClick={handleDialogClose}>
              Add Instrument
            </Button>
          </DialogActions>
        </Dialog>
      </Box>
    </MainLayout>
  );
};

export default Instruments;
