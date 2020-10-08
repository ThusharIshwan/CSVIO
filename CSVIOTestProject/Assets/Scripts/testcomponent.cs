using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testcomponent : MonoBehaviour
{
    void Start()
    {

        //Just a couple of test objects.

        TestCompObj tcox = new TestCompObj();
        TestCompObj tcoy = new TestCompObj("");
        TestCompObj tcoz = new TestCompObj("23,Hello,1");
        TestCompObj[] tcoA = CSVIO.Read<TestCompObj[]>("45,Hello!!!, 1\n56,Hello, True");
        TestCompObjDerived tcoDx = new TestCompObjDerived("55,233,Hello!,1");
        TestCompObjDerived[] tcoDA = CSVIO.Read<TestCompObjDerived[]>("553,45,Hello!!!,1\n0,56,Hello,True\n\n");
        TestCompObjDerived[] tcoDAwithErr = CSVIO.Read<TestCompObjDerived[]>("5,4,neat,1\n0,56,Hello,True\n234ddsasdwq3\n");
        TestCompObjProp tcoPx = new TestCompObjProp("2010$Ohlo!$1,55");
        TestCompObjProp[] TCOPA = CSVIO.Read<TestCompObjProp[]>("2010$Ohlo!$1,55\n665$Hola!$1,34");

        TestEvent[] Events = CSVIO.LoadArray<TestEvent>("TestEventList");

        Debug.Log(Events[0].Output());
    }

}
