using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Tekla.Structures;
using Tekla.Structures.DrawingInternal;
using TSDrg = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using TSG = Tekla.Structures.Geometry3d;
using Tekla.Structures.Drawing;
using PS = PTD_02.Properties.Settings;
using System.IO;
using System.Runtime.InteropServices;
using System.CodeDom;

namespace PTD_02

{
    public partial class Form1 : Form
    {
        enum dialogType
        {
            bar = 1,
            coupler = 2
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void resetForm()
        {
            txtb_barmark.Text = "N/A";
            txtb_phase.Text = "N/A";
            lbl_info.Text = "";
        }

        private void btn_settings_Click(object sender, EventArgs e)
        {
            Form form2 = new Form2();
            form2.Show();
        }

        private void btn_Hidebars_Click(object sender, EventArgs e)
        {
            if (!cbx_outlineBars.Checked)
            {
                changeDialog(PS.Default.usr_Hidebars, dialogType.bar);
            }
            else
            {
                changeDialog(PS.Default.usr_Outlinebars, dialogType.bar);
            }
        }

        private void btn_Showbars_Click(object sender, EventArgs e)
        {
            changeDialog(PS.Default.usr_Showbars, dialogType.bar);
        }

        private void btn_Hidecplr_Click(object sender, EventArgs e)
        {
            if (!cbx_outlinePart.Checked)
            {
                changeDialog(PS.Default.usr_Hidepart, dialogType.coupler);
            }
            else
            {
                changeDialog(PS.Default.usr_Outlinepart, dialogType.coupler);
            }
        }

        private void btn_Showcplr_Click(object sender, EventArgs e)
        {
            changeDialog(PS.Default.usr_Showpart, dialogType.coupler);

        }

        private void btn_Added_Click(object sender, EventArgs e)
        {
            if (cbx_addedLinks.Checked && cbx_addedBars.Checked)
            {
                changeSelectionFilter(PS.Default.usr_FilterAdded, true);
            }
            else if (cbx_addedBars.Checked && !cbx_addedLinks.Checked)
            {
                changeSelectionFilter(PS.Default.usr_FilterAddedBars, true);
            }
            else if (cbx_addedLinks.Checked && !cbx_addedBars.Checked)
            {
                changeSelectionFilter(PS.Default.usr_FilterAddedLinks, true);
            }
            else
            {
                changeSelectionFilter("standard", false);
            }
        }

        private void btn_Deleted_Click(object sender, EventArgs e)
        {
            if (cbx_deletedLinks.Checked && cbx_deletedBars.Checked)
            {
                changeSelectionFilter(PS.Default.usr_FilterDeleted, true);
            }
            else if (cbx_deletedBars.Checked && !cbx_deletedLinks.Checked)
            {
                changeSelectionFilter(PS.Default.usr_FilterDeletedBars, true);
            }
            else if (cbx_deletedLinks.Checked && !cbx_deletedBars.Checked)
            {
                changeSelectionFilter(PS.Default.usr_FilterDeletedLinks, true);
            }
            else
            {
                changeSelectionFilter("standard", false);
            }
        }

        private void btn_Standard_Click(object sender, EventArgs e)
        {
            changeSelectionFilter("standard", false);
        }

        private void btn_addMark_Click(object sender, EventArgs e)
        {
            resetForm();
            addMark(PS.Default.usr_BmSimple, false);
        }

        private void btn_addLongMark_Click(object sender, EventArgs e)
        {
            resetForm();
            addMark(PS.Default.usr_BmLong, false);
        }

        private void btn_addLongMarkCust_Click(object sender, EventArgs e)
        {
            resetForm();
            addMark(PS.Default.usr_BmLongCust, true, txtb_barMarkAmount.Text);
        }

        private void btn_read_Click(object sender, EventArgs e)
        {
            resetForm();

            ReinforcementBase dwgRebar = getSingleObjectFromSelection<ReinforcementBase>() as ReinforcementBase;
            if (dwgRebar == null)
            {
                dwgRebar = getSingleObjectFromUser<ReinforcementBase>() as ReinforcementBase;
            }

            List<string> barInfo = new List<string>();
            if (dwgRebar != null)
            {
                barInfo = readBarInformation(dwgRebar);
            }

            if (barInfo != null)
            {
                txtb_phase.Text = barInfo[0];
                txtb_barmark.Text = barInfo[1];
            }
            else
            {
                lbl_info.Text = "Please select one bar";
            }
        }

