﻿using System.Collections.Generic;
using System.Diagnostics;

namespace Common.Collections
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Helper function that swaps erasing element with the back element and then erases the back.
        /// </summary>
        /// <remarks>
        /// This function helps avoid array rearrangements but is only useful for collections where element order does not matter.
        /// </remarks>
        /// <typeparam name="T">List element type.</typeparam>
        /// <param name="i_List">List container.</param>
        /// <param name="i_Index">Index of element to erase.</param>
        public static void RemoveSwap<T>(this IList<T> i_List, int i_Index)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");
            Debug.Assert(i_Index >= 0 && i_Index < i_List.Count, "Index out of bounds.");

            int size = i_List.Count;
            if (size > 0)
            {
                int lastIndex = size - 1;
                T temp = i_List[lastIndex];
                i_List.RemoveAt(lastIndex);
                i_List[i_Index] = temp;
            }
        }

        /// <summary>
        /// Performs insertion sort on the array.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="i_Array">The array.</param>
        /// <param name="i_Comparer">An optional comparer.</param>
        public static void InsertionSort<T>(this T[] i_Array, IComparer<T> i_Comparer = null)
        {
            Debug.Assert(i_Array != null, "Invalid null parameter.");

            IComparer<T> comparer = i_Comparer ?? Comparer<T>.Default;
            int size = i_Array.Length;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                T examinedValue = i_Array[index];
                //early break test for quicker processing of almost sorted arrays
                if (comparer.Compare(i_Array[readIndex], examinedValue) > 0)
                {
                    while ((readIndex >= 0) && (comparer.Compare(i_Array[readIndex], examinedValue) > 0))
                    {
                        i_Array[readIndex + 1] = i_Array[readIndex];
                        --readIndex;
                    }
                    i_Array[readIndex + 1] = examinedValue;
                }
            }
        }

        /// <summary>
        /// Performs insertion sort on the list.
        /// </summary>
        /// <typeparam name="T">List element type</typeparam>
        /// <param name="i_List">The list.</param>
        /// <param name="i_Comparer">The comparer.</param>
        public static void InsertionSort<T>(this IList<T> i_List, IComparer<T> i_Comparer = null)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            IComparer<T> comparer = i_Comparer ?? Comparer<T>.Default;
            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                T examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (comparer.Compare(i_List[readIndex], examinedValue) > 0)
                {
                    while ((readIndex >= 0) && (comparer.Compare(i_List[readIndex], examinedValue) > 0))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

#region Specialised primitive numerical type InsertionSort varients

        public static void InsertionSortAscending(this IList<int> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                int examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] > examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] > examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        public static void InsertionSortDescending(this IList<int> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                int examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] < examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] < examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        public static void InsertionSortAscending(this IList<float> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                float examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] > examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] > examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        public static void InsertionSortDescending(this IList<float> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");
            
            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                float examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] < examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] < examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        public static void InsertionSortAscending(this IList<long> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                long examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] > examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] > examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        public static void InsertionSortDescending(this IList<long> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                long examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] < examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] < examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }
        

        public static void InsertionSortAscending(this IList<double> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");
            
            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                double examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] > examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] > examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        public static void InsertionSortDescending(this IList<double> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                double examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] < examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] < examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }


        public static void InsertionSortAscending(this IList<short> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                short examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] > examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] > examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        public static void InsertionSortDescending(this IList<short> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");
            
            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                short examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] < examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] < examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }


        public static void InsertionSortAscending(this IList<byte> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");
            
            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                byte examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] > examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] > examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        public static void InsertionSortDescending(this IList<byte> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");
            
            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                byte examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] < examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] < examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }


        public static void InsertionSortAscending(this IList<ulong> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                ulong examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] > examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] > examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        public static void InsertionSortDescending(this IList<ulong> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                ulong examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] < examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] < examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }


        public static void InsertionSortAscending(this IList<uint> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                uint examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] > examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] > examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        public static void InsertionSortDescending(this IList<uint> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                uint examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] < examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] < examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }


        public static void InsertionSortAscending(this IList<ushort> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                ushort examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] > examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] > examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        public static void InsertionSortDescending(this IList<ushort> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                ushort examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] < examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] < examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }


        public static void InsertionSortAscending(this IList<sbyte> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                sbyte examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] > examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] > examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        public static void InsertionSortDescending(this IList<sbyte> i_List)
        {
            Debug.Assert(i_List != null, "Invalid null parameter.");

            int size = i_List.Count;
            for (int index = 1; index < size; ++index)
            {
                int readIndex = index - 1;
                sbyte examinedValue = i_List[index];
                //early break test for quicker processing of almost sorted arrays
                if (i_List[readIndex] < examinedValue)
                {
                    while ((readIndex >= 0) && (i_List[readIndex] < examinedValue))
                    {
                        i_List[readIndex + 1] = i_List[readIndex];
                        --readIndex;
                    }
                    i_List[readIndex + 1] = examinedValue;
                }
            }
        }

        #endregion
    }
}
