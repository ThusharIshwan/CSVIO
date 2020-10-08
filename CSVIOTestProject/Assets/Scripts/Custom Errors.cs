using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvalidDefaultLengthAttempt : Exception
{
    public InvalidDefaultLengthAttempt() : base("Please make sure that DefalutValues() has the same number of objects as the number of parameters.") { }
}

public class InvalidDefaultTypesAttempt : Exception
{
    public InvalidDefaultTypesAttempt() : base("Please make sure that objects in DefalutValues() match the property types in the correct order.\nRemeber that properties defined in derived classes appear at the beginning of the list.") { }
}

public class NoMoreDelimitersException : Exception
{
    public NoMoreDelimitersException() : base("You have run out of Delimiters. Please use CSVIO.ChangeNextDelimiters() to update which delimiters you wish to use.") { }
}

public class UnworkableComponentClass : Exception
{
    public UnworkableComponentClass() : base("The components of your structure include a type that has not been implemented yet. As of the base version, only strings ints and bools are usable as composite object components.") { }
}

public class InvalidDelimiterChange : Exception
{
    public InvalidDelimiterChange() : base("You attempted to change the delimiter pattern int such a way that would make some delimiter appear twice.") { }
}

