#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.Diagnostics;


#endregion

namespace Week4
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // Toggle the drag a rectangle to select elements function in Revit
            IList<Element> PickList = uidoc.Selection.PickElementsByRectangle("Select Elements");

            //pop up with count of elements
            //TaskDialog.Show("Test", "You selected " + PickList.Count.ToString());

            List<CurveElement> LineList = new List<CurveElement>();

            foreach(Element element in PickList)
            {
                if(element is CurveElement)
                {
                    CurveElement curve = (CurveElement)element;

                    if(curve.CurveElementType == CurveElementType.ModelCurve)
                        
                        LineList.Add(curve);
                }
            }

            Transaction t1 = new Transaction(doc);
            t1.Start("Create Wall");
            Level NewLevel = Level.Create(doc, 15);

            WallType currentWT = GetWallTypeByName(doc, "Ext Block - 140");
            MEPSystemType PipeSystemType = GetMEPSystemTypeByName(doc, "Domestic Hot Water");
            PipeType PipeType = GetPipeTypeByName(doc, "Default");
            MEPSystemType DuctSystemType = GetMEPSystemTypeByName(doc, "Supply Air");
            DuctType DuctType = GetDuctTypeByName(doc, "Default");
            WallType StorefrontWall = GetWallTypeByName(doc, "Storefront");
            WallType Generic = GetWallTypeByName(doc, "Generic - 8in");
            

            foreach (CurveElement CurrentCurve in LineList)
            {
                GraphicsStyle CurrentGS = CurrentCurve.LineStyle as GraphicsStyle;
                //Debug.Print(CurrentGS.Name); 
                Curve curve = CurrentCurve.GeometryCurve;
                if (CurrentGS.Name == "<Medium Lines>")
                {
                    
                    continue;
                }

                XYZ StartPt = curve.GetEndPoint(0);
                XYZ EndPt = curve.GetEndPoint(1);

                //Wall NewWall = Wall.Create(doc, curve, NewLevel.Id, false);
                //Wall NewWall = Wall.Create(doc, curve, currentWT.Id, NewLevel.Id, 20, 0, false, false);
                //Pipe NewPipe = Pipe.Create(doc, PipeSystemType.Id, PipeType.Id, NewLevel.Id, StartPt, EndPt); 
                //Duct NewDuct = Duct.Create(doc, DuctSystemType.Id, DuctType.Id, NewLevel.Id, StartPt, EndPt);

                switch (CurrentGS.Name)
                {
                    case "A-GLAZ":
                        //TaskDialog.Show("test", "The line is A-GLAZ");
                        Wall NewWall1 = Wall.Create(doc, curve, StorefrontWall.Id, NewLevel.Id, 20, 0, false, false);
                        break;

                    case "A-WALL":
                        //TaskDialog.Show("test", "The line is A-WALL");
                        Wall NewWall2 = Wall.Create(doc, curve, Generic.Id, NewLevel.Id, 20, 0, false, false);
                        break;

                    case "M-DUCT":
                        //TaskDialog.Show("test", "The line is M-DUCT");
                        Duct NewDuct = Duct.Create(doc, DuctSystemType.Id, DuctType.Id, NewLevel.Id, StartPt, EndPt);
                        break;

                    case "P-PIPE":
                        //TaskDialog.Show("test", "The line is P-PIPE");
                        Pipe NewPipe = Pipe.Create(doc, PipeSystemType.Id, PipeType.Id, NewLevel.Id, StartPt, EndPt);
                        break;

                    default:
                        //TaskDialog.Show("test", "Dunno what this line is");
                        break;
                }

            }
     
            //pop up with count of elements
            TaskDialog.Show("Test", "You selected " + LineList.Count.ToString());

            t1.Commit();
            t1.Dispose();

            return Result.Succeeded;
        }

        private WallType GetWallTypeByName(Document doc, string WallType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            //collector.OfClass(typeof(WallType));
            collector.OfCategory(BuiltInCategory.OST_Walls).WhereElementIsElementType();

            foreach(WallType CurrentWT in collector)
            {
                if(CurrentWT.Name == WallType)
                return CurrentWT;
            }
            return null;
        }

        private MEPSystemType GetMEPSystemTypeByName(Document doc, string PipeSystemType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(MEPSystemType));
            
            foreach (MEPSystemType CurrentST in collector)
            {
                if (CurrentST.Name == PipeSystemType)
                    return CurrentST;
            }
            return null;
        }


        private PipeType GetPipeTypeByName(Document doc, string PipeType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));

            foreach (PipeType CurrentPT in collector)
            {
                if (CurrentPT.Name == PipeType)
                    return CurrentPT;
            }
            return null;
        }

        private DuctType GetDuctTypeByName(Document doc, string DuctType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DuctType));

            foreach (DuctType CurrentDT in collector)
            {
                if (CurrentDT.Name == DuctType)
                    return CurrentDT;
            }
            return null;
        }
    }
}
