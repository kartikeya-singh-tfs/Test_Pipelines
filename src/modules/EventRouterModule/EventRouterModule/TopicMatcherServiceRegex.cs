using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace ThermoFisher.EventRouterModule;

/// <inheritdoc/>
internal class TopicMatcherServiceRegex : ITopicMatcherService
{
    private ConcurrentDictionary<string, PatternTopicMatcher> _matcherCache = new();

    /// <inheritdoc/>
    public bool IsMatch(string pattern, string topic)
    {
        var matcher = _matcherCache.GetOrAdd(pattern, p => new PatternTopicMatcher(p));
        return matcher.IsMatch(topic);
    }

    private class PatternTopicMatcher
    {
        private string _pattern;
        private Regex _regex;
        private ConcurrentHashSet<string> _matchedTopics;
        private ConcurrentHashSet<string> _unmatchedTopics;

        public PatternTopicMatcher(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException(
                    "Pattern cannot be null or whitespace",
                    nameof(pattern)
                );
            }

            _pattern = pattern;

            // Only create a regex if the pattern contains wildcards
            if (_pattern.Contains('*') || _pattern.Contains('#'))
            {
                _regex = GetRegexForPattern();
                _matchedTopics = [];
                _unmatchedTopics = [];
            }
        }

        public bool IsMatch(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return false;
            }
            if (_pattern == topic)
            {
                return true;
            }
            if (_regex == null)
            {
                return false;
            }
            if (_matchedTopics.Contains(topic))
            {
                return true;
            }
            if (_unmatchedTopics.Contains(topic))
            {
                return false;
            }
            var isMatch = _regex.IsMatch(topic);
            if (isMatch)
            {
                _matchedTopics.Add(topic);
            }
            else
            {
                _unmatchedTopics.Add(topic);
            }
            return isMatch;
        }

        /// <summary>
        /// Create a Regex for the given pattern
        /// * (star) can substitute for exactly one word.
        /// # (hash) can substitute for zero or more words.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        // The following code is from with the added support for empty words
        // https://stackoverflow.com/a/76292667
        private Regex GetRegexForPattern()
        {
            string regexPattern;
            if (_pattern == "#")
            {
                regexPattern = @"^.*$";
            }
            else
            {
                // Escape any special regex characters in the pattern
                regexPattern = $"^{Regex.Escape(_pattern)}$";
                regexPattern = regexPattern.Replace(@"\*", @"[^.]*");
                regexPattern = regexPattern.Replace(@"\.\#", @"(?:\.[^.]*)*");
                regexPattern = regexPattern.Replace(@"\#\.", @"(?:[^.]*\.)*");
            }

            return new Regex(
                regexPattern,
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking
            );
        }
    }
}