        private void btn_addCloud_Click(object sender, EventArgs e)
        {
            resetForm();

            var(points, view) = getRectangleCornersFromUser();

            if (points != null)
            {
                TSDrg.PointList rectangle = calculateRectanlgeFromCorners(points);
                drawCloud(rectangle,view, PS.Default.usr_Cloud);
            }
            else
            {
                lbl_info.Text = "Action interrupted;";
            }
        }

        private void btn_addFcrNote_Click(object sender, EventArgs e)
        {
            bool complete = false;
            resetForm();
            ReinforcementBase usrReinforcement = getSingleObjectFromSelection<ReinforcementBase>() as ReinforcementBase;
            if (usrReinforcement == null) usrReinforcement = getSingleObjectFromUser<ReinforcementBase>() as ReinforcementBase;

            if (usrReinforcement != null)
            {
                var (usrPoint, usrView) = getPointFromUser();
                if (usrPoint != null)
                {
                    addFCRNote(usrView, usrPoint, usrReinforcement);
                    complete = true;
                }
            }
            if (!complete) lbl_info.Text = "Action interrupted.";
        }

        private void button404_Click(object sender, EventArgs e)
        {
            resetForm();
            try
            {
                List<DrawingObject> usrObjects = getObjectsFromSelection(typeof(MarkBase));
                List<MarkBase> usrBarMarks = new List<MarkBase>();
                foreach (MarkBase mark in usrObjects)
                {
                    usrBarMarks.Add(mark);
                }
                MarkBase usrBaseMark = getSingleObjectFromUser<MarkBase>() as MarkBase;

                alignMarks(usrBaseMark, usrBarMarks);
                
                
            }
            catch(NullReferenceException nullEx)
            {
                lbl_info.Text = "Action interrupted.";
            }

        }

        private void btn_alignSeries_Click(object sender, EventArgs e)
        {
            resetForm();
            try
            {
                List<DrawingObject> usrObjects = getObjectsFromSelection(typeof(MarkBase));
                List<MarkBase> usrBarMarks = new List<MarkBase>();
                foreach (MarkBase mark in usrObjects)
                {
                    usrBarMarks.Add(mark);
                }
                MarkBase usrBaseMark = getSingleObjectFromUser<MarkBase>() as MarkBase;

                alignMarks(usrBaseMark, usrBarMarks);


            }
            catch (NullReferenceException nullEx)
            {
                lbl_info.Text = "Action interrupted.";
            }
        }

        private void btn_alignHorz_Click(object sender, EventArgs e)
        {
            resetForm();
            try
            {
                List<DrawingObject> usrObjects = getObjectsFromSelection(typeof(MarkBase));
                List<MarkBase> usrBarMarks = new List<MarkBase>();
                foreach (MarkBase mark in usrObjects)
                {
                    usrBarMarks.Add(mark);
                }

                alignMarksHorizontal(usrBarMarks);
                usrBarMarks[0].GetView().GetDrawing().CommitChanges();
            }
            catch (NullReferenceException nullEx)
            {
                lbl_info.Text = "Action interrupted.";
            }
        }


        /// <summary>*******************************************************************
        /// 
        /// 
        /// HELPER METHODS
        /// 
        /// 
        /// </summary>*******************************************************************

        private void changeDialog(string attributeFileName, dialogType objectType)
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

        private void changeSelectionFilter(string filterName, bool partOnly)
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

        private void addMark(string attributeName, bool customAmount, string barNo = null)
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

        private List<string> readBarInformation(ReinforcementBase dwgRebar)
        {
            List<string> info = new List<string>();
            TSM.Model MyModel = new TSM.Model();
            TSM.ModelObject modelRebar = MyModel.SelectModelObject(dwgRebar.ModelIdentifier);
            TSM.Phase rebarPhase = new TSM.Phase();
            modelRebar.GetPhase(out rebarPhase);
            info.Add(rebarPhase.PhaseName);
            string shapeCode = null;
            bool gotProperty = modelRebar.GetReportProperty("REBAR_POS", ref shapeCode);
            info.Add(shapeCode);
            return info;
        }

        private void drawCloud(TSDrg.PointList pointList, ViewBase view, string attributesFile)
        {
                TSDrg.Cloud cloud = new TSDrg.Cloud(view, pointList, new Cloud.CloudAttributes(attributesFile));
                cloud.Attributes.Line.Color = TSDrg.DrawingColors.Red;
                cloud.Insert();
                view.GetDrawing().CommitChanges();
        }

