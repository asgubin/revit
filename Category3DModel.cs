using System.Collections.Generic;
using Autodesk.Revit.DB;
using Npgsql;

namespace СostСalculate
{
    class Category3DModel
    {
        private int id;
        private string name;

        //Категория для выгрузки
        private static BuiltInCategory[] categories = new BuiltInCategory[]
        {
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_StructuralFraming,
            BuiltInCategory.OST_Materials,
            BuiltInCategory.OST_Levels,
            BuiltInCategory.OST_Areas
        };

        private Category3DModel(BuiltInCategory category)
        {
            id = (int)category;
            name = category.ToString();
        }

        public static List<Category3DModel> GetCategories()
        {
            List<Category3DModel> categories3D = new List<Category3DModel>();

            foreach (BuiltInCategory category in categories)
            {
                categories3D.Add(new Category3DModel(category));
            }

            return categories3D;
        }

        public static NpgsqlConnection DropTable(NpgsqlConnection conn)
        {
            using (var command = new NpgsqlCommand("DROP TABLE IF EXISTS category", conn))
            {
                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static NpgsqlConnection CreateTable(NpgsqlConnection conn)
        {
            using (var command
                    = new NpgsqlCommand(
                        "CREATE TABLE category(" +
                            "id INTEGER, " +
                            "name VARCHAR(50)," +
                            "CONSTRAINT id_category_pk PRIMARY KEY (id))"
                        , conn))
            {
                command.ExecuteNonQuery();
            }

            return conn;
        }

        public static NpgsqlConnection InsertTable(NpgsqlConnection conn, Category3DModel category3D)
        {
            using (var command
                        = new NpgsqlCommand(
                            "INSERT INTO category(id, name) " +
                                "VALUES (@n, @q)", conn))
            {
                command.Parameters.AddWithValue("n", category3D.getId);
                command.Parameters.AddWithValue("q", category3D.getName);

                command.ExecuteNonQuery();
            }

            return conn;
        }

        public int getId
        {
           get { return id; }
        }

        public string getName
        {
            get { return name; }
        }
    }
}