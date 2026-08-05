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
        [TableField("IntArr")]
        public int[] intArray;

        public override string ToString()
        {
            var arrayDebugString = "[" + string.Join(", ", this.intArray) + "]";
            return $"|{this.id}|{this.name}|{arrayDebugString}";
        }
    }

    private IEnumerator Start()
    {
        yield return TestCase3();
    }

    private IEnumerator TestCase3()
    {
        // OPTIMAL
        var task = EasyDataSetReader.Index<TestTableData>("new_tableset", 0);
        yield return task;
        Debug.Log(task.Result);
    }

    private IEnumerator TestCase2()
    {
        // SLOW
        var task = EasyDataSetReader.Index<TestTableData>("new_tableset", 0, IOType.Resources);
        yield return task;
        Debug.Log(task.Result);
    }

    private IEnumerator TestCase1()
    {
        // SLOW
        var task = EasyDataSetReader.Index<TestTableData>("new_tableset", 0, IOType.StreamingAssets);
        yield return task;
        Debug.Log(task.Result);
    }
}