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
            var arrayDebugString = "[";
            if (this.intArray != null)
            {
                for (var i = 0; i < this.intArray.Length; i++)
                {
                    if (i == this.intArray.Length - 1)
                    {
                        arrayDebugString += $"{this.intArray[i]}";
                    }
                    else
                    {
                        arrayDebugString += $"{this.intArray[i]}, ";
                    }
                }
            }
            arrayDebugString += "]";
            return $"|{this.id}|{this.name}|{arrayDebugString}";
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
        var task = EasyDataSetReader.Index<TestTableData>("new_tableset", 0, IOType.StreamingAssets);
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

    private IEnumerator TestCase3()
    {
        // OPTIMAL
        var task = EasyDataSetReader.Index<TestTableData>("new_tableset", 0);
        yield return task;
        Debug.Log(task.Result);
    }
}