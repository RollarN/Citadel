using System;
using UnityEngine;

public static class EnumArrayUtility
{
    public static void CheckAssetArray<TEnum>(ref GameObject[] assetArray, ref string[] trackedEnumArray) where TEnum : struct, IComparable
    {
        if (!typeof(TEnum).IsEnum)
        {
            throw new ArgumentException("TEnum must be an enumerated type");
        }

        TEnum[] enumArray = Enum.GetValues(typeof(TEnum)) as TEnum[];

        if(enumArray.Length - 1 != assetArray.Length || enumArray.Length != trackedEnumArray.Length)
        {
            UpdateAssetArrays<TEnum>(ref assetArray, ref trackedEnumArray);
            return;
        }

        for(int i = 0; i < enumArray.Length - 1; i++)
        {
            if(enumArray[i].CompareTo(trackedEnumArray[i]) != 0)
            {
                UpdateAssetArrays<TEnum>(ref assetArray, ref trackedEnumArray);
                return;
            }
        }
    }

    // Bad code design but c# won't let me create params ref Object[][]
    #region CheckAssetArray Overloads
    public static void CheckAssetArrays<TEnum>(ref GameObject[] assetArray0, ref GameObject[] assetArray1, ref string[] trackedEnumArray) where TEnum : struct, IComparable
    {
        if (!typeof(TEnum).IsEnum)
        {
            throw new ArgumentException("TEnum must be an enumerated type");
        }

        TEnum[] enumArray = Enum.GetValues(typeof(TEnum)) as TEnum[];

        if (enumArray.Length - 1 != assetArray0.Length
            || enumArray.Length - 1 != assetArray1.Length
            || enumArray.Length != trackedEnumArray.Length)
        {
            UpdateAssetArrays<TEnum>(ref assetArray0, ref assetArray1, ref trackedEnumArray);
            return;
        }

        for (int i = 0; i < enumArray.Length - 1; i++)
        {
            if (enumArray[i].CompareTo(trackedEnumArray[i]) != 0)
            {
                UpdateAssetArrays<TEnum>(ref assetArray0, ref assetArray1, ref trackedEnumArray);
                return;
            }
        }
    }

    public static void CheckAssetArrays<TEnum>(ref GameObject[] assetArray0, ref GameObject[] assetArray1, ref GameObject[] assetArray2, ref string[] trackedEnumArray) where TEnum : struct, IComparable
    {
        if (!typeof(TEnum).IsEnum)
        {
            throw new ArgumentException("TEnum must be an enumerated type");
        }

        TEnum[] enumArray = Enum.GetValues(typeof(TEnum)) as TEnum[];

        if (enumArray.Length - 1 != assetArray0.Length
            || enumArray.Length - 1 != assetArray1.Length
            || enumArray.Length - 1 != assetArray2.Length
            || enumArray.Length != trackedEnumArray.Length)
        {
            UpdateAssetArrays<TEnum>(ref assetArray0, ref assetArray1, ref assetArray2, ref trackedEnumArray);
            return;
        }

        for (int i = 0; i < enumArray.Length - 1; i++)
        {
            if (enumArray[i].ToString() != trackedEnumArray[i])
            {
                UpdateAssetArrays<TEnum>(ref assetArray0, ref assetArray1, ref assetArray2, ref trackedEnumArray);
                return;
            }
        }
    }

    public static void CheckAssetArrays<TEnum>(ref GameObject[] assetArray0, ref GameObject[] assetArray1, ref GameObject[] assetArray2, ref GameObject[] assetArray3, ref string[] trackedEnumArray) where TEnum : struct, IComparable
    {
        if (!typeof(TEnum).IsEnum)
        {
            throw new ArgumentException("TEnum must be an enumerated type");
        }

        TEnum[] enumArray = Enum.GetValues(typeof(TEnum)) as TEnum[];

        if (enumArray.Length - 1 != assetArray0.Length
            || enumArray.Length - 1 != assetArray1.Length
            || enumArray.Length - 1 != assetArray2.Length
            || enumArray.Length - 1 != assetArray3.Length
            || enumArray.Length != trackedEnumArray.Length)
        {
            UpdateAssetArrays<TEnum>(ref assetArray0, ref assetArray1, ref assetArray2, ref assetArray3, ref trackedEnumArray);
            return;
        }

        for (int i = 0; i < enumArray.Length - 1; i++)
        {
            if (enumArray[i].CompareTo(trackedEnumArray[i]) != 0)
            {
                UpdateAssetArrays<TEnum>(ref assetArray0, ref assetArray1, ref assetArray2, ref assetArray3, ref trackedEnumArray);
                return;
            }
        }
    }
    #endregion

