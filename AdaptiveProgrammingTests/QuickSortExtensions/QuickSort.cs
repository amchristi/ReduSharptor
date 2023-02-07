using System;
using System.Collections.Generic;
using System.Text;

namespace QuickSortExtentions
{
    static public class QuickSort
    {
        static private int Partition(int[] array, int low, int high)
        {
            int pivot = array[high];

            int lowIndex = (low - 1);

            for (int j = low; j < high; j++)
            {
                if (array[j] <= pivot)
                {
                    lowIndex++;

                    int temp = array[lowIndex];
                    array[lowIndex] = array[j];
                    array[j] = temp;
                }
            }

            int temp1 = array[lowIndex + 1];
            array[lowIndex + 1] = array[high];
            array[high] = temp1;

            return lowIndex + 1;
        }

        static private bool AnyDuplicates(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (array[i] == array[j])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static private bool Sort(int[] array, int low, int high)
        {
            if (low < high)
            {
                int partitionIndex = Partition(array, low, high);

                // If either portion failes, return false
                if (!Sort(array, low, partitionIndex - 1) || !Sort(array, partitionIndex + 1, high))
                {
                    return false;
                }
            }

            // Otherwise return true
            return true;
        }

        static public bool Sort(int[] array)
        {
            // If there are duplicates, then return false
            if (AnyDuplicates(array))
            {
                return false;
            }

            return QuickSort.Sort(array, 0, array.Length - 1);
        }

        static public bool IsSorted(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (array[i] > array[j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
