using System;
using System.Collections.Generic;

namespace СostСalculate
{
    class ConstructionInfo
    {
        private static double buildingHeight;       //Высота здания
        private static int    floorCount;           //Кол-во этажей
        private static double buildingSpan;         //Пролет здания
        private static double area;                 //Суммарная площадь этажей
        private static double steelWeight;          //Суммарный вес стали
        private static double steelConsumption;     //Расход стали
        //private static double craneCapacity;        //Грузоподъемность крана

        public static void GetInfo(
                        List<Element3DModel>    elements3D,
                        List<Family3DModel>     families3D,
                        List<Level3dModel>      levels3D,
                        List<Area3dModel>       areas3D)
        {
            buildingHeight   = 0.0;
            floorCount       = 0;
            buildingSpan     = 0.0;
            area             = 0.0;
            steelWeight      = 0.0;
            steelConsumption = 0.0;
            //craneCapacity    = 0.0;

            foreach (Level3dModel element in levels3D)
            {
                if (element.getElevation > buildingHeight)
                {
                    buildingHeight = element.getElevation;
                }
            }

            
            foreach (Area3dModel element in areas3D)
            {
                if (element.getFloor > floorCount)
                {
                    floorCount = element.getFloor;
                }

                area += element.getArea;
            }

            
            foreach (Element3DModel element in elements3D)
            {
                double length = (double)element.getLength / 1000;

                if (element.getMark.Equals("Б1"))
                {
                    if (length > buildingSpan)
                    {
                        buildingSpan = length;
                    }
                }

                int family_id = element.getFamily_id;
                foreach (Family3DModel family in families3D)
                {
                    if (family_id == family.getId)
                    {
                        steelWeight += length * family.getWeight;
                    }
                }
            }

            steelConsumption = (area is 0) ? 0.0 : (steelWeight / area);

            buildingSpan = Math.Round(buildingSpan, 2);
            steelWeight = Math.Round(steelWeight, 2);
            steelConsumption = Math.Round(steelConsumption, 2);
        }

        public static string AsStrig()
        {
            return "Высота здания: " + buildingHeight + "; " +
                   "Кол-во этажей: " + floorCount + "; " +
                   "Пролет здания: " + buildingSpan + "; " +
                   "Суммарная площадь этажей: " + area + "; " +
                   "Суммарный вес стали: " + steelWeight + "; " +
                   "Расход стали: " + steelConsumption;
        }
    }    
}