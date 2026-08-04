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
        yield return LoadData();
    }

    private IEnumerator LoadData()
    {
        var task = EasyDataSetReader.Index<TestTableData>("new_tableset.tbs", 0, IOType.StreamingAssets);
        yield return task;
        Debug.Log(task.Result);
    }
}