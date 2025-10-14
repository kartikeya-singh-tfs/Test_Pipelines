namespace ThermoFisher.AcquisitionModule.Abstractions
{
    public class SequenceData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<SampleData> Samples { get; set; } = new List<SampleData>();
    }

    public class SampleData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string MethodFilePath { get; set; }
        public string RawFilePath { get; set; }
        public int Volume { get; set; }
        public string Position { get; set; }
    }

    public class SequenceStatus
    {
        public string SequenceId { get; set; }
        public string SampleId { get; set; }
        public SequenceState SequenceState { get; set; }
        public bool IsError { get; set; }
    }

    public class SampleStatus
    {
        public string SequenceId { get; set; }
        public string SampleId { get; set; }
        public AcquisitionState AcquisitionState { get; set; }
    }

    public enum AcquisitionState
    {
        Stopped = 0,
        Error = 1,
        Load = 2,
        Initialize = 3,
        WaitInitialize = 4,
        WaitReady = 5,
        Ready = 6,
        ReadBarcode = 7,
        WaitBarcode = 8,
        WaitBarcodeError = 9,
        SendMethod = 10,
        WaitMethodReady = 11,
        PreAcquisitionProgram = 12,
        WaitPreAcquisitionProgram = 13,
        WaitStartSlaves = 14,
        WaitStartMaster = 15,
        WaitContactClosure = 16,
        Acquire = 17,
        WaitMethodRun = 18,
        PostAcquisitionProgram = 19,
        WaitPostAcquisitionProgram = 20,
        PostRun = 21,
        Abort = 22,
    }

    public enum SequenceState
    {
        SequenceStart = 0,
        SequenceComplete = 1,
        SampleStart = 2,
        DataFileCreate = 3,
        SampleComplete = 4,
        InvalidDeviceList = 5,
        DeviceError = 6,
        BarcodeError = 7,
        MethodValidationFail = 8,
        MethodValidationOk = 9,
        InvalidVial = 10,
        InvalidInjectionVolume = 11,
        DataFileCreateFail = 12,
        MethodDownloadFail = 13,
        PreAcquisitionProgramRunFail = 14,
        PostAcquisitionProgramRunFail = 15,
        MethodStoreFail = 16,
        ChangeOperatingModeFail = 17,
        DeviceInitializeFail = 18,
        SequenceCompletedFilesMoved = 19,
        SequencePausedFilesMoved = 20,
        DiskSpaceLow = 21,
        InstrumentSentMessage = 22,
        StartRunFail = 23,
        StopRunFail = 24,
        DeviceDetached = 25,
        MissingInstrumentMethod = 26,
        SequenceCompletedSomeFilesNotMoved = 27,
    }

    public class Sequence
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SequenceStatus SequenceStatus { get; set; } = new SequenceStatus();
        public List<Sample> Samples { get; set; } = new List<Sample>();
    }

    public class Sample
    {
        public SampleData SampleData { get; set; } = new SampleData();
        public SampleStatus SampleStatus { get; set; } = new SampleStatus();
    }
}
