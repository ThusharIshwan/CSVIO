using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// A compoosite object is made up of basic classes and other composite objects.
/// basic composite objects currently implemented include string, bool, and int.
/// </summary>
public abstract class CompObj
{
    private static Dictionary<Type, MethodInfo> InputMethodDict = new Dictionary<Type, MethodInfo>();
    private void UpdateInputMethodDict(Type t)
    {
        MethodInfo m = typeof(CSVIO).GetMethod("Read", new Type[] { typeof(string), typeof(char) }).MakeGenericMethod(t);
        InputMethodDict[t] = m;
    }
    private static Dictionary<Type, MethodInfo> OutputMethodDict = new Dictionary<Type, MethodInfo>();
    private void UpdateOutputMethodDict(Type t)
    {
        MethodInfo m = typeof(CSVIO).GetMethod("Write").MakeGenericMethod(t);
        OutputMethodDict[t] = m;
    }


    //Note that properties defined in derived classes order BEFORE properties defined in its base class.
    private PropertyInfo[] GetProperties()
    {
        return this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }



    /// <summary>
    /// Changes all the properties of an instance according to the given object array.
    /// If the value types don't match up, the defined DefaultValues() is used.
    /// Note that properties defined in derived classes order BEFORE properties defined in its base class.
    /// </summary>
    /// <param name="vals">The object array that has the ordered desired values for the object's properties. </param>
    /// <exception cref="InvalidDefaultLengthAttempt">Thrown if the default is used and doesn't have the same number of values as the object has properties.</exception>
    /// <exception cref="InvalidDefaultTypesAttempt">Thrown if there is a mismatch between the type in the default array and the property type.</exception>
    public void InstanceSetup(object[] vals)
    {
        object[] use = vals;
        PropertyInfo[] properties = this.GetProperties();

        if (use.Length != properties.Length)
        {
            use = DefaultValues();
        }
        if (use.Length != properties.Length)
        {
            throw new InvalidDefaultLengthAttempt();
        }
        else
        {
            try
            {
                for (int i = 0; i < use.Length; i++)
                {
                    properties[i].SetValue(this, use[i]);
                }
            }
            catch
            {
                throw new InvalidDefaultTypesAttempt();
            }
        }
    }

    /// <summary>
    /// Changes all the properties of an instance according to the string and delimiter that has been passed through.
    /// </summary>
    /// <param name="s">The string representing the object</param>
    /// <param name="d">The first delimiter</param>
    /// <exception cref="InvalidDefaultLengthAttempt">Thrown if the default is used and doesn't have the same number of values as the object has properties.</exception>
    /// <exception cref="InvalidDefaultTypesAttempt">Thrown if there is a mismatch between the type in the default array and the property type.</exception>
    public void InstanceSetupFromString(string s, char d)
    {

        PropertyInfo[] properties = this.GetProperties();
        string[] part = s.Split(d);

        if (part.Length != properties.Length)
        {
            InstanceSetup(DefaultValues());
        }
        else
        {
            for (int i = 0; i < part.Length; i++)
            {
                if (!InputMethodDict.ContainsKey(properties[i].PropertyType))
                {
                    UpdateInputMethodDict(properties[i].PropertyType);
                }
                properties[i].SetValue(this, InputMethodDict[properties[i].PropertyType].Invoke(null, new object[] { part[i], CSVIO.GetNextDelimiter(d) }));
            }
        }
    }

    public abstract object[] DefaultValues();
    public CompObj()
    {
        InstanceSetup(DefaultValues());
    }
    public CompObj(string s)
    {
        InstanceSetupFromString(s, ',');
    }
    public CompObj(string s, char d)
    {
        InstanceSetupFromString(s, d);
    }


    /// <summary>
    /// Outputs all of the information stored in the declared properties of the object in readable format.
    /// </summary>
    /// <param name="d">The first delimiter</param>
    /// <returns>a string that the computer can read in order to recreate the object.</returns>
    public string Output(char d)
    {
        PropertyInfo[] properties = this.GetProperties();
        string[] s = new string[properties.Length];

        for (int i = 0; i < properties.Length; i++)
        {
            if (!OutputMethodDict.ContainsKey(properties[i].PropertyType))
            {
                UpdateOutputMethodDict(properties[i].PropertyType);
            }
            s[i] = (string)OutputMethodDict[properties[i].PropertyType].Invoke(null, new object[] { properties[i].GetValue(this), CSVIO.GetNextDelimiter(d) });
        }
        return string.Join(new string(new char[] { d }), s);
    }
    public string Output()
    {
        return Output(',');
    }
    

}


