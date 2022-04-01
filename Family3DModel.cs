using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Npgsql;

namespace СostСalculate
{
    class Family3DModel
    {
        private int     id;             //Element id
        private int     isType;         //Является типом или экземпляром
        private string  name;           //Type name
        private string  designation;    //ADSK_Обозначение
        private int     category_id;    //Category id
        private double  weight;         //Nominal weight
        private double  area;           //Section area

        //Параметры выгрузки
        private static string[] parameters = new string[]
        {
            "ADSK_Наименование",        //[0]name
            "ADSK_Обозначение",         //[1]designation
            "Nominal Weight",           //[2]weight
            "Section Area"              //[3]area
        };        

        //Категория для выгрузки
        private static BuiltInCategory[] categories = new BuiltInCategory[]
        {
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_StructuralFraming
        };

        private Family3DModel(Element element)
        {
            id          = element.Id.IntegerValue;

            isType      = 1;

            name        = (element.LookupParameter(parameters[0]).AsString() is null)
                           ? "" : element.LookupParameter(parameters[0]).AsString();

            designation = (element.LookupParameter(parameters[1]).AsString() is null)
                           ? "" : element.LookupParameter(parameters[1]).AsString();

            category_id = element.Category.Id.IntegerValue;

            weight      = (element.LookupParameter(parameters[2]).AsDouble() is 0)
                           ? 0.0 : Math.Round((element.LookupParameter(parameters[2]).AsDouble() / 9.8), 2);

            area        = (element.LookupParameter(parameters[3]).AsDouble() is 0)
                           ? 0.0 : Math.Round((element.LookupParameter(parameters[3]).AsDouble() * 30.48 * 30.48), 2);
        }

        public static List<Family3DModel> GetFamilies(FilteredElementCollector collector)
        {
            List<Family3DModel> families3D = new List<Family3DModel>();

            foreach (Element element in collector)
            {
                if (element.Category != null)
                {
                    foreach (BuiltInCategory category in categories)
                    {
                        if (element.Category.Id.IntegerValue == (int)category)
                        {
                            if (element is ElementType)
                            {
                                if (element.LookupParameter(parameters[2]) != null)
                                {
                                    families3D.Add(new Family3DModel(element));
                                }                                
                            }
                        }
                    }

                }
            }

            return families3D;
        }

        public static NpgsqlConnection DropTable(NpgsqlConnection conn)
        {
            using (var command = new NpgsqlCommand("DROP TABLE IF EXISTS families", conn))
            {
                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static NpgsqlConnection CreateTable(NpgsqlConnection conn)
        {
            using (var command
                    = new NpgsqlCommand(
                        "CREATE TABLE families(" +
                            "id INTEGER, " +
                            "designation VARCHAR(50), " +
                            "name VARCHAR(50), " +
                            "weight REAL, " +
                            "section_area REAL, " +
                            "category_id INTEGER, " +
                            "CONSTRAINT id_families_pk PRIMARY KEY (id), " +
                            "CONSTRAINT fk_families_category foreign key (category_id) references category(id)" +
                        ")", conn))
            {
                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static NpgsqlConnection InsertTable(NpgsqlConnection conn, Family3DModel family3D)
        {
            using (var command
                        = new NpgsqlCommand(
                            "INSERT INTO families(id, designation, name, weight, section_area, category_id) " +
                                "VALUES (@a, @b, @c, @d, @e, @f)", conn))
            {
                command.Parameters.AddWithValue("a", family3D.getId);
                command.Parameters.AddWithValue("b", family3D.getDesignation);
                command.Parameters.AddWithValue("c", family3D.getName);
                command.Parameters.AddWithValue("d", family3D.getWeight);
                command.Parameters.AddWithValue("e", family3D.getArea);
                command.Parameters.AddWithValue("f", family3D.getCategory_id);

                command.ExecuteNonQuery();
            }

            return conn;
        }

        public override string ToString()
        {
            return "Element: " + id + "; IsType: " + isType +
                "; Type Name: " + name + "; ADSK_Обозначение: " + designation +
                "; category_id: " + category_id + "; Nominal Weight: " + weight +
                "; Section Area: " + area;
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

        public string getDesignation
        {
            get { return designation; }
        }

        public int getCategory_id
        {
            get { return category_id; }
        }

        public double getWeight
        {
            get { return weight; }
        }

        public double getArea
        {
            get { return area; }
        }

        public string[] getParameters
        {
            get { return parameters; }
        }
    }
}