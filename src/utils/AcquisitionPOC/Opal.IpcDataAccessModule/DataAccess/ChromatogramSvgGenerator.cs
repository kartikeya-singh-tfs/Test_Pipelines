using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.PeakDetect;
using ThermoFisher.CommonCore.PlotCore;
using ThermoFisher.CommonCore.PlotCore.Plotters;
using ThermoFisher.CommonCore.PlotCore.Renderers;

namespace Thermofisher.Opal.IpcDataAccessModule.DataAccess
{
    public class CrudeTextMetricProvider : ITextMetricProvider
    {
        private const double CharWidthScaleEst = 1.4;

        // convert char point size from "em" sent int to estimated "line height", used by canvas
        private const double PointToLineHeight = 1.5;

        /// <inheritdoc />
        public PlotSize MeasureString(string text, PlotFont font, double angle, bool exactMeasure)
        {
            if (text.Length == 1 && text[0] >= 'A' && text[0] <= 'Z')
            {
                // measure single upper case letter, larger estimate
                return new PlotSize(text.Length * font.Size, font.Size * PointToLineHeight);
            }

            // "_" etc. is extra large, count it
            int large = text.Count(c => c == '_' || c == '#');
            return new PlotSize(
                (text.Length - large) * font.Size / CharWidthScaleEst + large * font.Size,
                font.Size * PointToLineHeight
            );
        }
    }

    // This class is responsible for generating SVG representations of chromatogram data for DEMO.
    internal class ChromatogramSvgGenerator
    {
        // Logger for diagnostic output.
        private readonly ILogger<ChromatogramSvgGenerator> _logger;

        // Constructor that initializes the logger using the provided logger factory.
        public ChromatogramSvgGenerator(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger<ChromatogramSvgGenerator>();
        }

        /// <summary>
        /// Generates an SVG string representing the chromatogram plot for the specified spectra range.
        /// </summary>
        /// <param name="dataAccessContext">Context containing acquisition sample and related data.</param>
        /// <param name="opalSeqId">The Opal sequence identifier.</param>
        /// <param name="detectorReader">The detector reader for accessing chromatogram data.</param>
        /// <param name="startSpectrum">The starting spectrum number.</param>
        /// <param name="endSpectrum">The ending spectrum number.</param>
        /// <returns>SVG string of the chromatogram plot.</returns>
        public string GetRealtimePlotDataSvg(
            DataAccessContext dataAccessContext,
            string opalSeqId,
            IDetectorReader detectorReader,
            int startSpectrum,
            int endSpectrum
        )
        {
            // Retrieve run header and sample information.
            var runHeader = detectorReader.RunHeaderEx;
            var rawDataUri = dataAccessContext.AcqSample.RawFileNameFull;
            var sampleId = dataAccessContext.AcqSample.SampleId;

            // Create chromatogram signals for the specified spectra range.
            var chromSignals = CreateChromatogramSignals(
                detectorReader,
                startSpectrum,
                endSpectrum
            );

            // Define the plot range based on the expected run time.
            var range = new Range(0.0, runHeader.ExpectedRunTime);

            // Use the first chromatogram signal for plotting.
            var first = chromSignals[0];

            // Detect peaks in the chromatogram signal.
            ICIS icis = new ICIS();
            var peaks = icis.DetectPeaks(first);

            // Add suitability results to the detected peaks.
            var peaksWithSuitability = AddSuitabilityResults(peaks, first);

            // Set up the chromatogram plotter and configure labeling.
            var plotter = new ChromatogramPlotter();
            var labeller = plotter.Labeller;

            labeller.LabelArea = true;
            labeller.LabelHeight = true;

            plotter.HideAutoLabelsWhenPeaksAreDetected = true;
            plotter.RetentionTimeRange = new DoubleRange(0.0, runHeader.ExpectedRunTime);

            // Create the chromatogram plot from the signal and peaks.
            plotter.CreateChromatogramFromSignalAndPeaks(
                first,
                range,
                peaksWithSuitability,
                rawDataUri,
                "",
                ""
            );

            // Use a crude text metric provider for SVG rendering.
            var clientRenderer = new CrudeTextMetricProvider();
            var svgPlotData = string.Empty;

            // Render the plot to SVG using a memory stream.
            using (var memStream = new MemoryStream())
            {
                plotter.RenderSvg(memStream, 800, 600, clientRenderer);

                memStream.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(memStream))
                {
                    svgPlotData = sr.ReadToEnd();
                }
            }

            return svgPlotData;
        }

        /// <summary>
        /// Add suitability results to each detected peak.
        /// </summary>
        /// <param name="detectedPeaks">The detected peaks.</param>
        /// <param name="signal">The chromatogram signal.</param>
        /// <returns>Enumerable of peaks with added suitability results.</returns>
        private IEnumerable<PeakWithSuitability> AddSuitabilityResults(
            IEnumerable<IPeakAccess> detectedPeaks,
            Signal signal
        )
        {
            List<PeakWithSuitability> peaks = new List<PeakWithSuitability>();
            foreach (IPeakAccess peakAccess in detectedPeaks)
            {
                peaks.Add(ApplySuitability(peakAccess, signal));
            }

            return peaks;
        }

        /// <summary>
        /// Apply system suitability tests to a peak.
        /// </summary>
        /// <param name="peakAccess">The peak access object.</param>
        /// <param name="signal">The chromatogram signal.</param>
        /// <returns>PeakWithSuitability containing the test results.</returns>
        private PeakWithSuitability ApplySuitability(IPeakAccess peakAccess, Signal signal)
        {
            const double SearchWindow = 1.0;
            var settings = new SystemSuitabilitySettings()
            {
                EnablePeakClassificationChecks = true,
                EnableResolutionChecks = true,
                EnableSymmetryChecks = true,
            };

            // Run default system suitability tests.
            var results = SystemSuitability.RunTests(settings, signal, peakAccess, SearchWindow);
            return new PeakWithSuitability(peakAccess, results);
        }

        /// <summary>
        /// Create chromatogram signals for the specified spectra range.
        /// </summary>
        /// <param name="detectorReader">The detector reader.</param>
        /// <param name="startSpectrum">The starting spectrum number.</param>
        /// <param name="endSpectrum">The ending spectrum number.</param>
        /// <returns>Array of ChromatogramSignal objects.</returns>
        private ChromatogramSignal[] CreateChromatogramSignals(
            IDetectorReader detectorReader,
            int startSpectrum,
            int endSpectrum
        )
        {
            // Get chromatogram data, TIC (Total Ion Chromatogram).
            IChromatogramSettings[] settings = { new ChromatogramTraceSettings() };
            var chromatogramData = detectorReader.GetChromatogramData(
                settings,
                startSpectrum,
                endSpectrum
            );

            var chromSignals = ChromatogramSignal.FromChromatogramData(chromatogramData);

            return chromSignals;
        }
    }
}