/// <summary>
/// A class which contains all the different functions for importing and exporting data from a CSV file.
/// </summary>

public class CSVIO
{
    private static char[] delimiter_order = { '\n', ',', '$', '|', '*', '^' };
    private static int find_delimiter(char d)
    {
        return (new string(delimiter_order)).ToString().IndexOf(d);
    }
    private static Ty convert<Tx, Ty>(Tx a)
    {
        return (Ty)System.Convert.ChangeType(a, typeof(Ty));
    }
    private static string SpaceDelim(string s, char delim)
    {
        char[] doubdelim = { delim, delim };
        string ddel = doubdelim.ToString();
        char[] doubdelimspace = { delim, ' ', delim };
        string ddels = doubdelimspace.ToString();
        s = s.Replace(ddel, ddels);
        if (s.IndexOf(delim) == 0)
        {
            s = " " + s;
        }
        if (s.LastIndexOf(delim) == (s.Length - 1))
        {
            s = s + " ";
        }

        return s;
    }

    /// <summary>
    /// Change delimiters and the two derivitive functions are there
    /// to modify the delimiters that are used. Note that '\n' and ',' always
    /// appear consecutively and aren't changable.
    /// </summary>
    /// <param name="newpd">delimiters that should be used before '\n' in order </param>
    /// <param name="newnd">delimiters that should be used after ',' in order </param>
    /// <exception cref="InvalidDelimiterChange"> Thrown if a delimiter would end up
    /// appearing more than once in  the list of delimiters</exception>
    public static void ChangeDelimiters(char[] newpd, char[] newnd)
    {
        ChangePrevDelimiters(newpd);
        ChangeNextDelimiters(newnd);
    }
    public static void ChangePrevDelimiters(char[] newpd)
    {
        string news = new string(newpd);
        string olds = new string(delimiter_order);
        string ans = news + olds.Substring(olds.IndexOf('\n'));

        for (int i = 0; i < delimiter_order.Length; i++)
        {
            if (ans.LastIndexOf(ans[i]) != ans.IndexOf(ans[i]))
            {
                throw new InvalidDelimiterChange();
            }
        }
        delimiter_order = ans.ToCharArray();

    }
    public static void ChangeNextDelimiters(char[] newnd)
    {
        string news = new string(newnd);
        string olds = new string(delimiter_order);
        string ans = olds.Substring(0, olds.IndexOf(',') + 1) + news;

        for (int i = 0; i < delimiter_order.Length; i++)
        {
            if (ans.LastIndexOf(ans[i]) != ans.IndexOf(ans[i]))
            {
                throw new InvalidDelimiterChange();
            }
        }
        delimiter_order = ans.ToCharArray();

    }

    // Note that if an Invalid Delimiter is used, either '\n' or ',' is used depending on the situation.
    private static char ValidDelimiter(char curr)
    {
        return ValidDelimiter(curr, ',');
    }
    private static char ValidDelimiter(char curr, char def)
    {
        int x = find_delimiter(curr);
        if (x != -1)
        {
            return curr;
        }
        else
        {
            return def;
        }
    }
    public static char GetNextDelimiter(char d)
    {
        int x = find_delimiter(d);
        try
        {
            return delimiter_order[x + 1];
        }
        catch
        {
            throw new NoMoreDelimitersException();
        }
    }

