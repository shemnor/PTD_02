using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using PS = PTD_02.Properties.Settings;
using System.Configuration;
using System.Windows.Forms;
using System.IO;


using System.Reflection;

using System.ComponentModel;

using System.Drawing;
using System.Drawing.Text;

using Tekla.Structures;
using Tekla.Structures.DrawingInternal;
using TSDrg = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using TSG = Tekla.Structures.Geometry3d;
using Tekla.Structures.Drawing;

namespace PTD_02
{
    public static class helpers
    {
        public enum dialogType
        {
            bar = 1,
            coupler = 2
        }

        public static void applyUserProperties(DataTable userSettings)
        {
            foreach (DataRow row in userSettings.Rows)
            {
                switch (PS.Default[row[0].ToString()])
                {
                    case String t1:
                        PS.Default[row[0].ToString()] = row.Field<String>(1);
                        break;
                    case Double t2:
                        PS.Default[row[0].ToString()] = Double.Parse(row[1].ToString());
                        break;
                    case int t3:
                        PS.Default[row[0].ToString()] = (row.Field<int>(1));
                        break;
                }
            }
        }

        public static void readUserProperties(ref DataTable userSettings)
        {
            foreach (SettingsProperty property in PS.Default.Properties)
            {
                DataRow row = userSettings.NewRow();
                row[0] = property.Name;
                row[1] = PS.Default[property.Name].ToString();

                userSettings.Rows.Add(row);
            }
        }

        public static Tuple<List<string>, List<string>> parse2valueCSV(string path)
        {
            using (var reader = new StreamReader(path))
            {
                List<string> listA = new List<string>();
                List<string> listB = new List<string>();
                while (!reader.EndOfStream)
                {
                    var values = reader.ReadLine().Split(',');

                    listA.Add(values[0]);
                    listB.Add(values[1]);
                }
                return new Tuple<List<string>, List<string>>(listA, listB);
            }

        }

        public static Tuple<List<string>, List<string>> splitCsvFromString(string text, char delimeter)
        {
            var lines = text.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);
            List<string> listA = new List<string>();
            List<string> listB = new List<string>();
            for (int i = 0; i < lines.Length-1; i++)
            {
                var values = lines[i].Split(delimeter);
                listA.Add(values[0]);
                listB.Add(values[1]);
            }
            return new Tuple<List<string>, List<string>>(listA, listB);
        }

