using System.Collections.Generic;

public static class CardArrayExtensions
{
    // Количество НЕ-null элементов (занятые слоты)
    public static int FilledCount<T>(this T[] array) where T : class
    {
        int count = 0;
        foreach (T item in array)
            if (item != null)
                count++;
        return count;
    }

    // Количество null элементов (пустые слоты)
    public static int EmptyCount<T>(this T[] array) where T : class
    {
        int count = 0;
        foreach (T item in array)
            if (item == null)
                count++;
        return count;
    }

    // Есть ли хотя бы один пустой слот
    public static bool AnyNull<T>(this T[] array) where T : class
    {
        foreach (T item in array)
            if (item == null)
                return true;
        return false;
    }

    // Есть ли хотя бы один занятый слот
    public static bool AnyNotNull<T>(this T[] array) where T : class
    {
        foreach (T item in array)
            if (item != null)
                return true;
        return false;
    }

    // Полностью заполнен (нет null)
    public static bool IsFull<T>(this T[] array) where T : class
    {
        return array.FilledCount() == array.Length;
    }

    // Полностью пуст (все null)
    public static bool IsEmpty<T>(this T[] array) where T : class
    {
        return array.FilledCount() == 0;
    }

    public static IEnumerable<T> GetAllNotNull<T>(this T[] array) where T : class
    {
        foreach (T item in array)
            if (item != null)
                yield return item;
    }
}
