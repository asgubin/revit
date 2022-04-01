using System.Collections.Generic;
using Autodesk.Revit.DB;
using Npgsql;

namespace СostСalculate
{
    class Material3DModel
    {
        private int     id;                 //Element id
        private int     isType;             //является типом или экземпляром
        private string  name;               //ADSK_Материал наименование
        private string  designation;        //ADSK_Материал обозначение
        private int     category_id;        //Category

        //Параметры выгрузки
        private static string[] parameters = new string[]
        {
            "ADSK_Материал наименование",   //[0]name
            "ADSK_Материал обозначение"     //[1]designation
        };

        //Категория для выгрузки
        private static BuiltInCategory categories = BuiltInCategory.OST_Materials;

        private Material3DModel(Element element)
        {
            id          = element.Id.IntegerValue;
            isType      = (element is ElementType) ? 1 : 0;
            name        = element.LookupParameter(parameters[0]).AsString();
            designation = (element.LookupParameter(parameters[1]).AsString() is null) 
                           ? "" : element.LookupParameter(parameters[1]).AsString();
            category_id = element.Category.Id.IntegerValue;
        }

         public static List<Material3DModel> GetMaterials(FilteredElementCollector collector)
         {
            List<Material3DModel> materials = new List<Material3DModel>();

            foreach(Element element in collector)
            {
                if (element.Category != null) 
                {
                    if (element.Category.Id.IntegerValue == (int)categories)
                    {
                        if (element.LookupParameter(parameters[0]).AsString() != null)
                        {
                            materials.Add(new Material3DModel(element));
                        }                        
                    }
                }
            }

             return materials;
         }

        public static NpgsqlConnection DropTable(NpgsqlConnection conn)
        {
            using (var command = new NpgsqlCommand("DROP TABLE IF EXISTS materials", conn))
            {
                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static NpgsqlConnection CreateTable(NpgsqlConnection conn)
        {
            using (var command
                    = new NpgsqlCommand(
                        "CREATE TABLE materials(" +
                            "id INTEGER, " +
                            "designation VARCHAR(50), " +
                            "name VARCHAR(50), " +
                            "category_id INTEGER, " +
                            "CONSTRAINT id_materials_pk PRIMARY KEY (id), " +
                            "CONSTRAINT fk_materials_category foreign key (category_id) references category(id)" +
                        ")", conn))
            {
                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static NpgsqlConnection InsertTable(NpgsqlConnection conn, Material3DModel material)
        {
            using (var command
                        = new NpgsqlCommand(
                            "INSERT INTO materials(id, designation, name, category_id) " +
                                "VALUES (@a, @b, @c, @d)", conn))
            {
                command.Parameters.AddWithValue("a", material.getId);
                command.Parameters.AddWithValue("b", material.getDesignation);
                command.Parameters.AddWithValue("c", material.getName);
                command.Parameters.AddWithValue("d", material.getCategory_id);

                command.ExecuteNonQuery();
            }

            return conn;
        }

        public override string ToString()
        {
            return "Element: " + id + "; IsType: " + isType +
                "; Name: " + name + "; Designation: " + designation +
                "; category_id: " + category_id;
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

        public string[] getParameters
        {
            get { return parameters; }
        }
    }
}