        /*
        public static void changeDialog(string attributeFileName, dialogType objectType)
        {
            if (!TeklaStructures.Connect()) return;
            var macroBuilder = new MacroBuilder();

            switch (objectType)
            {
                case dialogType.bar:
                    macroBuilder.Callback("acmd_display_selected_drawing_object_dialog", "", "View_10 window_1");
                    macroBuilder.ValueChange("rebar_dial", "gr_rebar_get_menu", attributeFileName);
                    macroBuilder.PushButton("gr_rebar_get", "rebar_dial");
                    macroBuilder.PushButton("rebar_modify", "rebar_dial");
                    macroBuilder.Run();
                    break;
                case dialogType.coupler:
                    macroBuilder.Callback("acmd_display_selected_drawing_object_dialog", "", "View_10 window_1");
                    macroBuilder.ValueChange("part_dial", "gr_part_get_menu", attributeFileName);
                    macroBuilder.PushButton("gr_part_get", "part_dial");
                    macroBuilder.PushButton("part_modify", "part_dial");
                    macroBuilder.Run();
                    break;
                default:
                    break;
            }
        }

        public static void changeSelectionFilter(string filterName, bool partOnly)
        {
            if (!TeklaStructures.Connect()) return;
            var macroBuilder = new MacroBuilder();
            macroBuilder.Callback("acmd_display_gr_select_filter_dialog", "", "main_frame");
            macroBuilder.ValueChange("diaSelDrawingObjectGroupDialogInstance", "get_menu", filterName);
            macroBuilder.PushButton("dia_pa_apply", "diaSelDrawingObjectGroupDialogInstance");
            if (partOnly)
            {
                macroBuilder.ValueChange("main_frame", "gr_sel_all", "0");
                macroBuilder.ValueChange("main_frame", "gr_sel_drawing_part", "1");
            }
            else
            {
                macroBuilder.ValueChange("main_frame", "gr_sel_all", "1");
            }
            macroBuilder.Run();
        }

        public static void addMark(string attributeName, bool customAmount, string barNo = null)
        {
            if (!TeklaStructures.Connect()) return;
            var macroBuilder = new MacroBuilder();
            if (customAmount)
            {
                macroBuilder.Callback("acmd_create_marks_selected", "", "View_10 window_1");
                macroBuilder.ValueChange("rebar_mark_dial", "gr_rebar_mark_get_menu", attributeName);
                macroBuilder.PushButton("gr_rebar_get", "rebar_mark_dial");
                macroBuilder.TableSelect("rebar_mark_dial", "gr_mark_selected_elements", new int[] { 1 });
                macroBuilder.Activate("rebar_mark_dial", "gr_mark_selected_elements");
                macroBuilder.ValueChange("gr_mark_text", "gr_text", barNo);
                macroBuilder.PushButton("gr_mark_prompt_modify", "gr_mark_text");
                macroBuilder.PushButton("rebar_mark_modify", "rebar_mark_dial");
                macroBuilder.Run();
            }
            else
            {
                macroBuilder.Callback("acmd_create_marks_selected", "", "View_10 window_1");
                macroBuilder.ValueChange("rebar_mark_dial", "gr_rebar_mark_get_menu", attributeName);
                macroBuilder.PushButton("gr_rebar_get", "rebar_mark_dial");
                macroBuilder.PushButton("rebar_mark_modify", "rebar_mark_dial");
                macroBuilder.Run();
            }
        }

        public static List<string> readBarInformation()
        {
            ReinforcementBase dwgRebar = getSingleObjectFromSelection<ReinforcementBase>() as ReinforcementBase;
            List<string> info = new List<string>();

            if (dwgRebar == null)
            {
                dwgRebar = getSingleObjectFromUser<ReinforcementBase>() as ReinforcementBase;
            }
            if (dwgRebar != null)
            {
                TSM.Model MyModel = new TSM.Model();
                TSM.ModelObject modelRebar = MyModel.SelectModelObject(dwgRebar.ModelIdentifier);
                TSM.Phase rebarPhase = new TSM.Phase();
                modelRebar.GetPhase(out rebarPhase);
                info.Add(rebarPhase.PhaseName);
                string shapeCode = null;
                bool gotProperty = modelRebar.GetReportProperty("REBAR_POS", ref shapeCode);
                info.Add(shapeCode);
            }
            else
            {
                info = null;
            }
            return info;
        }

        public static bool drawCloud()
        {
            bool complete = false;
            Tuple<TSDrg.PointList, TSDrg.View> usrInp = getRectangleCornersFromUser();
            if (usrInp.Item1 != null)
            {
                TSDrg.PointList rectangle = calculateRectanlgeFromCorners(usrInp.Item1);
                TSDrg.Cloud cloud = new TSDrg.Cloud(usrInp.Item2, rectangle, new Cloud.CloudAttributes(PS.Default.usr_Cloud));
                cloud.Attributes.Line.Color = TSDrg.DrawingColors.Red;
                cloud.Insert();
                usrInp.Item2.GetDrawing().CommitChanges();
                complete = true;
            }
            return complete;
        }

        public static bool addFCRNote()
        {
            bool complete = false;
            //ADD REBAR FCR NOTE
            ReinforcementBase usrReinforcement = getSingleObjectFromSelection<ReinforcementBase>() as ReinforcementBase;
            if (usrReinforcement == null) usrReinforcement = getSingleObjectFromUser<ReinforcementBase>() as ReinforcementBase;

            if (usrReinforcement != null)
            {
                ViewBase userView = usrReinforcement.GetView();
                Tuple<TSG.Point, TSDrg.View> userBasePoint = getPointFromUser();
                string fcrNumber = getPhaseNameFromReinforcement(usrReinforcement);
                fcrNumber = fcrNumber.Trim();
                if (fcrNumber.LastIndexOf(" ") > 0)
                    fcrNumber = fcrNumber.Substring(0, fcrNumber.LastIndexOf(" ", fcrNumber.Length));
                TSDrg.Text fcrNote = new TSDrg.Text(userView, userBasePoint.Item1, fcrNumber, new TSDrg.Text.TextAttributes(PS.Default.usr_FcrNote));
                fcrNote.Insert();
                userView.GetDrawing().CommitChanges();
                complete = true;
            }
            return complete;
        }

        public static void alignMarks()
        {
            List<DrawingObject> usrObjects = getObjectsFromSelection(typeof(MarkBase));
            if (usrObjects == null) return;
            List<MarkBase> usrBarMarks = new List<MarkBase>();
            foreach (MarkBase mark in usrObjects)
            {
                usrBarMarks.Add(mark);
            }

            MarkBase baseMark = getSingleObjectFromUser<MarkBase>() as MarkBase;
            if (baseMark == null) return;

            TSG.Point basePoint = baseMark.InsertionPoint;
            MarkBase[] sortedBarMarks = usrBarMarks.ToArray();
            Double[] sortedBarMarkXpos = new Double[usrBarMarks.Count];
            bool reversed = false;

            TSG.Point temp = new TSG.Point();
            //create array of barmark positions for sorting
            for (int i = 0; i < usrBarMarks.Count; i++)
            {
                TSDrg.LeaderLinePlacing placing = (LeaderLinePlacing)usrBarMarks[i].Placing;
                drawcircle(placing.StartPoint, sortedBarMarks[i].GetView());
                temp = placing.StartPoint;
                sortedBarMarkXpos[i] = placing.StartPoint.X;
            }

            //sort and check for position of the basemark, if basemark is at the end, then reverse order of marks for sorting
            Array.Sort(sortedBarMarkXpos, sortedBarMarks);
            if (basePoint.X > sortedBarMarkXpos.Last())
            {
                sortedBarMarks.Reverse();
                sortedBarMarkXpos.Reverse();
                reversed = true;
            }

            //move barmarks
            Double newInsertionYpos = basePoint.Y - PS.Default.usr_BarMarkYOffset;
            for (int i = 0; i < sortedBarMarks.Length; i++)
            {

                TSG.Point currBarMarkPosition = sortedBarMarks[i].InsertionPoint;
                double newInsertionXpos = 0.0;
                if (reversed == true)
                {
                    newInsertionXpos = sortedBarMarkXpos[i] + 75 + sortedBarMarks[i].GetObjectAlignedBoundingBox().Width / 2;
                }
                else
                {
                    newInsertionXpos = sortedBarMarkXpos[i] - 75 - sortedBarMarks[i].GetObjectAlignedBoundingBox().Width / 2;
                }

                TSG.Point newInsertionPoint = new TSG.Point(newInsertionXpos, newInsertionYpos);
                TSG.Vector moveVector = new TSG.Vector(
                    newInsertionPoint.X - currBarMarkPosition.X,
                    newInsertionPoint.Y - currBarMarkPosition.Y,
                    0.0);
                sortedBarMarks[i].MoveObjectRelative(moveVector);
                sortedBarMarks[i].Modify();
                //newInsertionYpos += Double.Parse(PS.Default.usr_BarMarkYOffset);
                newInsertionYpos -= PS.Default.usr_BarMarkYOffset;
            }
        }

        public static TSDrg.PointList calculateRectanlgeFromCorners(TSDrg.PointList cornerList)
        {
            PointList rectangle = new PointList();

            TSG.Point[] corners = cornerList.ToArray();
            rectangle.Add(corners[0]);
            rectangle.Add(new TSG.Point(corners[1].X, corners[0].Y, 0.0));
            rectangle.Add(corners[1]);
            rectangle.Add(new TSG.Point(corners[0].X, corners[1].Y, 0.0));

            return rectangle;
        }

        public static string getPhaseNameFromReinforcement(ReinforcementBase drawingObject)
        {
            TSDrg.ReinforcementBase dwgRebar = drawingObject as TSDrg.ReinforcementBase;
            TSM.Model MyModel = new TSM.Model();
            TSM.ModelObject modelRebar = MyModel.SelectModelObject(dwgRebar.ModelIdentifier);
            TSM.Phase rebarPhase = new TSM.Phase();
            modelRebar.GetPhase(out rebarPhase);
            return rebarPhase.PhaseName;
        }

        public static DrawingObject getSingleObjectFromUser<T>()
        {
            DrawingHandler myDrawingHandler = new TSDrg.DrawingHandler();
            TSDrg.UI.Picker pointPicker = myDrawingHandler.GetPicker();
            DrawingObject usrObject = null;
            ViewBase myViewBase = null;
            TSDrg.View myView = myViewBase as TSDrg.View;

            try
            {
                pointPicker.PickObject("Please select a bar", out usrObject, out myViewBase);
                myView = myViewBase as TSDrg.View;

                while (myView == null || !(usrObject is T))
                {
                    pointPicker.PickObject("Please select a bar", out usrObject, out myViewBase);
                    myView = myViewBase as TSDrg.View;
                }

                return usrObject;
            }

            catch (Tekla.Structures.Drawing.PickerInterruptedException interrupted)
            {
                //Tekla.Structures.Model.Operations.Operation.DisplayPrompt("THIS METHOD NOT WORKING BECAUSE TEKLA API IS THE WORST THING I HAVE EVER WORKED WITH");
                lbl_info.Text = "User interrupted action.";
                return usrObject;
            }
        }

        public static DrawingObject getSingleObjectFromSelection<T>()
        {
            TSDrg.DrawingHandler drawingHandler = new TSDrg.DrawingHandler();
            TSDrg.DrawingObjectEnumerator dwgObjectEnumerator;

            dwgObjectEnumerator = drawingHandler.GetDrawingObjectSelector().GetSelected();

            if (dwgObjectEnumerator.GetSize() > 1)
            {
                lbl_info.Text = "Please select only one object";
                return null;
            }
            else if (dwgObjectEnumerator.GetSize() == 0)
            {
                lbl_info.Text = "Please select one object";
                return null;
            }

            foreach (DrawingObject drawingObject in dwgObjectEnumerator)
            {
                if (drawingObject != null && drawingObject.GetType().IsSubclassOf(typeof(T)))
                {
                    return drawingObject;
                }
            }
            return null;
        }

        public static List<DrawingObject> getObjectsFromSelection(Type filterType)
        {
            TSDrg.DrawingHandler drawingHandler = new TSDrg.DrawingHandler();
            TSDrg.DrawingObjectEnumerator dwgObjectEnumerator;
            List<DrawingObject> dwgObjs = new List<DrawingObject>();

            dwgObjectEnumerator = drawingHandler.GetDrawingObjectSelector().GetSelected();

            if (dwgObjectEnumerator.GetSize() == 0)
            {
                lbl_info.Text = "Please select at least one object";
                return null;
            }
            foreach (DrawingObject drawingObject in dwgObjectEnumerator)
            {
                if (drawingObject != null && drawingObject.GetType().IsSubclassOf(filterType))
                {
                    dwgObjs.Add(drawingObject);
                }
            }
            return dwgObjs;

        }

        public static Tuple<TSG.Point, TSDrg.View> getPointFromUser()
        {
            TSDrg.DrawingHandler myDrawingHandler = new TSDrg.DrawingHandler();
            TSDrg.UI.Picker pointPicker = myDrawingHandler.GetPicker();
            TSG.Point myPoint = null;
            TSDrg.ViewBase myViewBase = null;
            TSDrg.View myView = myViewBase as TSDrg.View;

            try
            {
                pointPicker.PickPoint("Pick a point to insert an FCR note", out myPoint, out myViewBase);
                myView = myViewBase as TSDrg.View;

                while (myView == null)
                {
                    pointPicker.PickPoint("Selected point is not inside a view. Pick a point to insert an FCR note", out myPoint, out myViewBase);
                    myView = myViewBase as TSDrg.View;
                }

                return new Tuple<TSG.Point, TSDrg.View>(myPoint, myView);
            }

            catch (Tekla.Structures.Drawing.PickerInterruptedException interrupted)
            {
                //Tekla.Structures.Model.Operations.Operation.DisplayPrompt("THIS METHOD NOT WORKING BECAUSE TEKLA API IS THE WORST THING I HAVE EVER WORKED WITH");
                lbl_info.Text = "User interrupted action.";
                return new Tuple<TSG.Point, TSDrg.View>(myPoint, myView);
            }
        }

        public static Tuple<TSDrg.PointList, TSDrg.View> getRectangleCornersFromUser()
        {
            TSDrg.DrawingHandler myDrawingHandler = new TSDrg.DrawingHandler();
            TSDrg.UI.Picker pointPicker = myDrawingHandler.GetPicker();
            TSDrg.PointList myPoints = null;
            TSDrg.ViewBase myViewBase = null;
            TSDrg.View myView = myViewBase as TSDrg.View;
            TSDrg.StringList promptMsg = new TSDrg.StringList();
            promptMsg.Add("Select first corner of the rectangle");
            promptMsg.Add("Select second corner of the rectangle");

            try
            {
                pointPicker.PickPoints(2, promptMsg, out myPoints, out myViewBase);
                myView = myViewBase as TSDrg.View;

                while (myView == null)
                {
                    pointPicker.PickPoints(2, promptMsg, out myPoints, out myViewBase);
                    myView = myViewBase as TSDrg.View;
                }

                return new Tuple<TSDrg.PointList, TSDrg.View>(myPoints, myView);
            }

            catch (Tekla.Structures.Drawing.PickerInterruptedException interrupted)
            {
                //Tekla.Structures.Model.Operations.Operation.DisplayPrompt("THIS METHOD NOT WORKING BECAUSE TEKLA API IS THE WORST THING I HAVE EVER WORKED WITH");
                lbl_info.Text = "User interrupted action.";
                return new Tuple<TSDrg.PointList, TSDrg.View>(myPoints, myView);
            }
        }

        public static void adjustBBToInsertionPoint(ref RectangleBoundingBox boundingBox, TSG.Point insertionPoint)
        {
            if (insertionPoint.X > boundingBox.GetCenterPoint().X)
            {
                boundingBox.LowerRight.X += 75;
                boundingBox.UpperRight.X += 75;
            }
            else if (insertionPoint.X < boundingBox.GetCenterPoint().X)
            {
                boundingBox.LowerLeft.X -= 75;
                boundingBox.UpperLeft.X -= 75;
            }
        }

        public static void drawcircle(TSG.Point centre, ViewBase view)
        {
            TSDrg.Circle circle = new TSDrg.Circle(view, centre, 50.0);
            circle.Insert();
        }

        static void DrawBB(ViewBase DrawView, RectangleBoundingBox boundingBox, DrawingColors Color)
        {
            PointList points = new PointList();
            points.Add(boundingBox.LowerLeft);
            points.Add(boundingBox.UpperLeft);
            points.Add(boundingBox.UpperRight);
            points.Add(boundingBox.LowerRight);
            Polygon MyPolygon = new Polygon(DrawView, points);
            MyPolygon.Attributes.Line.Color = Color;
            MyPolygon.Insert();
        }
    }*/
    }
}
