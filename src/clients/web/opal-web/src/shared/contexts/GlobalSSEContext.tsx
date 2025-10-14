import React, {
  createContext,
  useContext,
  useEffect,
  useRef,
  useState,
  ReactNode,
  useCallback,
} from "react";
import { API_ENDPOINTS } from "../constants/navigation-constants";
import { AcquisitionState, SequenceState } from "../types/enums";

// Types for SSE data structures
export interface SequenceStatus {
  sequenceId: string | null;
  sampleName: string | null;
  sequenceState: number;
  isError: boolean;
}

export interface SampleData {
  id: string;
  name: string | null;
  type: string | null;
  methodFilePath: string | null;
  rawFilePath: string | null;
  volume: number;
  position: string | null;
}

export interface SampleStatus {
  sequenceId: string | null;
  sampleName: string | null;
  acquisitionState: number;
}

export interface SampleInfo {
  sampleData: SampleData;
  sampleStatus: SampleStatus;
}

export interface ApiSequence {
  sequenceId: string;
  name?: string | null;
  description?: string | null;
  sequenceStatus: SequenceStatus;
  samples: SampleInfo[] | null;
}

// SSE Message types
export interface SSESequenceMessage {
  Type: "SequenceStatus";
  Data: {
    SequenceId: string;
    SampleId: string;
    SequenceState: string;
    IsError: boolean;
  };
}

export interface SSESampleMessage {
  Type: "SampleStatus";
  Data: {
    SequenceId: string;
    SampleId: string;
    AcquisitionState: string;
  };
}

export interface SSESparklineMessage {
  type: "SparklineData";
  data: {
    sequenceId: string;
    sampleId: string;
    intensities: number[];
  };
}

export interface SSEChromatogramMessage {
  type: "ChromatogramSvgData";
  data: {
    sequenceId: string;
    sampleId: string;
    startScanNumber: number;
    endScanNumber: number;
    chromatogramSvg: string;
  };
}

// Status mapping utility functions
export const mapSequenceStateToStatus = (
  sequenceState: number,
  isError: boolean,
): "running" | "completed" | "pending" | "error" => {
  if (isError) return "error";

  switch (sequenceState) {
    case SequenceState.SequenceStart:
    case SequenceState.SampleStart:
    case SequenceState.DataFileCreate:
    case SequenceState.MethodValidationOk:
    case SequenceState.InstrumentSentMessage:
      return "running";

    case SequenceState.SequenceComplete:
    case SequenceState.SampleComplete:
    case SequenceState.SequenceCompletedFilesMoved:
    case SequenceState.SequencePausedFilesMoved:
    case SequenceState.SequenceCompletedSomeFilesNotMoved:
      return "completed";

    case SequenceState.DeviceError:
    case SequenceState.BarcodeError:
    case SequenceState.MethodValidationFail:
    case SequenceState.InvalidVial:
    case SequenceState.InvalidInjectionVolume:
    case SequenceState.DataFileCreateFail:
    case SequenceState.MethodDownloadFail:
    case SequenceState.PreAcquisitionProgramRunFail:
    case SequenceState.PostAcquisitionProgramRunFail:
    case SequenceState.MethodStoreFail:
    case SequenceState.ChangeOperatingModeFail:
    case SequenceState.DeviceInitializeFail:
    case SequenceState.StartRunFail:
    case SequenceState.StopRunFail:
    case SequenceState.DeviceDetached:
    case SequenceState.MissingInstrumentMethod:
      return "error";

    case SequenceState.InvalidDeviceList:
    case SequenceState.DiskSpaceLow:
    default:
      return "pending";
  }
};

