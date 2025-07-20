using CooklangSharp.Models;

namespace CooklangSharp.Core;

/// <summary>
/// An internal helper record to pass parsing results for sections.
/// </summary>
internal record SectionsResult(List<Section> Sections, Dictionary<string, object> ClassicMetadata);