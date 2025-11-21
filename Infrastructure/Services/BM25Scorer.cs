using Castle.Core.Logging;
using Core.Ports;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class BM25Scorer : IBM25Scorer
    {
        private const double K1 = 1.2;  // Term frequency saturation parameter
        private const double B = 0.75;  // Length normalization parameter

        // Pre-computed statistics
        private Dictionary<string, double> _idfScores; // Term → IDF score
        private Dictionary<string, int> _documentFrequencies; // Term → How many documents contain it
        private double _averageDocumentLength;
        private int _totalDocuments;
        private Dictionary<int, int> _documentLengths; // DocumentId → Length
        private bool _isInitialized = false;
        public BM25Scorer()
        {
            _idfScores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            _documentFrequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _documentLengths = new Dictionary<int, int>();
        }
        private List<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            return text
                .ToLowerInvariant()
                .Split(new[] { ' ', '\t', '\n', '\r', ',', '.', '!', '?', ';', ':', '-', '_', '(', ')', '[', ']', '{', '}' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length > 1)
                .ToList();
        }
        public async Task InitializeAsync(List<(int Id, string Text)> documents)
        {
            if (documents == null || !documents.Any())
            {
                return;
            }

            _totalDocuments = documents.Count;
            _documentFrequencies.Clear();
            _documentLengths.Clear();
            _idfScores.Clear();

            // Step 1: Calculate document frequencies and lengths
            foreach (var (id, text) in documents)
            {
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                var terms = Tokenize(text);
                int docLength = terms.Count;
                _documentLengths[id] = docLength;

                // Count unique terms in this document
                var uniqueTerms = new HashSet<string>(terms, StringComparer.OrdinalIgnoreCase);
                foreach (var term in uniqueTerms)
                {
                    _documentFrequencies.TryGetValue(term, out int currentCount);
                    _documentFrequencies[term] = currentCount + 1;
                }
            }

            // Step 2: Calculate average document length
            if (_documentLengths.Any())
            {
                _averageDocumentLength = _documentLengths.Values.Average();
            }
            else
            {
                _averageDocumentLength = 0;
            }

            // Step 3: Calculate IDF scores for all terms
            foreach (var term in _documentFrequencies.Keys)
            {
                int df = _documentFrequencies[term]; // Document frequency
                double idf = Math.Log((_totalDocuments - df + 0.5) / (df + 0.5) + 1.0);
                _idfScores[term] = idf;
            }

            _isInitialized = true;
        }
        public double GetIdfScore(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return 0;

            string normalizedTerm = term.ToLowerInvariant().Trim();
            return _idfScores.TryGetValue(normalizedTerm, out double idf) ? idf : 0;
        }
        /// <summary>
        /// Calculates BM25 score for a query against a document
        /// </summary>
        public double CalculateScore(List<string> queryTerms, string documentText, int documentId)
        {
            if (!_isInitialized)
            {
                return 0;
            }

            if (string.IsNullOrWhiteSpace(documentText) || queryTerms == null || !queryTerms.Any())
                return 0;

            // Get document length
            if (!_documentLengths.TryGetValue(documentId, out int docLength))
            {
                var terms = Tokenize(documentText);
                docLength = terms.Count;
                _documentLengths[documentId] = docLength;
            }

            // Tokenize document
            var documentTerms = Tokenize(documentText);
            var termFrequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Count term frequencies in document
            foreach (var term in documentTerms)
            {
                termFrequencies.TryGetValue(term, out int count);
                termFrequencies[term] = count + 1;
            }

            // Calculate BM25 score
            double score = 0.0;

            foreach (var queryTerm in queryTerms)
            {
                string normalizedQueryTerm = queryTerm.ToLowerInvariant().Trim();

                // Get IDF for this term
                double idf = GetIdfScore(normalizedQueryTerm);
                if (idf <= 0)
                    continue; // Term not in corpus, skip

                // Get term frequency in document
                int tf = termFrequencies.TryGetValue(normalizedQueryTerm, out int freq) ? freq : 0;
                if (tf == 0)
                    continue; // Term not in document, skip

                // BM25 formula
                double numerator = idf * tf * (K1 + 1);
                double denominator = tf + K1 * (1 - B + B * (docLength / _averageDocumentLength));
                double termScore = numerator / denominator;

                score += termScore;
            }

            return score;
        }
        /// <summary>
        /// Calculates BM25 score across multiple fields with different weights
        /// </summary>
        public double CalculateMultiFieldScore(List<string> queryTerms, Dictionary<string, string> fields, Dictionary<string, double> fieldWeights, int documentId)
        {
            if (fields == null || !fields.Any() || fieldWeights == null || !fieldWeights.Any())
                return 0;

            double totalScore = 0.0;

            foreach (var field in fields)
            {
                string fieldName = field.Key;
                string fieldText = field.Value ?? string.Empty;

                // Get weight for this field (default to 0 if not specified)
                double weight = fieldWeights.TryGetValue(fieldName, out double w) ? w : 0.0;
                if (weight <= 0)
                    continue; // Skip fields with zero weight

                // Calculate BM25 score for this field
                double fieldScore = CalculateScore(queryTerms, fieldText, documentId);

                // Apply weight and add to total
                totalScore += fieldScore * weight;
            }

            return totalScore;
        }
    }
}
