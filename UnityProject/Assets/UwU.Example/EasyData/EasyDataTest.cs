using System.Collections;
using UnityEngine;
using UwU.EasyData;
using UwU.EasyData.Attributes;
using UwU.IO;

public class EasyDataTest : MonoBehaviour
{
    [TableData("MasterTable")]
    private class TestTableData
    {
        [TableField("Id")]
        public int id;
        [TableField("Name")]
        public string name;

        public override string ToString()
        {
            return $"|{this.id}|{this.name}|";
        }
    }

    private IEnumerator Start()
    {
        yield return TestCase1();
        yield return TestCase2();
        yield return TestCase3();
    }

    private IEnumerator TestCase1()
    {
        // SLOW
        var task = EasyDataSetReader.Index<TestTableData>("new_tableset", 3, IOType.StreamingAssets);
        yield return task;
        Debug.Log(task.Result);
    }

    private IEnumerator TestCase2()
    {
        // SLOW
        var task = EasyDataSetReader.Index<TestTableData>("new_tableset", 3, IOType.Resources);
        yield return task;
        Debug.Log(task.Result);
    }

    private IEnumerator TestCase3()
    {
        // OPTIMAL
        var task = EasyDataSetReader.Index<TestTableData>("new_tableset", 3);
        yield return task;
        Debug.Log(task.Result);
    }
}