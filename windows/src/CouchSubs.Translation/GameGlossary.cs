// SPDX-License-Identifier: AGPL-3.0-only

using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace CouchSubs.Translation;

/// <summary>
/// Oyuna özel terim sözlüğü: oynanan oyun sözlüğü olan bir oyunsa yapay zekâ isteğine eklenir; adlar korunur, hitap,
/// küfür ve arayüz yazıları her satırda aynı çevrilir. Bugün yalnız GTA 6 (<c>tools/packs/glossary/gta6.json</c>,
/// gömülü kaynak). Oyun bilinmiyorsa ya da sözlüğü yoksa istem değişmez.
/// </summary>
public static partial class GameGlossary
{
    // "Grand Theft Auto VI", "GTA VI", "GTA 6", "GTA6"; "Grand Theft Auto V" eşleşmez.
    [GeneratedRegex(@"\b(grand\s+theft\s+auto\s*(vi|6)|gta\s*(vi|6))\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Gta6Pattern();

    private static readonly Lazy<string?> Gta6 = new(() => Build("glossary/gta6.json"));

    /// <summary>İsteğe eklenecek sözlük metni; oyunun sözlüğü yoksa null.</summary>
    public static string? For(string? game) =>
        !string.IsNullOrWhiteSpace(game) && Gta6Pattern().IsMatch(game) ? Gta6.Value : null;

    private static readonly string[] Sections = ["address", "insults", "crime", "game_ui", "phrases", "places"];

    private static string? Build(string resource)
    {
        using var stream = typeof(GameGlossary).Assembly.GetManifestResourceStream(resource);
        if (stream is null) return null;
        var root = JsonNode.Parse(stream)?.AsObject();
        if (root is null) return null;
        return Format(root);
    }

    /// <summary>Sözlük JSON'undan istem metni (testler için ayrı).</summary>
    internal static string Format(JsonObject root)
    {
        var text = new StringBuilder();
        text.Append("Glossary for this game. Use it consistently in every line:");
        if (root["register"]?.GetValue<string>() is { Length: > 0 } register)
            text.Append("\nStyle (in Turkish): ").Append(register);
        if (root["keep"] is JsonArray keep && keep.Count > 0)
            text.Append("\nKeep exactly as written, never translate: ")
                .Append(string.Join(", ", keep.Select(k => k?.GetValue<string>()).OfType<string>()));
        var terms = new List<string>();
        foreach (var section in Sections)
        {
            if (root[section] is not JsonArray entries) continue;
            foreach (var entry in entries.OfType<JsonObject>())
            {
                if (entry["en"]?.GetValue<string>() is not { Length: > 0 } en ||
                    entry["tr"]?.GetValue<string>() is not { Length: > 0 } tr) continue;
                var note = entry["note"]?.GetValue<string>();
                terms.Add(string.IsNullOrEmpty(note) ? $"{en} → {tr}" : $"{en} → {tr} ({note})");
            }
        }
        if (terms.Count > 0)
            text.Append("\nPreferred Turkish (adapt the form to the sentence): ").Append(string.Join("; ", terms));
        return text.ToString();
    }
}
