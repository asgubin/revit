using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Npgsql;

namespace СostСalculate
{
    class Element3DModel
    {
        private int     id;             //Element id
        private int     isType;         //Является типом или экземпляром
        private int     material_id;    //Structural Material
        private int     length;         //Length
        private int     category_id;    //Category id
        private string  mark;           //Mark
        private int     family_id;      //Family
        private int     level_id;       //Top Level or Reference Level

        //Параметры выгрузки
        private static string[] parameters = new string[]
        {
            "ADSK_Материал",            //[0]material_id
            "ADSK_Размер_Длина",        //[1]length
            "ADSK_Марка конструкции",   //[2]mark
            "Family",                   //[3]family_id
            "Top Level",                //[4]level
            "Reference Level"           //[5]level
        };

        //Категория для выгрузки
        private static BuiltInCategory[] categories = new BuiltInCategory[]
        {
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_StructuralFraming
        };

        private Element3DModel(Element element)
        {
            id          = element.Id.IntegerValue;

            isType      = 0;

            material_id = element.LookupParameter(parameters[0]).AsElementId().IntegerValue;

            length      = (int)(element.LookupParameter(parameters[1]).AsDouble() * 304.8);

            category_id = element.Category.Id.IntegerValue;

            mark        = (element.LookupParameter(parameters[2]).AsString() is null)
                           ? "" : element.LookupParameter(parameters[2]).AsString();

            family_id   = element.LookupParameter(parameters[3]).AsElementId().IntegerValue;

            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns)
            {
                level_id = element.LookupParameter(parameters[4]).AsElementId().IntegerValue;
            }
            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFraming)
            {
                level_id = element.LookupParameter(parameters[5]).AsElementId().IntegerValue;
            }
        }

        public static List<Element3DModel> GetElements(FilteredElementCollector collector)
        {
            List<Element3DModel> elements3D = new List<Element3DModel>();

            foreach (Element element in collector)
            {
                if (element.Category != null)
                {
                    foreach (BuiltInCategory category in categories)
                    {
                        if (element.Category.Id.IntegerValue == (int)category)
                        {
                            if (!(element is ElementType))
                            {
                                elements3D.Add(new Element3DModel(element));
                            }
                        }
                    }
                    
                }
            }

            return elements3D;
        }

        public static NpgsqlConnection DropTable(NpgsqlConnection conn)
        {
            using (var command = new NpgsqlCommand("DROP TABLE IF EXISTS elements", conn))
            {
                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static NpgsqlConnection CreateTable(NpgsqlConnection conn)
        {
            using (var command
                    = new NpgsqlCommand(
                        "CREATE TABLE elements" +
                            "(id INTEGER, " +
                            "mark VARCHAR(50), " +
                            "length INTEGER, " +
                            "family_id INTEGER, " +
                            "material_id INTEGER, " +
                            "level_id INTEGER, " +
                            "category_id INTEGER, " +
                            "fer_id VARCHAR(50), " +
                            "fsscm_id VARCHAR(50), " +
                            "coast REAL, " +
                            "CONSTRAINT fk_elements_families foreign key (family_id) references families(id)," +
                            "CONSTRAINT fk_elements_materials foreign key (material_id) references materials(id)," +
                            "CONSTRAINT fk_elements_levels foreign key (level_id) references levels(id)," +
                            "CONSTRAINT fk_elements_category foreign key (category_id) references category(id)," +
                            "CONSTRAINT fk_elements_fer foreign key (fer_id) references fer(id)," +
                            "CONSTRAINT fk_elements_fsscm foreign key (fsscm_id) references fsscm(id)" +
                        ")", conn))
            {
                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static NpgsqlConnection InsertTable(NpgsqlConnection conn, Element3DModel element3D)
        {
            using (var command
                        = new NpgsqlCommand(
                            "INSERT INTO elements(id, mark, length, family_id, material_id, level_id, category_id, fer_id, fsscm_id) " +
                                "VALUES (@a, @b, @c, @d, @e, @f, @g, @h, @i)", conn))
            {
                command.Parameters.AddWithValue("a", element3D.getId);
                command.Parameters.AddWithValue("b", element3D.getMark);
                command.Parameters.AddWithValue("c", element3D.getLength);
                command.Parameters.AddWithValue("d", element3D.getFamily_id);
                command.Parameters.AddWithValue("e", element3D.getMaterial_id);
                command.Parameters.AddWithValue("f", element3D.getLevel_id);
                command.Parameters.AddWithValue("g", element3D.getCategory_id);
                command.Parameters.AddWithValue("h", "invalid");
                command.Parameters.AddWithValue("i", "invalid");

                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static void MessageMissingMaterial(Element3DModel element3D)
        {
            TaskDialog.Show("Недостаточно данных",
                            "Для элемента " +
                                "[ID: " + element3D.getId + "; " +
                                "Марка: " + element3D.getMark +
                                "] не указан параметр \"ADSK_Материал\"");
        }

        public static void MessageMissingMark(Element3DModel element3D)
        {
            TaskDialog.Show("Недостаточно данных",
                            "Для элемента " +
                                "[ID: " + element3D.getId + "; " +
                                "Марка: " + element3D.getMark +
                                "] не указан параметр \"ADSK_Марка конструкции\"");
        }

        public override string ToString()
        {
            return "Element: " + id + "; " +
                   "IsType: " + isType + "; " +
                   "Structural Material: " + material_id + "; " +
                   "Length: " + length + "; " +
                   "category_id: " + category_id + "; " +
                   "Mark: " + mark + "; " +
                   "family_id: " + family_id + "; " +
                   "Level: " + level_id;
        }

        public int getId
        {
            get { return id; }
        }

        public int getIsType
        {
            get { return isType; }
        }

        public int getMaterial_id
        {
            get { return material_id; }
        }

        public int getLength
        {
            get { return length; }
        }

        public int getCategory_id
        {
            get { return category_id; }
        }

        public string getMark
        {
            get { return mark; }
        }

        public int getFamily_id
        {
            get { return family_id; }
        }

        public int getLevel_id
        {
            get { return level_id; }
        }

        public string[] getParameters
        {
            get { return parameters; }
        }
    }
}