    private static void UpdateAssetArrays<TEnum>(ref GameObject[] assetArray, ref string[] trackedEnumArray) where TEnum : struct, IComparable
    {
        UpdateAssetArray<TEnum>(ref assetArray, in trackedEnumArray);
        UpdateTrackedEnumArray<TEnum>(ref trackedEnumArray);
    }

    // Bad code design but c# won't let me create params ref Object[][]
    #region UpdateAssetArrays Overloads
    private static void UpdateAssetArrays<TEnum>(ref GameObject[] assetArray0, ref GameObject[] assetArray1, ref string[] trackedEnumArray) where TEnum : struct, IComparable
    {
        UpdateAssetArray<TEnum>(ref assetArray0, in trackedEnumArray);
        UpdateAssetArray<TEnum>(ref assetArray1, in trackedEnumArray);
        UpdateTrackedEnumArray<TEnum>(ref trackedEnumArray);
    }

    private static void UpdateAssetArrays<TEnum>(ref GameObject[] assetArray0, ref GameObject[] assetArray1, ref GameObject[] assetArray2, ref string[] trackedEnumArray) where TEnum : struct, IComparable
    {
        UpdateAssetArray<TEnum>(ref assetArray0, in trackedEnumArray);
        UpdateAssetArray<TEnum>(ref assetArray1, in trackedEnumArray);
        UpdateAssetArray<TEnum>(ref assetArray2, in trackedEnumArray);
        UpdateTrackedEnumArray<TEnum>(ref trackedEnumArray);
    }

    private static void UpdateAssetArrays<TEnum>(ref GameObject[] assetArray0, ref GameObject[] assetArray1, ref GameObject[] assetArray2, ref GameObject[] assetArray3, ref string[] trackedEnumArray) where TEnum : struct, IComparable
    {
        UpdateAssetArray<TEnum>(ref assetArray0, in trackedEnumArray);
        UpdateAssetArray<TEnum>(ref assetArray1, in trackedEnumArray);
        UpdateAssetArray<TEnum>(ref assetArray2, in trackedEnumArray);
        UpdateAssetArray<TEnum>(ref assetArray3, in trackedEnumArray);
        UpdateTrackedEnumArray<TEnum>(ref trackedEnumArray);
    }
    #endregion

    private static void UpdateAssetArray<TEnum>(ref GameObject[] assetArray, in string[] trackedEnumArray) where TEnum : struct, IComparable
    {
        if (!typeof(TEnum).IsEnum)
        {
            throw new ArgumentException("TEnum must be an enumerated type");
        }
        
        TEnum[] enumArray = Enum.GetValues(typeof(TEnum)) as TEnum[];
        GameObject[] newObjectArray = new GameObject[enumArray.Length - 1];

        for(int i = 0; i < enumArray.Length - 1; i++) 
        {
            for(int j = 0; j < assetArray.Length && assetArray[j] != null && j < trackedEnumArray.Length; j++)
            {
                if(enumArray[i].ToString() == trackedEnumArray[j])
                {
                    newObjectArray[i] = assetArray[j];
                    break;
                }
                newObjectArray[i] = null;
            }
        }
        assetArray = newObjectArray;
    }
    private static void UpdateTrackedEnumArray<TEnum>(ref string[] enumArray)
    {
        enumArray = Enum.GetNames(typeof(TEnum));
    }
}
