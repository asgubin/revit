using System;
using System.Collections.Generic;
using Npgsql;

namespace СostСalculate
{
    class PostgresDriver
    {
        private static string Host      = "127.0.0.1";
        private static string User      = "postgres";
        private static string DBname    = "curs";
        private static string Password  = "puser";
        private static string Port      = "5432";

        public static void Connect(
                    List<Category3DModel>   categories3D,
                    List<Material3DModel>   materials3D,
                    List<Element3DModel>    elements3D,
                    List<Family3DModel>     families3D,
                    List<Level3dModel>      levels3D,
                    List<Area3dModel>       areas3D)
        {
            string connString =
                String.Format(
                    "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                    Host,
                    User,
                    DBname,
                    Port,
                    Password);

            NpgsqlConnection conn = new NpgsqlConnection(connString);

            conn.Open();

            //DROP TABLE IF EXISTS
            Element3DModel.DropTable(conn);
            Family3DModel.DropTable(conn);
            Material3DModel.DropTable(conn);
            Area3dModel.DropTable(conn);
            Level3dModel.DropTable(conn);
            Category3DModel.DropTable(conn);

            //category
            Category3DModel.CreateTable(conn);
            foreach (Category3DModel category3D in categories3D)
            {
                Category3DModel.InsertTable(conn, category3D);
            }

            //levels
            Level3dModel.CreateTable(conn);
            foreach (Level3dModel level3D in levels3D)
            {
                Level3dModel.InsertTable(conn, level3D);
            }

            //areas
            Area3dModel.CreateTable(conn);
            foreach (Area3dModel area3D in areas3D)
            {
                Area3dModel.InsertTable(conn, area3D);
            }

            //materials
            Material3DModel.CreateTable(conn);
            foreach (Material3DModel material in materials3D)
            {
                Material3DModel.InsertTable(conn, material);
            }

            //families
            Family3DModel.CreateTable(conn);
            foreach (Family3DModel family3D in families3D)
            {
                Family3DModel.InsertTable(conn, family3D);
            }

            //elements
            Element3DModel.CreateTable(conn);
            foreach (Element3DModel element3D in elements3D)
            {
                Element3DModel.InsertTable(conn, element3D);
            }

            conn.Close();
        }
    }
}