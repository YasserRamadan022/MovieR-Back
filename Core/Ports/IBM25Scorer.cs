using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ports
{
    public interface IBM25Scorer
    {
        double CalculateScore(List<string> queryTerms, string documentText, int documentId);
        double CalculateMultiFieldScore(
            List<string> queryTerms,
            Dictionary<string, string> fields,
            Dictionary<string, double> fieldWeights,
            int documentId);
        Task InitializeAsync(List<(int Id, string Text)> documents);
        double GetIdfScore(string term);
    }
}
