using QuickSortExtentions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Adaptive.QuickSortTest
{
    static public class QuickSortTest
    {
        [Fact]
        static public void SuccessfullyRuns()
        {
            int[] array = { 72, 12, 6, 20, 81, 97, 37, 59, 52, 1, 33 };
            bool isSuccessful = QuickSort.Sort(array);

            Assert.True(isSuccessful);
        }

        [Fact]
        static public void SortsCorrectly()
        {
            int[] array = { 72, 12, 6, 20, 81, 97, 37, 59, 52, 1, 33 };
            QuickSort.Sort(array);
            bool isSuccessful = QuickSort.IsSorted(array);

            Assert.True(isSuccessful);

        }

        public static IEnumerable<object[]> TestQuickSortDuplicateData =>
        new List<object[]>
        {
            new object[] { new List<int> { 72, 12, 6, 20, 81, 97, 37, 59, 52, 1 } },
            //new object[] { new List<int> { 72, 12, 6, 20, 81, 97, 37, 59, 52, 1, 20 } }, // Has Duplicate
            new object[] { new List<int> { 72, 12, 6, 20, 81, 97, 37, 59, 52, 1, 54 } },
        };

        [Theory]
        [MemberData(nameof(TestQuickSortDuplicateData))]
        static public bool NoDuplicateEntries(int[] array)
        {
            bool isSuccessful = QuickSort.Sort(array);

            Assert.True(isSuccessful);
            return isSuccessful;
        }
    }
}
