using System.ComponentModel.DataAnnotations;

namespace ThermoFisher.SampleOnionModule.Abstractions;

public class SampleOnionModuleConfiguration
{
    public string Option1 { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "Option2 must be between 0 and 100.")]
    public int Option2 { get; set; } = 0;
}
