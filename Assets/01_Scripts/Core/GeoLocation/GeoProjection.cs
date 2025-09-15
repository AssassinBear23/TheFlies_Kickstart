using UnityEngine;

namespace GeoImport
{
    /// <summary>
    /// Provides methods for projecting geographic coordinates (latitude, longitude) to planar coordinates (meters)
    /// using the equirectangular projection, suitable for small areas around a fixed origin.
    /// </summary>
    public static class GeoProjection
    {
        /// <summary>
        /// WGS84 ellipsoid radius in meters.
        /// </summary>
        const double R = 6378137.0;

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">Angle in degrees.</param>
        /// <returns>Angle in radians.</returns>
        static double DegreesToRadians(double degrees) => degrees * System.Math.PI / 180.0;

        /// <summary>
        /// Projects a latitude/longitude coordinate to planar meters using the equirectangular projection
        /// centered at the specified origin.
        /// </summary>
        /// <param name="latitude">Latitude of the point to project (degrees).</param>
        /// <param name="longitude">Longitude of the point to project (degrees).</param>
        /// <param name="originLatitude">Latitude of the projection origin (degrees).</param>
        /// <param name="originLongitude">Longitude of the projection origin (degrees).</param>
        /// <returns>
        /// A <see cref="Vector2"/> representing the projected coordinates in meters (x: east-west, y: north-south).
        /// </returns>
        public static Vector2 LatLonToMeters(double latitude, double longitude, double originLatitude, double originLongitude)
        {
            double offsetEastMeters = R * (DegreesToRadians(longitude - originLongitude)) * System.Math.Cos(DegreesToRadians(originLatitude));
            double offsetNorthMeters = R * (DegreesToRadians(latitude - originLatitude));
            return new Vector2((float)offsetEastMeters, (float)offsetNorthMeters);
        }

        /// <summary>
        /// Projects an array of longitude/latitude pairs to planar meters using the equirectangular projection
        /// centered at the specified origin.
        /// </summary>
        /// <param name="longitudeLatitudePairs">
        /// A list of <see cref="Vector2"/> where each element represents a coordinate pair (x: longitude, y: latitude).
        /// </param>
        /// <param name="originLatitude">Latitude of the projection origin (degrees).</param>
        /// <param name="originLongitude">Longitude of the projection origin (degrees).</param>
        /// <returns>
        /// An array of <see cref="Vector2"/> containing the projected coordinates in meters.
        /// </returns>
        public static Vector2[] LatLonArrayToMeters(System.Collections.Generic.IList<Vector2> longitudeLatitudePairs, double originLatitude, double originLongitude)
        {
            int count = longitudeLatitudePairs.Count;
            var projected = new Vector2[count];

            for (int i = 0; i < count; i++)
            {
                var pair = longitudeLatitudePairs[i]; // pair = (lon, lat)
                projected[i] = LatLonToMeters(pair.y, pair.x, originLatitude, originLongitude);
            }

            return projected;
        }
    }
}