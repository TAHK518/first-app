using System;
using covidSim.Models;

namespace covidSim.Utils
{
    public static class VecExtensions
    {
        public static double GetDistanceTo(this Vec vec, Vec other)
        {
            var dx = vec.X - other.X;
            var dy = vec.Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        } 
    }
}