    /// <summary>
    /// Turns a string-formated value into the appropriate object
    /// </summary>
    /// <typeparam name="T">The type of object the string represtents</typeparam>
    /// <param name="s">the object in string format</param>
    /// <param name="del">the first delimiting character</param>
    /// <returns>The object in its true form</returns>
    /// <exception cref="UnworkableComponentClass">Thrown if a basic object (like string, int or bool) hasn't been implemented yet.</exception>
    /// <exception cref="NoMoreDelimitersException">If the current model doesn't have enough delimiters.</exception>
    public static T Read<T>(string s, char del)
    {
        char d = ValidDelimiter(del, '\n');

        if (typeof(T).IsArray)
        {
            try
            {
                s = SpaceDelim(s, d);
                string[] part = s.Split(d);
                Array temp = Array.CreateInstance(typeof(T).GetElementType(), part.Length);

                if (typeof(T).GetElementType().IsSubclassOf(typeof(CompObj)))
                {
                    ConstructorInfo DefaultConstructor = typeof(T).GetElementType().GetConstructor(new Type[] { });
                    MethodInfo InstSetup = typeof(T).GetElementType().GetMethod("InstanceSetupFromString", new Type[] { typeof(string), typeof(char) });

                    for (int i = 0; i < temp.Length; i++)
                    {
                        try
                        {
                            var ans = DefaultConstructor.Invoke(new object[] { });
                            InstSetup.Invoke(ans, new object[] { part[i], GetNextDelimiter(d) });
                            temp.SetValue(ans, i);
                        }
                        catch
                        {
                            temp.SetValue(DefaultConstructor.Invoke(new object[] { }), i);
                        }
                    }
                }
                else
                {
                    MethodInfo genMeth = typeof(CSVIO).GetMethod("Read", new Type[] { typeof(string), typeof(char) }).MakeGenericMethod(typeof(T).GetElementType());
                    for (int i = 0; i < temp.Length; i++)
                    {
                        temp.SetValue(genMeth.Invoke(null, new object[] { part[i], GetNextDelimiter(d) }), i);
                    }
                }
                return (T)(object)temp;
            }
            catch
            {
                Array temp = Array.CreateInstance(typeof(T).GetElementType(), 0);
                return (T)(object)temp;
            }

        }
        else if (typeof(T).IsSubclassOf(typeof(CompObj)))
        {
            ConstructorInfo DefaultConstructor = typeof(T).GetConstructor(new Type[] { });
            MethodInfo InstSetup = typeof(T).GetMethod("InstanceSetupFromString", new Type[] { typeof(string), typeof(char) });
            try
            {

                s = SpaceDelim(s, d);
                T ans = (T)DefaultConstructor.Invoke(new object[] { });
                InstSetup.Invoke(ans, new object[] { s, d });
                return ans;
            }
            catch
            {
                return (T)DefaultConstructor.Invoke(new object[] { });
            }
        }
        else if (typeof(T) == typeof(string))
        {
            return convert<string, T>(s.Replace("\r", ""));
        }
        else if (typeof(T) == typeof(int))
        {
            int ans;
            bool success = System.Int32.TryParse(s.Trim(), out ans);

            if (success)
            {
                return convert<int, T>(ans);
            }
            else
            {
                return convert<int, T>(0);
            }
        }
        else if (typeof(T) == typeof(bool))
        {
            if (s.Trim().Equals("1") || s.Trim().Equals("true") || s.Trim().Equals("True") || s.Trim().Equals("TRUE"))
            {
                return convert<bool, T>(true);
            }
            else
            {
                return convert<bool, T>(false);
            }
        }
        else
        {
            throw new UnworkableComponentClass();
        }

    }
    public static T Read<T>(string s)
    {
        if (typeof(T).IsArray)
        {
            return Read<T>(s, '\n');

        }
        else
        {
            return Read<T>(s, ',');
        }
    }





