using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing.Internal;

namespace ThermoFisher.EventRouterModule;

/// <inheritdoc/>
internal class TopicMatcherService : ITopicMatcherService
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
        private readonly string _pattern;
        private readonly bool _patternHasWildcards;
        private readonly ConcurrentDictionary<string, bool> _topicMatchCache;
        private readonly string[] _patternParts;

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
            _patternHasWildcards = _pattern.Contains('*') || _pattern.Contains('#');

            if (_patternHasWildcards)
            {
                _topicMatchCache = [];
                _patternParts = _pattern.Split('.');
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
            if (!_patternHasWildcards)
            {
                return false;
            }
            if (_topicMatchCache.TryGetValue(topic, out var match))
            {
                return match;
            }

            var isMatch = IsTopicMatch(topic);
            _topicMatchCache[topic] = isMatch;
            return isMatch;
        }

        private bool IsTopicMatch(string topic)
        {
            var topicParts = topic.Split('.');
            return IsTopicMatchRecursive(0, topicParts, 0);
        }

        private bool IsTopicMatchRecursive(int pIndex, string[] topicParts, int tIndex)
        {
            while (pIndex < _patternParts.Length && tIndex < topicParts.Length)
            {
                if (_patternParts[pIndex] == "#")
                {
                    // '#' at end matches all remaining topic parts
                    if (pIndex == _patternParts.Length - 1)
                    {
                        return true;
                    }
                    // Try to match zero or more topic parts
                    for (int skip = tIndex; skip <= topicParts.Length; skip++)
                    {
                        if (IsTopicMatchRecursive(pIndex + 1, topicParts, skip))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                else if (_patternParts[pIndex] == "*")
                {
                    pIndex++;
                    tIndex++;
                }
                else
                {
                    // this part must match exactly
                    if (_patternParts[pIndex] != topicParts[tIndex])
                    {
                        return false;
                    }
                    pIndex++;
                    tIndex++;
                }
            }

            // Remaining pattern parts must all be '#' wildcards
            while (pIndex < _patternParts.Length && _patternParts[pIndex] == "#")
            {
                pIndex++;
            }

            // Both pattern and topic must be fully consumed
            return pIndex == _patternParts.Length && tIndex == topicParts.Length;
        }
    }
}
