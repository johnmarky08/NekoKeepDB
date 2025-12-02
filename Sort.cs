namespace NekoKeepDB
{
    /**
     * Merge Sort Algorithm
     * Time Complexity: O(n log n) - For average, and worst cases
     * Space Complexity: O(n) - Due to auxiliary array
     * Stability: Stable (preserves the order of equal elements)
     */
    public static class Sort
    {
        // Generic stable merge sort that returns a new sorted list
        public static List<T> MergeSort<T>(List<T> sourceList, Comparison<T> comparisonFunction)
        {
            if (sourceList.Count <= 1) return [.. sourceList];

            T[] workingArray = [.. sourceList];
            T[] auxiliaryArray = new T[workingArray.Length];
            MergeSortRecursive(workingArray, auxiliaryArray, 0, workingArray.Length - 1, comparisonFunction);
            return [.. workingArray];
        }

        private static void MergeSortRecursive<T>(T[] workingArray, T[] auxiliaryArray, int leftIndex, int rightIndex, Comparison<T> comparisonFunction)
        {
            if (leftIndex >= rightIndex) return;
            int middleIndex = leftIndex + (rightIndex - leftIndex) / 2;
            MergeSortRecursive(workingArray, auxiliaryArray, leftIndex, middleIndex, comparisonFunction); // Sort left half
            MergeSortRecursive(workingArray, auxiliaryArray, middleIndex + 1, rightIndex, comparisonFunction); // Sort right half
            MergeRanges(workingArray, auxiliaryArray, leftIndex, middleIndex, rightIndex, comparisonFunction); // Merge sorted halves
        }

        private static void MergeRanges<T>(T[] workingArray, T[] auxiliaryArray, int leftIndex, int middleIndex, int rightIndex, Comparison<T> comparisonFunction)
        {
            // Copy the range to auxiliary array
            for (int i = leftIndex; i <= rightIndex; i++)
            {
                auxiliaryArray[i] = workingArray[i];
            }

            int leftPointer = leftIndex;
            int rightPointer = middleIndex + 1;
            int writePointer = leftIndex;

            while (leftPointer <= middleIndex && rightPointer <= rightIndex)
            {
                // Use <= to preserve stability: left element wins when equal
                if (comparisonFunction(auxiliaryArray[leftPointer], auxiliaryArray[rightPointer]) <= 0)
                {
                    workingArray[writePointer++] = auxiliaryArray[leftPointer++];
                }
                else
                {
                    workingArray[writePointer++] = auxiliaryArray[rightPointer++];
                }
            }

            // Copy any remaining elements from the left half
            while (leftPointer <= middleIndex)
            {
                workingArray[writePointer++] = auxiliaryArray[leftPointer++];
            }

            // Copy any remaining elements from the right half
            while (rightPointer <= rightIndex)
            {
                workingArray[writePointer++] = auxiliaryArray[rightPointer++];
            }
        }
    }
}