// Convert string state names to enum values
export const mapSequenceStateStringToNumber = (stateString: string): number => {
  switch (stateString) {
    case "SequenceStart":
      return SequenceState.SequenceStart;
    case "SequenceComplete":
      return SequenceState.SequenceComplete;
    case "SampleStart":
      return SequenceState.SampleStart;
    case "DataFileCreate":
      return SequenceState.DataFileCreate;
    case "SampleComplete":
      return SequenceState.SampleComplete;
    case "InvalidDeviceList":
      return SequenceState.InvalidDeviceList;
    case "DeviceError":
      return SequenceState.DeviceError;
    case "BarcodeError":
      return SequenceState.BarcodeError;
    case "MethodValidationFail":
      return SequenceState.MethodValidationFail;
    case "MethodValidationOk":
      return SequenceState.MethodValidationOk;
    case "InvalidVial":
      return SequenceState.InvalidVial;
    case "InvalidInjectionVolume":
      return SequenceState.InvalidInjectionVolume;
    case "DataFileCreateFail":
      return SequenceState.DataFileCreateFail;
    case "MethodDownloadFail":
      return SequenceState.MethodDownloadFail;
    case "PreAcquisitionProgramRunFail":
      return SequenceState.PreAcquisitionProgramRunFail;
    case "PostAcquisitionProgramRunFail":
      return SequenceState.PostAcquisitionProgramRunFail;
    case "MethodStoreFail":
      return SequenceState.MethodStoreFail;
    case "ChangeOperatingModeFail":
      return SequenceState.ChangeOperatingModeFail;
    case "DeviceInitializeFail":
      return SequenceState.DeviceInitializeFail;
    case "SequenceCompletedFilesMoved":
      return SequenceState.SequenceCompletedFilesMoved;
    case "SequencePausedFilesMoved":
      return SequenceState.SequencePausedFilesMoved;
    case "DiskSpaceLow":
      return SequenceState.DiskSpaceLow;
    case "InstrumentSentMessage":
      return SequenceState.InstrumentSentMessage;
    case "StartRunFail":
      return SequenceState.StartRunFail;
    case "StopRunFail":
      return SequenceState.StopRunFail;
    case "DeviceDetached":
      return SequenceState.DeviceDetached;
    case "MissingInstrumentMethod":
      return SequenceState.MissingInstrumentMethod;
    case "SequenceCompletedSomeFilesNotMoved":
      return SequenceState.SequenceCompletedSomeFilesNotMoved;
    default:
      return 0; // Default to SequenceStart
  }
};

export const mapAcquisitionStateToSampleStatus = (
  acquisitionState: number,
): "Complete" | "Running" | "Pending" | "Error" | "Queued" => {
  switch (acquisitionState) {
    case AcquisitionState.Error:
    case AcquisitionState.WaitBarcodeError:
    case AcquisitionState.Abort:
      return "Error";

    case AcquisitionState.ReadBarcode:
    case AcquisitionState.WaitBarcode:
    case AcquisitionState.SendMethod:
    case AcquisitionState.WaitMethodReady:
    case AcquisitionState.PreAcquisitionProgram:
    case AcquisitionState.WaitPreAcquisitionProgram:
    case AcquisitionState.WaitStartSlaves:
    case AcquisitionState.WaitStartMaster:
    case AcquisitionState.WaitContactClosure:
    case AcquisitionState.Acquire:
    case AcquisitionState.WaitMethodRun:
    case AcquisitionState.PostAcquisitionProgram:
    case AcquisitionState.WaitPostAcquisitionProgram:
      return "Running";

    case AcquisitionState.PostRun:
      return "Complete";

    case AcquisitionState.Ready:
      return "Pending";

    case AcquisitionState.Stopped:
    case AcquisitionState.Load:
    case AcquisitionState.Initialize:
    case AcquisitionState.WaitInitialize:
    case AcquisitionState.WaitReady:
    default:
      return "Queued";
  }
};

