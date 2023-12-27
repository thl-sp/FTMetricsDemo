#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.UI;
using FTOptix.Store;
using FTOptix.ODBCStore;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Core;
#endregion

public class LogicFTMetrics : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void FTMetricsReadOEE()
    {
        // Get the different pieces we need to build the graph
        //IUANode myModelObject = Owner.Get("CarbonByShift");
        var batchID = (String)Owner.GetVariable("SelectBatchID").Value; 
        var workCellName = (String)Owner.GetVariable("SelectWorkCellName").Value;
        var nodata = Owner.GetVariable("NoData1");
        DateTime select_date = Owner.GetVariable("SelectDay").Value;
        int year = select_date.Year;
        int month = select_date.Month;
        int day = select_date.Day;
        Store myDbStore = InformationModel.Get<Store>(Owner.GetVariable("MyDatabase").Value);
        /*string sqlQuery = $"SELECT Shift,SUM(RateToCarbon) AS Value FROM RecordShiftEnergy " +
            $"WHERE Group=\'{group}\' AND Year={year} AND Month={month} AND Day={day} GROUP BY Shift";*/

        string sqlQuery = $"SELECT OEE,Availability,Performance,Quality,BatchId FROM FTOptix_GetOeeData " +
            $"WHERE BatchId=\'{batchID}\' AND WorkCellName='{workCellName}'";

        // Prepare SQL Query
        // Execute query and check result
        try
        {
            //PieChart myChart = (PieChart)Owner.GetObject("CarbonByShiftChart");
            Object[,] ResultSet;
            String[] Header;
            myDbStore.Query(sqlQuery, out Header, out ResultSet);
            if (ResultSet.GetLength(0) < 1)
            {
                nodata.Value = true;
                Log.Error(LogicObject.BrowseName, "Input query returned less than one line");
                return;
            }
            nodata.Value = false;
            // Delete all children from Object
            /*foreach (var children in myModelObject.Children)
            {
                children.Delete();
            }*/
            // For each column create an Object children
            for (int i = 0; i < ResultSet.GetLength(0); i++)
            {
                /*String columnName = "Shift_" + Convert.ToString(ResultSet[i, 0]);
                var myObj = InformationModel.MakeVariable(columnName, OpcUa.DataTypes.String);
                myObj.Value = Convert.ToDouble(ResultSet[i, 1]);
                myModelObject.Add(myObj);*/

                Log.Info(LogicObject.BrowseName, $"OEE = '{ResultSet[i, 0]}' - BatchId = '{ResultSet[i, 1]}'");

                Project.Current.GetObject("Model/OEE").GetVariable("Data" + (i + 1)).Value = Convert.ToString(ResultSet[i, 0]);
                Project.Current.GetObject("Model/Availability").GetVariable("Data" + (i + 1)).Value = Convert.ToString(ResultSet[i, 1]);
                Project.Current.GetObject("Model/Performance").GetVariable("Data" + (i + 1)).Value = Convert.ToString(ResultSet[i, 2]);
                Project.Current.GetObject("Model/Quality").GetVariable("Data" + (i + 1)).Value = Convert.ToString(ResultSet[i, 3]);

            }
            //myChart.Refresh();
        }
        catch (Exception ex)
        {
            Log.Error("LogicDeviceDaily", ex.Message);
            return;
        }
    }
}