    /// <summary>
    /// Creates a dictionary based on the first 2 columns ofa csv file.
    /// </summary>
    /// <typeparam name="TKey">The Key type</typeparam>
    /// <typeparam name="TValue">The Value type</typeparam>
    /// <param name="filer">The name of the file (in unity, after "Resources/")</param>
    /// <param name="d">The first delimiter</param>
    /// <param name="addval">If there is already a dictionary and you wish to add the pairs in the CSV to it, put it in here (ByVal still)</param>
    /// <returns>A Dictionary including the contents of a CSV</returns>
    /// <exception cref="UnworkableComponentClass">Thrown if a basic object (like string, int or bool) hasn't been implemented yet.</exception>
    /// <exception cref="NoMoreDelimitersException">If the current model doesn't have enough delimiters.</exception>
    public static Dictionary<TKey, TValue> LoadDict<TKey, TValue>(string filer, char d, Dictionary<TKey, TValue> addval)
    {
        Dictionary<TKey, TValue> adict = LoadDict<TKey, TValue>(filer, d);
        foreach (KeyValuePair<TKey, TValue> item in addval)
        {
            if (!adict.ContainsKey(item.Key))
            {
                adict.Add(item.Key, item.Value);
            }
        }
        return adict;
    }
    public static Dictionary<TKey, TValue> LoadDict<TKey, TValue>(string filer)
    {
        return LoadDict<TKey, TValue>(filer, '\n');
    }
    public static Dictionary<TKey, TValue> LoadDict<TKey, TValue>(string filer, char del)
    {
        char d = ValidDelimiter(del, '\n');

        string thedict = Resources.Load<TextAsset>(filer).text.TrimEnd();
        string[] split = thedict.Split(d);
        Dictionary<TKey, TValue> adict = new Dictionary<TKey, TValue>();
        for (int i = 0; i < split.Length; i++)
        {
            string s = SpaceDelim(split[i], d);
            string[] resplit = s.Split(GetNextDelimiter(d));
            TKey k = Read<TKey>(resplit[0], GetNextDelimiter(GetNextDelimiter(d)));
            TValue v = Read<TValue>(resplit[1], GetNextDelimiter(GetNextDelimiter(d)));
            adict.Add(k, v);
        }
        return adict;
    }
    public static Dictionary<TKey, TValue> LoadDict<TKey, TValue>(string filer, (TKey, TValue)[] addval)
    {
        return LoadDict<TKey, TValue>(filer, '\n', addval);
    }
    public static Dictionary<TKey, TValue> LoadDict<TKey, TValue>(string filer, Dictionary<TKey, TValue> addval)
    {
        return LoadDict<TKey, TValue>(filer, '\n', addval);
    }
    public static Dictionary<TKey, TValue> LoadDict<TKey, TValue>(string filer, char d, (TKey, TValue)[] addval)
    {
        Dictionary<TKey, TValue> adict = LoadDict<TKey, TValue>(filer, d);
        foreach ((TKey, TValue) m in addval)
        {
            adict.Add(m.Item1, m.Item2);
        }
        return adict;
    }




    /// <summary>
    /// Creates an array from a CSV file.
    /// </summary>
    /// <typeparam name="T">The type of object in the CSV file</typeparam>
    /// <param name="filer">The name of the file (in unity, after "Resources/")</param>
    /// <param name="del">The first delimiter</param>
    /// <param name="addval">If there is already an array and you wish to add the values in the CSV to it, put it in here (ByVal still)</param>
    /// <returns>A complete array with the values from the CSV</returns>
    /// <exception cref="UnworkableComponentClass">Thrown if a basic object (like string, int or bool) hasn't been implemented yet.</exception>
    /// <exception cref="NoMoreDelimitersException">If the current model doesn't have enough delimiters.</exception>
    public static T[] LoadArray<T>(string filer, char del, T[] addval)
    {
        T[] plus = LoadArray<T>(filer, del);
        T[] tot = new T[addval.Length + plus.Length];
        for (int i = 0; i < addval.Length; i++)
        {
            tot[i] = addval[i];
        }
        for (int i = 0; i < plus.Length; i++)
        {
            tot[i + addval.Length] = plus[i];
        }
        return tot;
    }
    public static T[] LoadArray<T>(string filer)
    {
        return LoadArray<T>(filer, '\n');
    }
    public static T[] LoadArray<T>(string filer, char del)
    {
        char d = ValidDelimiter(del, '\n');

        string thedict = Resources.Load<TextAsset>(filer).text.TrimEnd();
        string[] split = thedict.Split(d);
        T[] tot = new T[split.Length - 1];
        for (int i = 1; i < split.Length; i++)
        {
            string s = SpaceDelim(split[i], d);
            tot[i - 1] = Read<T>(s, GetNextDelimiter(d));
        }
        return tot;
    }