// Convert string acquisition state names to enum values
export const mapAcquisitionStateStringToNumber = (
  stateString: string,
): number => {
  switch (stateString) {
    case "Stopped":
      return AcquisitionState.Stopped;
    case "Error":
      return AcquisitionState.Error;
    case "Load":
      return AcquisitionState.Load;
    case "Initialize":
      return AcquisitionState.Initialize;
    case "WaitInitialize":
      return AcquisitionState.WaitInitialize;
    case "WaitReady":
      return AcquisitionState.WaitReady;
    case "Ready":
      return AcquisitionState.Ready;
    case "ReadBarcode":
      return AcquisitionState.ReadBarcode;
    case "WaitBarcode":
      return AcquisitionState.WaitBarcode;
    case "WaitBarcodeError":
      return AcquisitionState.WaitBarcodeError;
    case "SendMethod":
      return AcquisitionState.SendMethod;
    case "WaitMethodReady":
      return AcquisitionState.WaitMethodReady;
    case "PreAcquisitionProgram":
      return AcquisitionState.PreAcquisitionProgram;
    case "WaitPreAcquisitionProgram":
      return AcquisitionState.WaitPreAcquisitionProgram;
    case "WaitStartSlaves":
      return AcquisitionState.WaitStartSlaves;
    case "WaitStartMaster":
      return AcquisitionState.WaitStartMaster;
    case "WaitContactClosure":
      return AcquisitionState.WaitContactClosure;
    case "Acquire":
      return AcquisitionState.Acquire;
    case "WaitMethodRun":
      return AcquisitionState.WaitMethodRun;
    case "PostAcquisitionProgram":
      return AcquisitionState.PostAcquisitionProgram;
    case "WaitPostAcquisitionProgram":
      return AcquisitionState.WaitPostAcquisitionProgram;
    case "PostRun":
      return AcquisitionState.PostRun;
    case "Abort":
      return AcquisitionState.Abort;
    default:
      return AcquisitionState.Stopped; // Default to Stopped
  }
};

// Context type definition
interface GlobalSSEContextType {
  // Connection status
  isConnected: boolean;
  lastUpdateTime: string;
  connectionError: string | null;

  // Data streams
  sequenceUpdates: SSESequenceMessage[];
  sampleUpdates: SSESampleMessage[];
  sparklineUpdates: SSESparklineMessage[];
  chromatogramUpdates: SSEChromatogramMessage[];

  // Connection management
  startConnection: () => void;
  stopConnection: () => void;

  // Subscription management for components
  subscribeToSequenceUpdates: (
    callback: (message: SSESequenceMessage) => void,
  ) => () => void;
  subscribeToSampleUpdates: (
    callback: (message: SSESampleMessage) => void,
  ) => () => void;
  subscribeToSparklineUpdates: (
    callback: (message: SSESparklineMessage) => void,
  ) => () => void;
  subscribeToChromatogramUpdates: (
    callback: (message: SSEChromatogramMessage) => void,
  ) => () => void;

  // Utility functions
  mapSequenceStateToStatus: typeof mapSequenceStateToStatus;
  mapAcquisitionStateToSampleStatus: typeof mapAcquisitionStateToSampleStatus;
  mapSequenceStateStringToNumber: typeof mapSequenceStateStringToNumber;
  mapAcquisitionStateStringToNumber: typeof mapAcquisitionStateStringToNumber;
}

// Create context
const GlobalSSEContext = createContext<GlobalSSEContextType | undefined>(
  undefined,
);

// Custom hook to use the context
export const useGlobalSSE = (): GlobalSSEContextType => {
  const context = useContext(GlobalSSEContext);
  if (!context) {
    throw new Error("useGlobalSSE must be used within a GlobalSSEProvider");
  }
  return context;
};

// Provider component props
interface GlobalSSEProviderProps {
  children: ReactNode;
}

