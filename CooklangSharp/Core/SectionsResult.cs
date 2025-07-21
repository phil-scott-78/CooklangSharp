using System.Collections.Immutable;
using CooklangSharp.Models;

namespace CooklangSharp.Core;

/// <summary>
/// An internal helper record to pass parsing results for sections.
/// </summary>
internal record SectionsResult(ImmutableList<Section> Sections, ImmutableDictionary<string, object> ClassicMetadata);