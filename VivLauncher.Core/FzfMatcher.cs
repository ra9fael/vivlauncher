using System.Globalization;
using System.Text;

namespace VivLauncher.Core;

public sealed record FzfResult(int Score, IReadOnlyCollection<int> Positions);

public static class FzfMatcher
{
    private sealed record Term(string Text, int Type, bool Inverse, bool Sensitive, bool Normalize);
    private static readonly Dictionary<char, char> Normalization = FzfNormalization.Create();
    private static char Normalize(char character)
    {
        return Normalization.TryGetValue(character, out var normalized) ? normalized : character;
    }
    private static int Class(char character)
    {
        if (char.IsLower(character)) return 1;
        if (char.IsUpper(character)) return 2;
        if (char.IsNumber(character)) return 4;
        return char.IsLetter(character) ? 3 : 0;
    }
    private static int Bonus(int previous, int current) => previous == 0 && current != 0 ? 8
        : previous == 1 && current == 2 || previous != 4 && current == 4 ? 7 : current == 0 ? 8 : 0;
    private static int BonusAt(string text, int index) => index == 0 ? 8 : Bonus(Class(text[index - 1]), Class(text[index]));
    private static char Fold(char character, Term term) => term.Normalize
        ? Normalize(term.Sensitive ? character : char.ToLowerInvariant(character))
        : term.Sensitive ? character : char.ToLowerInvariant(character);

    private static List<List<Term>> Parse(string query)
    {
        query = query.TrimStart();
        var trimmed = query.TrimEnd();
        query = trimmed.EndsWith('\\') && query.Length > trimmed.Length && query[trimmed.Length] == ' ' ? trimmed + " " : trimmed;
        var sets = new List<List<Term>>();
        var set = new List<Term>();
        var switchSet = false;
        var afterBar = false;
        foreach (var token in query.Replace("\\ ", "\t").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var text = token.Replace('\t', ' ');
            var lower = text.ToLowerInvariant();
            var sensitive = text != lower;
            var normalize = lower.All(character => Normalize(character) == character);
            if (!sensitive) text = lower;
            if (set.Count > 0 && !afterBar && text == "|")
            {
                switchSet = false;
                afterBar = true;
                continue;
            }
            afterBar = false;
            var type = 0;
            var inverse = text.StartsWith('!');
            if (inverse) { type = 1; text = text.Substring(1); }
            if (text != "$" && text.EndsWith('$')) { type = 3; text = text.Substring(0, text.Length - 1); }
            if (text.StartsWith('\'')) { type = inverse ? 0 : 1; text = text.Substring(1); }
            else if (text.StartsWith('^')) { type = type == 3 ? 4 : 2; text = text.Substring(1); }
            if (text.Length == 0) continue;
            if (switchSet) { sets.Add(set); set = []; }
            set.Add(new(text, type, inverse, sensitive, normalize));
            switchSet = true;
        }
        if (set.Count > 0) sets.Add(set);
        return sets;
    }

    public static FzfResult? Match(string text, string query)
    {
        text = text.Normalize();
        query = query.Normalize();
        var total = 0;
        var positions = new HashSet<int>();
        foreach (var set in Parse(query))
        {
            var matched = false;
            var score = 0;
            foreach (var term in set)
            {
                var result = term.Type == 0 ? Fuzzy(text, term) : Exact(text, term);
                if (result is not null && !term.Inverse)
                {
                    matched = true;
                    score = result.Score;
                    positions.UnionWith(result.Positions);
                    break;
                }
                if (result is null && term.Inverse) matched = true;
            }
            if (!matched) return null;
            total += score;
        }
        return new(total, positions);
    }