    /// <summary>
    /// Writes out the Array within the developed property-driven format. Uses '\n' as the first delimiter.
    /// </summary>
    /// <typeparam name="T">The Type of object (The element of the array) that is being written out</typeparam>
    /// <param name="ans">the array itself</param>
    /// <returns>a string in the format that this class can interpret.</returns>
    /// <exception cref="UnworkableComponentClass">Thrown if a basic object (like string, int or bool) hasn't been implemented yet.</exception>
    /// <exception cref="NoMoreDelimitersException">If the current model doesn't have enough delimiters.</exception>
    public static string DefaultWriteArray<T>(T[] ans)
    {
        return WriteArray<T>(ans, '\n');
    }

    /// <summary>
    /// Writes out the Array within the developed property-driven format.
    /// </summary>
    /// <typeparam name="T">The Type of object (The element of the array) that is being written out</typeparam>
    /// <param name="ans">the array itself</param>
    /// <param name="del">The first delimiter</param>
    /// <returns>a string in the format that this class can interpret.</returns>
    /// <exception cref="UnworkableComponentClass">Thrown if a basic object (like string, int or bool) hasn't been implemented yet.</exception>
    /// <exception cref="NoMoreDelimitersException">If the current model doesn't have enough delimiters.</exception>
    public static string WriteArray<T>(T[] ans, char del)
    {
        char d = ValidDelimiter(del, '\n');

        string[] comp = new string[ans.Length];

        for (int i = 0; i < comp.Length; i++)
        {
            comp[i] = Write<T>(ans[i], GetNextDelimiter(d));
        }

        string s = string.Join(new string(new char[] { d }), comp);
        return s;
    }

    /// <summary>
    /// Writes out an Object with the developed property-driven format. Uses '\n' as the first delimiter if the 
    /// object is an array and ',' otherwise.
    /// </summary>
    /// <typeparam name="T">The Type of object that is being written out</typeparam>
    /// <param name="ans">the object itself</param>
    /// <returns>a string in the format that this class can interpret.</returns>
    /// <exception cref="UnworkableComponentClass">Thrown if a basic object (like string, int or bool) hasn't been implemented yet.</exception>
    /// <exception cref="NoMoreDelimitersException">If the current model doesn't have enough delimiters.</exception>
    public static string DefaultWrite<T>(T ans)
    {
        if (typeof(T).IsArray)
        {
            return Write<T>(ans, '\n');
        }
        else
        {
            return Write<T>(ans, ',');
        }
    }

    /// <summary>
    /// Writes out an Object with the developed property-driven format. Uses '\n' as the first delimiter if the 
    /// object is an array and ',' otherwise.
    /// </summary>
    /// <typeparam name="T">The Type of object that is being written out</typeparam>
    /// <param name="ans">the object itself</param>
    /// <param name="del">The first delimiter</param>
    /// <returns>a string in the format that this class can interpret.</returns>
    /// <exception cref="UnworkableComponentClass">Thrown if a basic object (like string, int or bool) hasn't been implemented yet.</exception>
    /// <exception cref="NoMoreDelimitersException">If the current model doesn't have enough delimiters.</exception>
    public static string Write<T>(T ans, char del)
    {
        char d = ValidDelimiter(del, '\n');
        if (typeof(T) == typeof(string))
        {
            try { return ans.ToString(); }
            catch { return ""; }
        }
        else if (typeof(T) == typeof(int))
        {
            try { return ans.ToString(); }
            catch { return "0"; }
        }
        else if (typeof(T) == typeof(string) || typeof(T) == typeof(int) || typeof(T) == typeof(bool))
        {
            try { return ans.ToString(); }
            catch { return "False"; }
        }
        else if (typeof(T).IsArray)
        {
            MethodInfo genMeth = typeof(CSVIO).GetMethod("WriteArray").MakeGenericMethod(typeof(T).GetElementType());
            return (string)genMeth.Invoke(null, new object[] { ans, d });
        }
        else if (typeof(T).IsSubclassOf(typeof(CompObj)))
        {

            MethodInfo genMeth = typeof(T).GetMethod("Output", new Type[] { typeof(char)});
            return (string)genMeth.Invoke(ans, new object[] { d });
        }
        else
        {
            throw new UnworkableComponentClass();
        }
    }
}
