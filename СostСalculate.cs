using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace СostСalculate
{
    [Transaction(TransactionMode.Manual)]
    public class СostСalculate : IExternalCommand
    {
        private const string url = "http://localhost:8080/elementGroups";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;

            Stopwatch sw = Stopwatch.StartNew();

            ElementFilter f = new LogicalOrFilter(new ElementIsElementTypeFilter(false),
                                                  new ElementIsElementTypeFilter(true));

            FilteredElementCollector collector = new FilteredElementCollector(doc)
                                                     .WherePasses(f);
            //Собрать все категории в List<>
            List<Category3DModel> categories3D = Category3DModel.GetCategories();
            //Собрать все материалы в List<>
            List<Material3DModel> materials3D = Material3DModel.GetMaterials(collector);
            //Собрать все семейства в List<>
            List<Family3DModel> families3D = Family3DModel.GetFamilies(collector);
            //Собрать все уровни в List<>
            List<Level3dModel> levels3D = Level3dModel.GetLevels(collector);
            //Собрать все элементы модели в List<>
            List<Element3DModel>  elements3D = Element3DModel.GetElements(collector);
            foreach (Element3DModel element3D in elements3D)
            {
                if (element3D.getMaterial_id == -1)
                {
                    Element3DModel.MessageMissingMaterial(element3D);
                    return Result.Succeeded;
                }

                if (element3D.getMark == "")
                {
                    Element3DModel.MessageMissingMark(element3D);
                    return Result.Succeeded;
                }
            }
            //Собрать все площади в List<>
            List<Area3dModel> areas3D = Area3dModel.GetAreas(collector);
            if (!areas3D.Any())
            {
                TaskDialog.Show("Недостаточно данных", "Не задана общая площадь здания");
                return Result.Succeeded;
            }

            foreach (Area3dModel area3D in areas3D)
            {
                if (area3D.getFloor == 0)
                {
                    Area3dModel.MessageMissingFloor(area3D);
                    return Result.Succeeded;
                }
            }

            ConstructionInfo.GetInfo(elements3D, families3D, levels3D, areas3D);
            //Кол-во выгружаемых объектов Revit
            int nElements = categories3D.Count + materials3D.Count +
                                elements3D.Count   + families3D.Count +
                                levels3D.Count     + areas3D.Count;

            //TaskDialog.Show("Construction Info", ConstructionInfo.AsStrig());

            PostgresDriver.Connect( categories3D,   materials3D,
                                    elements3D,     families3D,
                                    levels3D,       areas3D );

            sw.Stop();

            TaskDialog.Show( "Параметры экспорта",
        string.Format(
          "Экспорт {0} категорий и Всего {1} "
          + "объектов Revit за {2:F2} секунд.",
          categories3D.Count, nElements,
          sw.Elapsed.TotalSeconds ) );

            //открыть браузер по умолчанию            
            Process.Start(url);

            return Result.Succeeded;
        }       
    }
}