    private static FzfResult? Fuzzy(string text, Term term)
    {
        var pattern = term.Text;
        var length = text.Length;
        var count = pattern.Length;
        if (count > length) return null;
        if ((long)length * count > 102400) return Greedy(text, term);
        var first = new int[count];
        var transformed = new char[length];
        var bonus = new int[length];
        var firstScores = new int[length];
        var firstConsecutive = new int[length];
        var patternIndex = 0;
        var last = 0;
        var previousClass = 0;
        var previousScore = 0;
        var inGap = false;
        var maxScore = 0;
        var maxPosition = 0;
        for (var column = 0; column < length; column++)
        {
            var characterClass = Class(text[column]);
            transformed[column] = Fold(text[column], term);
            bonus[column] = Bonus(previousClass, characterClass);
            previousClass = characterClass;
            if (transformed[column] == pattern[Math.Min(patternIndex, count - 1)])
            {
                if (patternIndex < count) first[patternIndex++] = column;
                last = column;
            }
            if (transformed[column] == pattern[0])
            {
                firstScores[column] = 16 + 2 * bonus[column];
                firstConsecutive[column] = 1;
                if (count == 1 && firstScores[column] > maxScore)
                {
                    maxScore = firstScores[column];
                    maxPosition = column;
                    if (bonus[column] == 8) break;
                }
                inGap = false;
            }
            else
            {
                firstScores[column] = Math.Max(previousScore + (inGap ? -1 : -3), 0);
                inGap = true;
            }
            previousScore = firstScores[column];
        }
        if (patternIndex != count) return null;
        if (count == 1) return new(maxScore, new HashSet<int> { maxPosition });
        var start = first[0];
        var width = last - start + 1;
        var scores = new int[width * count];
        var consecutive = new int[width * count];
        Array.Copy(firstScores, start, scores, 0, width);
        Array.Copy(firstConsecutive, start, consecutive, 0, width);
        for (var row = 1; row < count; row++)
        {
            inGap = false;
            scores[row * width + first[row] - start - 1] = 0;
            for (var column = first[row]; column <= last; column++)
            {
                var cell = row * width + column - start;
                var gapScore = scores[cell - 1] + (inGap ? -1 : -3);
                var matchScore = 0;
                var runLength = 0;
                if (pattern[row] == transformed[column])
                {
                    matchScore = scores[cell - width - 1] + 16;
                    var currentBonus = bonus[column];
                    runLength = consecutive[cell - width - 1] + 1;
                    if (currentBonus == 8) runLength = 1;
                    else if (runLength > 1) currentBonus = Math.Max(currentBonus, Math.Max(4, bonus[column - runLength + 1]));
                    if (matchScore + currentBonus < gapScore) { matchScore += bonus[column]; runLength = 0; }
                    else matchScore += currentBonus;
                }
                consecutive[cell] = runLength;
                inGap = matchScore < gapScore;
                scores[cell] = Math.Max(Math.Max(matchScore, gapScore), 0);
                if (row == count - 1 && scores[cell] > maxScore) { maxScore = scores[cell]; maxPosition = column; }
            }
        }
        var positions = new HashSet<int>();
        var matchRow = count - 1;
        var matchColumn = maxPosition;
        var preferMatch = true;
        while (matchColumn >= start)
        {
            var cell = matchRow * width + matchColumn - start;
            var diagonal = matchRow > 0 && matchColumn >= first[matchRow] ? scores[cell - width - 1] : 0;
            var left = matchColumn > first[matchRow] ? scores[cell - 1] : 0;
            if (scores[cell] > diagonal && (scores[cell] > left || scores[cell] == left && preferMatch))
            {
                positions.Add(matchColumn);
                if (matchRow == 0) break;
                matchRow--;
            }
            preferMatch = consecutive[cell] > 1 || cell + width + 1 < consecutive.Length && consecutive[cell + width + 1] > 0;
            matchColumn--;
        }
        return new(maxScore, positions);
    }

    private static FzfResult? Greedy(string text, Term term)
    {
        var patternIndex = 0;
        var end = -1;
        for (var index = 0; index < text.Length; index++)
            if (Fold(text[index], term) == term.Text[patternIndex] && ++patternIndex == term.Text.Length) { end = index + 1; break; }
        if (end < 0) return null;
        patternIndex--;
        var start = end - 1;
        for (; start >= 0; start--)
            if (Fold(text[start], term) == term.Text[patternIndex] && --patternIndex < 0) break;
        return Score(text, term, start, end);
    }
    private static FzfResult? Exact(string text, Term term)
    {
        var transformed = new string(text.Select(character => Fold(character, term)).ToArray());
        var start = 0;
        var end = text.Length;
        if (term.Type is 2 or 4 && !char.IsWhiteSpace(term.Text[0]))
            while (start < end && char.IsWhiteSpace(text[start])) start++;
        if (term.Type is 3 or 4 && !char.IsWhiteSpace(term.Text[term.Text.Length - 1]))
            while (end > start && char.IsWhiteSpace(text[end - 1])) end--;
        if (end - start < term.Text.Length) return null;
        if (term.Type == 3) start = end - term.Text.Length;
        if (term.Type == 4 && end - start != term.Text.Length) return null;
        if (term.Type == 1)
        {
            var best = -1;
            var bestBonus = -1;
            for (var offset = 0; offset <= text.Length - term.Text.Length; offset++)
            {
                if (offset > transformed.Length - term.Text.Length || string.CompareOrdinal(transformed, offset, term.Text, 0, term.Text.Length) != 0) continue;
                var bonus = BonusAt(text, offset);
                if (bonus > bestBonus) { best = offset; bestBonus = bonus; }
                if (bonus == 8) break;
            }
            start = best;
        }
        if (start < 0 || start > transformed.Length - term.Text.Length || string.CompareOrdinal(transformed, start, term.Text, 0, term.Text.Length) != 0) return null;
        return term.Type == 4 ? new(24 * term.Text.Length + 8, new HashSet<int>(Enumerable.Range(start, term.Text.Length)))
            : Score(text, term, start, start + term.Text.Length);
    }
    private static FzfResult Score(string text, Term term, int start, int end)
    {
        var score = 0;
        var patternIndex = 0;
        var consecutive = 0;
        var firstBonus = 0;
        var inGap = false;
        var previousClass = start > 0 ? Class(text[start - 1]) : 0;
        var positions = new HashSet<int>();
        for (var index = start; index < end; index++)
        {
            var characterClass = Class(text[index]);
            if (Fold(text[index], term) == term.Text[patternIndex])
            {
                positions.Add(index);
                var bonus = Bonus(previousClass, characterClass);
                if (consecutive == 0) firstBonus = bonus;
                else
                {
                    if (bonus == 8) firstBonus = bonus;
                    bonus = Math.Max(Math.Max(bonus, firstBonus), 4);
                }
                score += 16 + bonus * (patternIndex == 0 ? 2 : 1);
                consecutive++;
                patternIndex++;
                inGap = false;
            }
            else
            {
                score += inGap ? -1 : -3;
                inGap = true;
                consecutive = 0;
                firstBonus = 0;
            }
            previousClass = characterClass;
        }
        return new(score, positions);
    }
}
