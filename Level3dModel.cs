using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Npgsql;

namespace СostСalculate
{
    class Level3dModel
    {
        private int     id;             //Element id
        private int     isType;         //является типом или экземпляром
        private string  name;           //Name
        private double  elevation;      //Elevation
        private int     category_id;    //Category

        //Параметры выгрузки
        private static string[] parameters = new string[]
        {
            "Name",                     //[0]name
            "Elevation"                 //[1]elevation
        };

        //Категория для выгрузки
        private static BuiltInCategory categories = BuiltInCategory.OST_Levels;

        private Level3dModel(Element element)
        {
            id          = element.Id.IntegerValue;
            isType      = (element is ElementType) ? 1 : 0;
            name        = element.LookupParameter(parameters[0]).AsString();
            elevation   = Math.Round((element.LookupParameter(parameters[1]).AsDouble() * 0.3048), 2);
            category_id = element.Category.Id.IntegerValue;
        }

        public static List<Level3dModel> GetLevels(FilteredElementCollector collector)
        {
            List<Level3dModel> levels3D = new List<Level3dModel>();

            foreach (Element element in collector)
            {
                if (element.Category != null)
                {
                    if (element.Category.Id.IntegerValue == (int)categories)
                    {
                        if (element.CanHaveTypeAssigned())
                        {
                            levels3D.Add(new Level3dModel(element));
                        }
                    }
                }
            }

            return levels3D;
        }

        public static NpgsqlConnection DropTable(NpgsqlConnection conn)
        {
            using (var command = new NpgsqlCommand("DROP TABLE IF EXISTS levels", conn))
            {
                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static NpgsqlConnection CreateTable(NpgsqlConnection conn)
        {
            using (var command
                    = new NpgsqlCommand(
                        "CREATE TABLE levels(" +
                            "id INTEGER, " +
                            "name VARCHAR(50), " +
                            "elevation REAL, " +
                            "category_id INTEGER, " +
                            "CONSTRAINT id_levels_pk PRIMARY KEY (id), " +
                            "CONSTRAINT fk_levels_category foreign key (category_id) references category(id)" +
                        ")", conn))
            {
                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static NpgsqlConnection InsertTable(NpgsqlConnection conn, Level3dModel level3D)
        {
            using (var command
                        = new NpgsqlCommand(
                            "INSERT INTO levels(id, name, elevation, category_id) " +
                                "VALUES (@a, @b, @c, @d)", conn))
            {
                command.Parameters.AddWithValue("a", level3D.getId);
                command.Parameters.AddWithValue("b", level3D.getName);
                command.Parameters.AddWithValue("c", level3D.getElevation);
                command.Parameters.AddWithValue("d", level3D.getCategory_id);

                command.ExecuteNonQuery();
            }

            return conn;
        }

        public override string ToString()
        {
            return  "Element: " + id + "; " + 
                    "IsType: " + isType + "; " +
                    "Name: " + name + "; " +
                    "Elevation: " + elevation + "; " +
                    "Category: " + category_id;
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

        public double getElevation
        {
            get { return elevation; }
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
