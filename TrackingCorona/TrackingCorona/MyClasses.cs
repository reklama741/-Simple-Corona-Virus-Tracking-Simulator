using System;
using System.Collections.Generic;

namespace TrackingCorona
{
    public class User
    {
        public List<Day> Days_records = new List<Day>();
    }
    public class Day
    {
        public DateTime day { get; set; }
        public List<Signals> signals = new List<Signals>(); 
        public Day(DateTime day) => this.day = day;
        public Day() { }
    }
    public class Signals
    {
        public double Time { get; set; }
        public Coordinates coordinate { get; set; }
        public Signals(double Time,Coordinates signals_cord)
        {
            this.Time = Time;
            coordinate = signals_cord;
        }
    }
    public class Coordinates
    {
        public static List<Coordinates> list_of_all_coordinates = new List<Coordinates>();
        public double x { get; set; }
        public double y { get; set; }
        public Coordinates(double x, double y) { this.x = x; this.y = y; }
        public Coordinates(List<Signals> signals) => NextRandomPoint(signals);
        public void NextRandomPoint(List<Signals> signals)
        { 
            double x;
            double y;
            double distance;
            if (signals.Count != 0)
            {
                do
                {
                    Coordinates coordinates = list_of_all_coordinates[new Random().Next(0, list_of_all_coordinates.Count)];
                    x = coordinates.x;
                    y = coordinates.y;
                    distance = Distance(x, y, signals[signals.Count - 1].coordinate.x, signals[signals.Count - 1].coordinate.y);
                } while (distance < 24 || distance > 48);
                this.x = x;
                this.y = y;
            }
            else
            {
                this.x = new Random().Next(0, (int)list_of_all_coordinates[list_of_all_coordinates.Count - 1].x);
                this.y = new Random().Next(0, (int)list_of_all_coordinates[list_of_all_coordinates.Count - 1].x);
            }
        }
        private double Distance(double x, double y, double _x, double _y) => Math.Sqrt(Math.Pow(_x - x, 2) + Math.Pow(_y - y, 2));
    }
    public class TimeRange
    {
        public int Start_time_in_sec { get; set; }
        public int End_time_in_sec { get; set; }
        public TimeRange(int time1,int time2) { Start_time_in_sec = time1; End_time_in_sec = time2;}
    }
    public class Area
    {
        public Coordinates Bottom_Left_Corner { get; set; }
        public Coordinates Top_Right_Corner { get; set; }
        public Area(Coordinates pointA, Coordinates pointB) { Bottom_Left_Corner = pointA;Top_Right_Corner = pointB; }
    }
}