        private void addFCRNote(ViewBase view, TSG.Point insertionPoint ,ReinforcementBase rebarObject)
        {
            string fcrNumber = getPhaseNameFromReinforcement(rebarObject);
            fcrNumber = fcrNumber.Trim();
            if (fcrNumber.LastIndexOf(" ") > 0)
                fcrNumber = fcrNumber.Substring(0, fcrNumber.LastIndexOf(" ", fcrNumber.Length));
            TSDrg.Text fcrNote = new TSDrg.Text(view, insertionPoint, fcrNumber, new TSDrg.Text.TextAttributes(PS.Default.usr_FcrNote));
            fcrNote.Insert();
            view.GetDrawing().CommitChanges();
        }

        private void alignMarks(MarkBase baseMark, List<MarkBase> barMarks)
        {
            TSG.Point basePoint = baseMark.InsertionPoint;
            MarkBase[] sortedBarMarks = barMarks.ToArray();
            Double[] sortedBarMarkXpos = new Double[barMarks.Count];
            bool reversed = false;

            //create array of barmark positions for sorting
            for (int i = 0; i < sortedBarMarks.Length; i++)
            {
                TSDrg.LeaderLinePlacing placing = (LeaderLinePlacing)sortedBarMarks[i].Placing;
                //drawcircle(placing.StartPoint, sortedBarMarks[i].GetView());
                sortedBarMarkXpos[i] = placing.StartPoint.X;
            }

            //sort and check for position of the basemark, if basemark is at the end, then reverse order of marks for sorting
            Array.Sort(sortedBarMarkXpos, sortedBarMarks);
            if (basePoint.X > sortedBarMarkXpos.Last())
            {
                reversed = true;
                Array.Reverse(sortedBarMarks);
                Array.Reverse(sortedBarMarkXpos);
            }

            Double newInsertionYpos = basePoint.Y - PS.Default.usr_BarMarkYOffset;

            //move barmarks
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

                newInsertionYpos -= PS.Default.usr_BarMarkYOffset;
            }
        }

        private void alignMarksHorizontal(List<MarkBase> barMarks)
        {
            MarkBase[] sortedBarMarks = barMarks.ToArray();

            for (int i = 0; i < sortedBarMarks.Length; i++)
            {
                TSDrg.LeaderLinePlacing placing = (LeaderLinePlacing)sortedBarMarks[i].Placing;
                double newInsertionXpos = sortedBarMarks[i].InsertionPoint.X;
                double newInsertionYpos = placing.StartPoint.Y + sortedBarMarks[i].GetObjectAlignedBoundingBox().Height/2;
                TSG.Point currBarMarkPosition = sortedBarMarks[i].InsertionPoint;

                TSG.Point newInsertionPoint = new TSG.Point(newInsertionXpos, newInsertionYpos);
                TSG.Vector moveVector = new TSG.Vector(
                    newInsertionPoint.X - currBarMarkPosition.X,
                    newInsertionPoint.Y - currBarMarkPosition.Y,
                    0.0);
                sortedBarMarks[i].MoveObjectRelative(moveVector);
                sortedBarMarks[i].Modify();
            }
        }


        private TSDrg.PointList calculateRectanlgeFromCorners(TSDrg.PointList cornerList)
        {
            PointList rectangle = new PointList();

            TSG.Point[] corners = cornerList.ToArray();
            rectangle.Add(corners[0]);
            rectangle.Add(new TSG.Point(corners[1].X, corners[0].Y, 0.0));
            rectangle.Add(corners[1]);
            rectangle.Add(new TSG.Point(corners[0].X, corners[1].Y, 0.0));

            return rectangle;
        }

        private string getPhaseNameFromReinforcement(ReinforcementBase drawingObject)
        {
            TSDrg.ReinforcementBase dwgRebar = drawingObject as TSDrg.ReinforcementBase;
            TSM.Model MyModel = new TSM.Model();
            TSM.ModelObject modelRebar = MyModel.SelectModelObject(dwgRebar.ModelIdentifier);
            TSM.Phase rebarPhase = new TSM.Phase();
            modelRebar.GetPhase(out rebarPhase);
            return rebarPhase.PhaseName;
        }

        private DrawingObject getSingleObjectFromUser<T>()
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

        private DrawingObject getSingleObjectFromSelection<T>()
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

        private List<DrawingObject> getObjectsFromSelection(Type filterType)
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

        private Tuple<TSG.Point, TSDrg.View> getPointFromUser()
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

        private Tuple<TSDrg.PointList, TSDrg.View> getRectangleCornersFromUser()
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

        private void adjustBBToInsertionPoint(ref RectangleBoundingBox boundingBox, TSG.Point insertionPoint)
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

        private void drawcircle(TSG.Point centre, ViewBase view)
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

        
    }
}
