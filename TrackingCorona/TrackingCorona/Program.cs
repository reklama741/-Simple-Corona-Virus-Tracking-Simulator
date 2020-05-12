using System;
using System.Collections.Generic;

namespace TrackingCorona
{
    //Program's purpose is to mimic in a way corona-virus tracking programs
    public class Program
    {
        static List<User> Infected_People = new List<User>();
        static User Mobile_User = new User();
        static void Main(string[] args)
        {
            Create_All_Coordinates(2000);
            Create_Mobile_User();
            Show_Mobile_User();
            Create_Infected_People();
            Console.WriteLine(Find_Crowded_Places(new DateTime(2020,5,12),new TimeRange(0,86400),new Area(new Coordinates(0,0),new Coordinates(2000,2000)),Infected_People,1500));
            Console.WriteLine(Possible_Covid_19_Infection(new DateTime(2020, 5, 12),Infected_People, 1500, 900 , 14400));
        }
        //Creates a table with List of with all the coordinates the exist betwenn an array_size * array_size
        public static void Create_All_Coordinates(int array_size)
        {
            for (double i = 0; i <= array_size; i++)
                for (double y = 0; y <= array_size; y++)
                    Coordinates.list_of_all_coordinates.Add(new Coordinates(i, y));
        }
        //Basically Populates the infected people List with people that are
        public static void Create_Infected_People()
        {
            Console.Write("How many infected people do you want to create ? : ");
            double Infected_People_to_create = Convert.ToDouble(Console.ReadLine());
            Console.Write("How many days do you want to generate per infected people ? : ");
            double days_to_create = Convert.ToDouble(Console.ReadLine());
            for (double i = 0; i < Infected_People_to_create; i++)
            {
                User User = new User();
                for (double z = 0; z < days_to_create; z++)
                {
                    Day day = new Day();
                    day.day = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(i);
                    for (double sec = 0; sec <= 86400; sec += 30)
                    {
                        Signals signal = new Signals(sec, new Coordinates(day.signals));
                        day.signals.Add(signal);
                    }
                    User.Days_records.Add(day);
                }
                Infected_People.Add(User);
            }
        }
        //Create a mobile user
        public static void Create_Mobile_User()
        {
            double counter = 0;
            Console.Write("How many days do you want your user to have ? : ");
            double days = Convert.ToDouble(Console.ReadLine());
            for(double i = 0; i < days; i++)
            {
                //A day has 86400 sec and every gps signal is taken every 30 seconds
                Day day = new Day();
                day.day = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day).AddDays(i);
                for(double sec = 0; sec <= 86400; sec += 30)
                {
                    if (sec != 86400 && sec != 0)
                    {
                        //In order to mimic gps signal loss this random gives a 30% chance to loose signla for random duration 0 - 300 sec
                        if (new Random().Next(0, 100) <= 30 && counter == 0)
                        {
                            counter = new Random().Next(0, 10);
                            continue;
                        }
                        else if (counter > 0)
                        {
                            counter--;
                            continue;
                        }
                    }
                    Signals signal = new Signals(sec, new Coordinates(day.signals));
                    day.signals.Add(signal);
                }
                Mobile_User.Days_records.Add(day);
                //Repairs the missing signals 
                Repair_Trajectory();
            }
        }
        //Checks if you are within range of a place that an infected person has been, for an extended time duration
        //and checks for as many hours in seconds you choose;
        public static bool Possible_Covid_19_Infection(DateTime day,List<User> infected_people,int Distance,int time_in_sec,int time_after_infected)
        {
            //For every infected checks every signal 
            foreach (User infected_person in infected_people)
                foreach (Day infected_person_day in infected_person.Days_records)
                    if (infected_person_day.day == day)
                        foreach (Signals infected_person_signals in infected_person_day.signals)
                        {
                            bool should_continue = false;
                            int remaining_time = time_after_infected;
                            int time_spent = 0;
                            //checks from the moment infected signal was created till the time_after_infected
                            //if a signal's creation time is 18:00 and coronavirus can exist after 4 hours it checks from 18:00-22:00
                            //also if signla's creattion time wsa 22:00 it also checks the first 2 hours of the next day 
                            foreach (Day user_day in Mobile_User.Days_records)
                            {
                                if (user_day.day == day || should_continue)
                                {
                                    if (user_day.day == day)
                                    {
                                        for (int signal_counter = 0; signal_counter <= user_day.signals.Count; signal_counter++)
                                        {
                                            if (user_day.signals[signal_counter].Time == infected_person_signals.Time)
                                            {
                                                for (Signals user_signal = user_day.signals[signal_counter]; remaining_time > 0;)
                                                {
                                                    if (time_spent == time_in_sec)
                                                        return true;
                                                    if (Points_Distance(user_signal.coordinate, infected_person_signals.coordinate) < Distance)
                                                    {
                                                        time_spent += 30;
                                                        remaining_time -= 30;
                                                    }
                                                    else
                                                    {
                                                        time_spent = 0;
                                                        remaining_time -= 30;
                                                    }
                                                    if (user_signal.Time == 600)
                                                    {
                                                        should_continue = true;
                                                    }
                                                }
                                                if (should_continue)
                                                    break;
                                                else
                                                    return false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int counter = 0;
                                        for (Signals signals = user_day.signals[counter]; remaining_time > 0;)
                                        {
                                            if (Points_Distance(signals.coordinate, infected_person_signals.coordinate) < Distance)
                                            {
                                                time_spent += 30;
                                                remaining_time -= 30;
                                            }
                                            else
                                            {
                                                time_spent = 0;
                                                remaining_time -= 30;
                                            }
                                            counter++;
                                        }
                                    }
                                }
                            }
                        }
            return false;
        }
        //Repairs all the missing points
        public static void Repair_Trajectory()
        {
            Day we_want = Mobile_User.Days_records[Mobile_User.Days_records.Count - 1];
            for(int repeats = 0; repeats < we_want.signals.Count - 1; )
            {
                if(we_want.signals[repeats + 1].Time - we_want.signals[repeats].Time > 30)
                {
                    int signals_missing = (int)(we_want.signals[repeats + 1].Time - we_want.signals[repeats].Time) / 30;
                    Coordinates pointA = we_want.signals[repeats].coordinate;
                    Coordinates pointB = we_want.signals[repeats + 1].coordinate;
                    for(int i = 1; i  < signals_missing; i++)
                    {
                        Coordinates loop_coordinates;
                        Fill_In_Between_Points(pointA, pointB, i, signals_missing,out loop_coordinates);
                        Signals missing_signal = new Signals(we_want.signals[repeats].Time + 30 * i, loop_coordinates);
                        we_want.signals.Insert(repeats + i, missing_signal);
                    }
                    repeats += signals_missing + 1;
                    continue;
                }
                repeats++;
            }
        }
        //Helps Repair_Trajectory
        public static void Fill_In_Between_Points(Coordinates PointA , Coordinates PointB ,double position,double of_points,out Coordinates to_return)
        {
            double p = (double)position;
            double pp = (double)of_points + 1.0;
            to_return = new Coordinates(Math.Round((p / pp) * (PointB.x - PointA.x) + PointA.x,0),Math.Round((p / pp) * (PointB.y - PointA.y) + PointA.y));
        }
        //Simple Console.WriteLine to show the infected people
        public static void Show_Infected()
        {
            foreach (User infected in Infected_People)
                foreach (Day day in infected.Days_records)
                    foreach (Signals signal in day.signals)
                        Console.WriteLine($"X : {signal.coordinate.x} Y : {signal.coordinate.y} Day : {day.day:d} Time : {signal.Time}");
        }
        //Simple Console.WriteLine to show mobile user position
        public static void Show_Mobile_User()
        {
                foreach (Day day in Mobile_User.Days_records)
                    foreach (Signals signal in day.signals)
                        Console.WriteLine($"X : {signal.coordinate.x} Y : {signal.coordinate.y} Day : {day.day:d} Time : {signal.Time}");
        }
        //Calculate distane between two points
        public static double Points_Distance(Coordinates PointA, Coordinates PointB) => Math.Sqrt(Math.Pow(PointA.x - PointB.x, 2) + Math.Pow(PointA.y - PointB.y, 2));
        //Removes gps signals that are too close to one another
        public static void Summarize_Mobile_Users_Trajectory_Last_Day(int Minimum_Distance_Between_Points)
        {
            Day sum_day = Mobile_User.Days_records[Mobile_User.Days_records.Count - 1];
            for(int i = 0; i < sum_day.signals.Count - 1; i++)
                for(int y = i + 1; y < sum_day.signals.Count; y++)
                    if(Points_Distance(sum_day.signals[i].coordinate, sum_day.signals[y].coordinate) < Minimum_Distance_Between_Points)
                    {
                        sum_day.signals.RemoveAt(y);
                        y = i;
                    }
        }
        //Find how many people spent minimum stay time in a specific day ,Area
        public static int Find_Crowded_Places(DateTime Day,TimeRange time_range,Area are_of_interest,List<User> users,int Minimum_stay_duration)
        {
            int users_to_return = 0;
            foreach(User usr in users)
            {
                foreach(Day usr_day in usr.Days_records)
                {
                    if (usr_day.day == Day) {
                        int time_spent = -1;
                        foreach (Signals user_signal in usr_day.signals)
                        {
                            if (time_spent >= Minimum_stay_duration)
                            {
                                users_to_return++;
                                break;
                            }
                            if(CheckArea(are_of_interest,user_signal.coordinate)
                                && user_signal.Time >= time_range.Start_time_in_sec
                                && user_signal.Time <= time_range.End_time_in_sec)
                            {
                                time_spent += 30;
                            }
                            else
                            {
                                time_spent = 0;
                            }
                        }
                    }
                }
            }
            return users_to_return;
        }
        //Ultra simple coordinates checking to check if its between this rectangle in it's simplest form i know!
        static bool CheckArea(Area to_check,Coordinates To_find)
        {
            return (To_find.x > to_check.Bottom_Left_Corner.x && To_find.x < to_check.Top_Right_Corner.x) &&
                (To_find.y > to_check.Bottom_Left_Corner.y && To_find.y < to_check.Top_Right_Corner.y);
        }

    }
}