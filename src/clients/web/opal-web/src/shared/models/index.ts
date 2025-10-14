export interface Sample {
  id: number;
  sampleType: "Standard" | "Unknown" | "QC" | "Blank" | "";
  rawFileName: string;
  sampleName: string;
  instMethod: string;
  position: string;
  injVol: number | null;
  status: "Pending" | "Running" | "Complete" | "Error" | "Queued";
  sparklineData: number[];
  backendSampleId?: string; // GUID from backend for SSE matching
}