// Provider component
export const GlobalSSEProvider: React.FC<GlobalSSEProviderProps> = ({
  children,
}) => {
  // Connection state
  const [isConnected, setIsConnected] = useState<boolean>(false);
  const [lastUpdateTime, setLastUpdateTime] = useState<string>("");
  const [connectionError, setConnectionError] = useState<string | null>(null);
  const [isConnecting, setIsConnecting] = useState<boolean>(false);

  // Data streams
  const [sequenceUpdates, setSequenceUpdates] = useState<SSESequenceMessage[]>(
    [],
  );
  const [sampleUpdates, setSampleUpdates] = useState<SSESampleMessage[]>([]);
  const [sparklineUpdates, setSparklineUpdates] = useState<
    SSESparklineMessage[]
  >([]);
  const [chromatogramUpdates, setChromatogramUpdates] = useState<
    SSEChromatogramMessage[]
  >([]);

  // Event source refs
  const acquisitionEventSourceRef = useRef<EventSource | null>(null);
  const sparklineEventSourceRef = useRef<EventSource | null>(null);
  const chromatogramEventSourceRef = useRef<EventSource | null>(null);

  // Throttling references
  const lastSequenceUpdateRef = useRef<number>(0);
  const lastSampleUpdateRef = useRef<number>(0);
  const lastSparklineUpdateRef = useRef<number>(0);
  const UPDATE_THROTTLE_MS = 1000; // 1 second throttle
  // ...existing code...

  // Subscription callbacks
  const sequenceCallbacksRef = useRef<
    Set<(message: SSESequenceMessage) => void>
  >(new Set());
  const sampleCallbacksRef = useRef<Set<(message: SSESampleMessage) => void>>(
    new Set(),
  );
  const sparklineCallbacksRef = useRef<
    Set<(message: SSESparklineMessage) => void>
  >(new Set());
  const chromatogramCallbacksRef = useRef<
    Set<(message: SSEChromatogramMessage) => void>
  >(new Set());

  // Subscription management functions
  const subscribeToSequenceUpdates = useCallback(
    (callback: (message: SSESequenceMessage) => void) => {
      sequenceCallbacksRef.current.add(callback);
      return () => {
        sequenceCallbacksRef.current.delete(callback);
      };
    },
    [],
  );

  const subscribeToSampleUpdates = useCallback(
    (callback: (message: SSESampleMessage) => void) => {
      sampleCallbacksRef.current.add(callback);
      return () => {
        sampleCallbacksRef.current.delete(callback);
      };
    },
    [],
  );

  const subscribeToSparklineUpdates = useCallback(
    (callback: (message: SSESparklineMessage) => void) => {
      sparklineCallbacksRef.current.add(callback);
      return () => {
        sparklineCallbacksRef.current.delete(callback);
      };
    },
    [],
  );

  const subscribeToChromatogramUpdates = useCallback(
    (callback: (message: SSEChromatogramMessage) => void) => {
      chromatogramCallbacksRef.current.add(callback);
      return () => {
        chromatogramCallbacksRef.current.delete(callback);
      };
    },
    [],
  );

  // Start all SSE connections
  const startConnection = useCallback(() => {
    if (isConnecting) {
      return;
    }
    setIsConnecting(true);
    if (!acquisitionEventSourceRef.current) {
      try {
        const acquisitionEventSource = new EventSource(
          API_ENDPOINTS.ACQUISITION_SSE,
        );
        acquisitionEventSourceRef.current = acquisitionEventSource;
        acquisitionEventSource.onopen = () => {
          setIsConnected(true);
          setConnectionError(null);
          setIsConnecting(false);
        };
        acquisitionEventSource.onmessage = (event) => {
          try {
            const data = JSON.parse(event.data);
            setLastUpdateTime(new Date().toLocaleTimeString());

            if (data.Type === "SequenceStatus" && data.Data) {
              const now = Date.now();
              const isCompletionEvent =
                data.Data.SequenceState === "SequenceComplete" ||
                data.Data.SequenceState === "SequenceCompletedFilesMoved" ||
                data.Data.SequenceState ===
                  "SequenceCompletedSomeFilesNotMoved";

              // Never throttle completion events - they are critical for notifications
              if (
                isCompletionEvent ||
                now - lastSequenceUpdateRef.current > UPDATE_THROTTLE_MS
              ) {
                if (!isCompletionEvent) {
                  lastSequenceUpdateRef.current = now;
                }
                const message: SSESequenceMessage = data;
                setSequenceUpdates((prev) => [...prev.slice(-49), message]);
                sequenceCallbacksRef.current.forEach((callback) =>
                  callback(message),
                );
              }
            }
            if (data.Type === "SampleStatus" && data.Data) {
              const now = Date.now();
              const isCompletionEvent =
                data.Data.SampleState === "SampleComplete" ||
                data.Data.SampleState === "SampleCompletedFilesMoved" ||
                data.Data.SampleState === "SampleCompletedSomeFilesNotMoved";

              // Never throttle completion events - they are critical for notifications
              if (
                isCompletionEvent ||
                now - lastSampleUpdateRef.current > UPDATE_THROTTLE_MS
              ) {
                if (!isCompletionEvent) {
                  lastSampleUpdateRef.current = now;
                }
                const message: SSESampleMessage = data;
                setSampleUpdates((prev) => [...prev.slice(-49), message]);
                sampleCallbacksRef.current.forEach((callback) =>
                  callback(message),
                );
              }
            }
          } catch (error) {
            console.warn("Failed to parse acquisition event data:", error);
          }
        };
        acquisitionEventSource.onerror = () => {
          setConnectionError("Lost connection to acquisition updates");
          setIsConnected(false);
          setIsConnecting(false);
          if (acquisitionEventSourceRef.current) {
            acquisitionEventSourceRef.current.close();
            acquisitionEventSourceRef.current = null;
          }
          setTimeout(() => {
            if (!acquisitionEventSourceRef.current && !isConnecting) {
              startConnection();
            }
          }, 5000);
        };
      } catch {
        setConnectionError("Failed to connect to acquisition updates");
        setIsConnected(false);
        setIsConnecting(false);
      }
    }
    if (!sparklineEventSourceRef.current) {
      try {
        const sparklineEventSource = new EventSource(
          API_ENDPOINTS.ACQUISITION_SPARKLINE_SSE,
        );
        sparklineEventSourceRef.current = sparklineEventSource;
        sparklineEventSource.onopen = () => {};
        sparklineEventSource.onmessage = (event) => {
          try {
            const data = JSON.parse(event.data);
            if (data.type === "SparklineData" && data.data) {
              const now = Date.now();
              if (now - lastSparklineUpdateRef.current > UPDATE_THROTTLE_MS) {
                lastSparklineUpdateRef.current = now;
                const message: SSESparklineMessage = data;
                setSparklineUpdates((prev) => [...prev.slice(-99), message]);
                sparklineCallbacksRef.current.forEach((callback) =>
                  callback(message),
                );
              }
            }
          } catch (error) {
            console.warn("Failed to parse sparkline event data:", error);
          }
        };
        sparklineEventSource.onerror = () => {
          if (sparklineEventSourceRef.current) {
            sparklineEventSourceRef.current.close();
            sparklineEventSourceRef.current = null;
          }
          setTimeout(() => {
            if (!sparklineEventSourceRef.current) {
              try {
                const newSparklineEventSource = new EventSource(
                  API_ENDPOINTS.ACQUISITION_SPARKLINE_SSE,
                );
                sparklineEventSourceRef.current = newSparklineEventSource;
                newSparklineEventSource.onopen = () => {};
                newSparklineEventSource.onmessage =
                  sparklineEventSource.onmessage;
                newSparklineEventSource.onerror = sparklineEventSource.onerror;
              } catch (error) {
                console.warn("Failed to reconnect sparkline SSE:", error);
              }
            }
          }, 5000);
        };
      } catch (error) {
        console.warn("Failed to setup sparkline SSE connection:", error);
      }
    }
    if (!chromatogramEventSourceRef.current) {
      try {
        const chromatogramEventSource = new EventSource(
          API_ENDPOINTS.ACQUISITION_CHROMATOGRAM_SSE,
        );
        chromatogramEventSourceRef.current = chromatogramEventSource;
        chromatogramEventSource.onopen = () => {};
        chromatogramEventSource.onmessage = (event) => {
          try {
            let data;
            try {
              data = JSON.parse(event.data);
            } catch (error) {
              console.warn("Failed to parse JSON:", error);
              const fixedData = event.data;
              const typeMatch = fixedData.match(/"type":\s*"([^"]+)"/);
              const sequenceIdMatch = fixedData.match(
                /"sequenceId":\s*"([^"]+)"/,
              );
              const sampleIdMatch = fixedData.match(/"sampleId":\s*"([^"]+)"/);
              const svgStartIndex = fixedData.indexOf('"chromatogramSvg":');
              if (svgStartIndex !== -1) {
                const svgValueStart =
                  fixedData.indexOf('"', svgStartIndex + 18) + 1;
                const remainingData = fixedData.substring(svgValueStart);
                const svgEndPattern = "</svg>";
                const svgEndIndex = remainingData.lastIndexOf(svgEndPattern);
                if (svgEndIndex !== -1) {
                  const svgContent = remainingData.substring(
                    0,
                    svgEndIndex + svgEndPattern.length,
                  );
                  const startScanMatch = fixedData.match(
                    /"startScanNumber":\s*(\d+)/,
                  );
                  const endScanMatch = fixedData.match(
                    /"endScanNumber":\s*(\d+)/,
                  );
                  data = {
                    type: typeMatch ? typeMatch[1] : "ChromatogramSvgData",
                    data: {
                      sequenceId: sequenceIdMatch ? sequenceIdMatch[1] : "",
                      sampleId: sampleIdMatch ? sampleIdMatch[1] : "",
                      startScanNumber: startScanMatch
                        ? parseInt(startScanMatch[1])
                        : 1,
                      endScanNumber: endScanMatch
                        ? parseInt(endScanMatch[1])
                        : 22,
                      chromatogramSvg: svgContent,
                    },
                  };
                } else {
                  return;
                }
              } else {
                return;
              }
            }
            if (data.type === "ChromatogramSvgData" && data.data) {
              const message: SSEChromatogramMessage = data;
              setChromatogramUpdates((prev) => [...prev.slice(-49), message]);
              chromatogramCallbacksRef.current.forEach((callback) =>
                callback(message),
              );
            }
          } catch (error) {
            console.warn("Failed to parse chromatogram event data:", error);
          }
        };
        chromatogramEventSource.onerror = () => {
          if (chromatogramEventSourceRef.current) {
            chromatogramEventSourceRef.current.close();
            chromatogramEventSourceRef.current = null;
          }
          setTimeout(() => {
            if (!chromatogramEventSourceRef.current) {
              try {
                const newChromatogramEventSource = new EventSource(
                  API_ENDPOINTS.ACQUISITION_CHROMATOGRAM_SSE,
                );
                chromatogramEventSourceRef.current = newChromatogramEventSource;
                newChromatogramEventSource.onopen = () => {};
                newChromatogramEventSource.onmessage =
                  chromatogramEventSource.onmessage;
                newChromatogramEventSource.onerror =
                  chromatogramEventSource.onerror;
              } catch (error) {
                console.warn("Failed to reconnect chromatogram SSE:", error);
              }
            }
          }, 5000);
        };
      } catch (error) {
        console.warn("Failed to setup chromatogram SSE connection:", error);
      }
    }
  }, [isConnecting]);

  // Stop all SSE connections
  const stopConnection = useCallback(() => {
    if (acquisitionEventSourceRef.current) {
      acquisitionEventSourceRef.current.close();
      acquisitionEventSourceRef.current = null;
    }

    if (sparklineEventSourceRef.current) {
      sparklineEventSourceRef.current.close();
      sparklineEventSourceRef.current = null;
    }

    if (chromatogramEventSourceRef.current) {
      chromatogramEventSourceRef.current.close();
      chromatogramEventSourceRef.current = null;
    }

    // ...existing code...

    setIsConnected(false);
    setConnectionError(null);
    setIsConnecting(false);
  }, []); // Empty dependency array since this function should be stable

  // Connection health check - monitor for stale connections
  useEffect(() => {
    const healthCheckInterval = setInterval(() => {
      if (
        acquisitionEventSourceRef.current?.readyState === EventSource.CLOSED
      ) {
        acquisitionEventSourceRef.current = null;
        if (!isConnecting) {
          startConnection();
        }
      }
      if (sparklineEventSourceRef.current?.readyState === EventSource.CLOSED) {
        sparklineEventSourceRef.current = null;
      }
      if (
        chromatogramEventSourceRef.current?.readyState === EventSource.CLOSED
      ) {
        chromatogramEventSourceRef.current = null;
      }
    }, 30000);
    return () => clearInterval(healthCheckInterval);
  }, [isConnecting, startConnection]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      stopConnection();
    };
  }, [stopConnection]);

  // Auto-start connection when provider mounts
  useEffect(() => {
    startConnection();
  }, [startConnection]);

  const contextValue: GlobalSSEContextType = {
    // Connection status
    isConnected,
    lastUpdateTime,
    connectionError,

    // Data streams
    sequenceUpdates,
    sampleUpdates,
    sparklineUpdates,
    chromatogramUpdates,

    // Connection management
    startConnection,
    stopConnection,

    // Subscription management
    subscribeToSequenceUpdates,
    subscribeToSampleUpdates,
    subscribeToSparklineUpdates,
    subscribeToChromatogramUpdates,

    // Utility functions
    mapSequenceStateToStatus,
    mapAcquisitionStateToSampleStatus,
    mapSequenceStateStringToNumber,
    mapAcquisitionStateStringToNumber,
  };

  return (
    <GlobalSSEContext.Provider value={contextValue}>
      {children}
    </GlobalSSEContext.Provider>
  );
};

export default GlobalSSEContext;
