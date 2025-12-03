using NekoKeepDB.Classes;

namespace NekoKeepDB
{
    /**
     * Levenshtein Distance Algorithm
     * Time Complexity: O(k * m * n + k log k)
     * Space Complexity: O(m * n + k)
     * Stability: Stable
     */
    public static class Search
    {
        // Compute Levenshtein Distance between two strings
        private static int CalculateLevenshteinDistance(string sourceText, string targetText)
        {
            int sourceLength = sourceText.Length;
            int targetLength = targetText.Length;
            int[,] distanceMatrix = new int[sourceLength + 1, targetLength + 1];

            // Initialize base cases
            for (int rowIndex = 0; rowIndex <= sourceLength; rowIndex++)
                distanceMatrix[rowIndex, 0] = rowIndex;

            for (int columnIndex = 0; columnIndex <= targetLength; columnIndex++)
                distanceMatrix[0, columnIndex] = columnIndex;

            // Fill the matrix
            for (int rowIndex = 1; rowIndex <= sourceLength; rowIndex++)
            {
                for (int columnIndex = 1; columnIndex <= targetLength; columnIndex++)
                {
                    int substitutionCost = (sourceText[rowIndex - 1] == targetText[columnIndex - 1]) ? 0 : 1;

                    distanceMatrix[rowIndex, columnIndex] = Math.Min(
                        Math.Min(distanceMatrix[rowIndex - 1, columnIndex] + 1,    // deletion
                                 distanceMatrix[rowIndex, columnIndex - 1] + 1),   // insertion
                        distanceMatrix[rowIndex - 1, columnIndex - 1] + substitutionCost // substitution
                    );
                }
            }

            return distanceMatrix[sourceLength, targetLength];
        }

        // Extract searchable text from an Account (DisplayName, Email, Provider if OAuthAccount)
        private static IEnumerable<string> GetSearchableFields(Account account)
        {
            var fields = new List<string>
        {
            account.Data.DisplayName,
            account.Data.Email
        };

            if (account is OAuthAccount oAuthAccount)
            {
                fields.Add(oAuthAccount.Data.Provider);
            }

            return fields.Where(field => !string.IsNullOrEmpty(field));
        }

        // Return a new list of Accounts sorted by Levenshtein distance, excluding non-matches
        public static List<Account> GetSortedMatchesByLevenshtein(string searchKeyword, List<Account> candidateAccounts)
        {
            string lowerCaseKeyword = searchKeyword.ToLower();

            return [.. candidateAccounts
            .Where(account =>
                GetSearchableFields(account).Any(field => field.Contains(lowerCaseKeyword, StringComparison.CurrentCultureIgnoreCase)))
            .OrderBy(account =>
                GetSearchableFields(account)
                    .Select(field => CalculateLevenshteinDistance(lowerCaseKeyword, field.ToLower()))
                    .Min())];
        }
    }
}