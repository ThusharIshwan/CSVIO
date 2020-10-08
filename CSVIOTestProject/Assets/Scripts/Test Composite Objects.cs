using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCompObj : CompObj
{
    public int ID { get; set; }
    public string Name { get; set; }
    public bool isTest { get; set; }

    public TestCompObj() : base() { }
    public TestCompObj(string s) : base(s) { }
    public TestCompObj(string s, char c) : base(s, c) { }
    public override object[] DefaultValues()
    {
        return new object[] { 0, "default", false };
    }

}

public class TestCompObjDerived : TestCompObj
{
    public int derivedID { get; set; }

    public override object[] DefaultValues()
    {
        return new object[] { 0, 0, "default", false };
    }
    public TestCompObjDerived(string s, char c) : base(s, c) { }
    public TestCompObjDerived(string s) : base(s) { }
    public TestCompObjDerived() : base() { }
}

public class TestCompObjProp : CompObj
{
    public TestCompObj test { get; set; }
    public int paramID { get; set; }

    public TestCompObjProp(string s, char c) : base(s, c) { }
    public TestCompObjProp(string s) : base(s) { }
    public TestCompObjProp() : base() { }
    public override object[] DefaultValues()
    {
        return new object[] { new TestCompObj(), 1 };
    }
}


public class TestEvent : CompObj
{
    public int ID { get; set; }
    public string name { get; set; }
    public string giver { get; set; }
    public string description { get; set; }
    public int status { get; set; }
    public int[] prerequisite { get; set; }
    public int rewardVal { get; set; }

    public override object[] DefaultValues()
    {
        return new object[] { -1, "unknown", "unknown", "no description", -1, new int[] { }, 0 };
    }

    public TestEvent() : base() { }
    public TestEvent(string s) : base(s) { }
    public TestEvent(string s, char c) : base(s, c) { }

    private string statuscheck()
    {
        switch (status)
        {
            case 0:
                return "Not Started";
            case 1:
                return "In Progress";
            case 2:
                return "Completed";
            default:
                return "Not Found";
        }
    }

    public string CustomOutput()
    {
        return "Quest " + ID + ": " + name + "\nGiven by: " + giver + "\n" + description
            + "\nStatus: " + statuscheck() + "\nReward: $" + rewardVal;
    }
}

