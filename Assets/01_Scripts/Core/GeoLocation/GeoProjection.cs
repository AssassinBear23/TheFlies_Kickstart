using UnityEngine;

namespace GeoImport
{
    public static class GeoProjection
    {
        const double R = 6378137.0; // WGS84 radius in meters
        static double Deg2Rad(double d) => d * System.Math.PI / 180.0;


        // Equirectangular projection around a fixed origin (small areas)
        public static Vector2 LatLonToMeters(double lat, double lon, double lat0, double lon0)
        {
            double x = R * (Deg2Rad(lon - lon0)) * System.Math.Cos(Deg2Rad(lat0));
            double y = R * (Deg2Rad(lat - lat0));
            return new Vector2((float)x, (float)y);
        }


        public static Vector2[] LatLonArrayToMeters(System.Collections.Generic.IList<Vector2> lonLatPairs, double lat0, double lon0)
        {
            int n = lonLatPairs.Count;
            var arr = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                var p = lonLatPairs[i];
                arr[i] = LatLonToMeters(p.y, p.x, lat0, lon0); // p = (lon, lat)
            }
            return arr;
        }
    }
}