using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Npgsql;

namespace СostСalculate
{
    class Area3dModel
    {
        private int     id;             //Element id
        private int     isType;         //является типом или экземпляром
        private string  name;           //Name
        private double  area;           //Area
        private int     perimeter;      //Perimeter
        private int     level_id;       //Level
        private int     floor;          //ADSK_Этаж
        private int     category_id;    //Category

        //Параметры выгрузки
        private static string[] parameters = new string[]
        {
            "Name",                     //[0]name
            "Area",                     //[1]area
            "Perimeter",                //[2]perimeter
            "Level",                    //[3]level_id
            "ADSK_Этаж"                 //[4]floor
        };

        //Категория для выгрузки
        private static BuiltInCategory categories = BuiltInCategory.OST_Areas;

        private Area3dModel(Element element)
        {
            id          = element.Id.IntegerValue;
            isType      = (element is ElementType) ? 1 : 0;
            name        = element.LookupParameter(parameters[0]).AsString();
            area        = Math.Round((element.LookupParameter(parameters[1]).AsDouble() * 0.3048 * 0.3048), 2);
            perimeter   = (int)(element.LookupParameter(parameters[2]).AsDouble() * 304.8);
            level_id    = element.LookupParameter(parameters[3]).AsElementId().IntegerValue;            
            floor       = (element.LookupParameter(parameters[4]).AsString() is null)
                           ? 0 : Convert.ToInt32(element.LookupParameter(parameters[4]).AsString());
            category_id = element.Category.Id.IntegerValue;
        }

        public static List<Area3dModel> GetAreas(FilteredElementCollector collector)
        {
            List<Area3dModel> areas3D = new List<Area3dModel>();

            foreach (Element element in collector)
            {
                if (element.Category != null)
                {
                    if (element.Category.Id.IntegerValue == (int)categories)
                    {
                        areas3D.Add(new Area3dModel(element));
                    }
                }
            }

            return areas3D;
        }

        public static NpgsqlConnection DropTable(NpgsqlConnection conn)
        {
            using (var command = new NpgsqlCommand("DROP TABLE IF EXISTS areas", conn))
            {
                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static NpgsqlConnection CreateTable(NpgsqlConnection conn)
        {
            using (var command
                    = new NpgsqlCommand(
                        "CREATE TABLE areas(" +
                            "id INTEGER, " +
                            "name VARCHAR(50), " +
                            "area REAL, " +
                            "perimeter INTEGER, " +
                            "floor INTEGER, " +
                            "level_id INTEGER, " +
                            "category_id INTEGER, " +
                            "CONSTRAINT fk_areas_levels foreign key (level_id) references levels(id), " +
                            "CONSTRAINT fk_areas_category foreign key (category_id) references category(id)" +
                        ")", conn))
            {
                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static NpgsqlConnection InsertTable(NpgsqlConnection conn, Area3dModel area3D)
        {
            using (var command
                        = new NpgsqlCommand(
                            "INSERT INTO areas(id, name, area, perimeter, floor, level_id, category_id) " +
                                "VALUES (@a, @b, @c, @d, @e, @f, @g)", conn))
            {
                command.Parameters.AddWithValue("a", area3D.getId);
                command.Parameters.AddWithValue("b", area3D.getName);
                command.Parameters.AddWithValue("c", area3D.getArea);
                command.Parameters.AddWithValue("d", area3D.getPerimeter);
                command.Parameters.AddWithValue("e", area3D.getFloor);
                command.Parameters.AddWithValue("f", area3D.getLevel_id);
                command.Parameters.AddWithValue("g", area3D.getCategory_id);

                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static void MessageMissingFloor(Area3dModel area3D)
        {
            TaskDialog.Show("Недостаточно данных",
                "Для элемента " +
                    "[ID: " + area3D.getId + "; " +
                    "] не указан параметр \"ADSK_Этаж\"");
        }

        public override string ToString()
        {
            return "Element: "      +   id          + "; " +
                    "IsType: "      +   isType      + "; " +
                    "Name: "        +   name        + "; " +
                    "Area: "        +   area        + "; " +
                    "Perimeter: "   +   perimeter   + "; " +
                    "Level: "       +   level_id    + "; " +
                    "Floor: "       +   floor       + "; " +
                    "Category: "    +   category_id;
        }

        public int getId
        {
            get { return id; }
        }

        public int getIsType
        {
            get { return isType; }
        }

        public string getName
        {
            get { return name; }
        }

        public double getArea
        {
            get { return area; }
        }

        public double getPerimeter
        {
            get { return perimeter; }
        }

        public int getLevel_id
        {
            get { return level_id; }
        }

        public int getFloor
        {
            get { return floor; }
        }

        public int getCategory_id
        {
            get { return category_id; }
        }

        public string[] getParameters
        {
            get { return parameters; }
        }
    }
}