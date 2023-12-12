﻿using DoListy.ViewModel;
using SQLite;

namespace DoListy.Database
{
   public class AppointmentRepository
    {
        string _dbpath;
        static SQLiteConnection conn;
        public AppointmentRepository(string dbpath)
        {
            _dbpath = dbpath;
        }
        private void Init()
        {
            if (conn != null)
                return;
            conn = new SQLiteConnection(_dbpath);
            conn.CreateTable<Appointment>();
            conn.CreateTable<Goal>();
        
        }
        public List<Appointment> GetAppointments()
        {
            Init();
            return conn.Table<Appointment>().ToList();
        }

        public List <Appointment> GetAppointmentsByDay(DateTime date) 
        {
            Init();
            var res = conn.Query<Appointment>("SELECT * FROM Appointment WHERE DATE(EventStart) = DATE(?) AND DATE(EventEnd) = DATE(?)", date.Date, date.Date);
            return res.ToList();
        }
        public void AddAppointment(Appointment app)
        {
            int result = 0;
            Init();
            result = conn.Insert(app);
        }
        public void DeleteAppointment(Appointment app)
        {
            Init();
            conn.Delete(app);
        }
        public Appointment GetAppointmentByID(int AppointmentID)
        {
            var temp = from u in conn.Table<Appointment>()
                       where u.Id == AppointmentID
                       select u;
            return  temp.FirstOrDefault();
        }

        public void Update(Appointment appointment)
        {
            Init();
            int result = 0;
            result = conn.Update(appointment);
        }
        public List<Goal> GetGoals()
        {
            Init();
            return conn.Table<Goal>().ToList();
        }
        public void AddGoal(Goal temp)
        {
            int result = 0;
            Init();
            result = conn.Insert(temp);
        }
        public void DeleteGoal(Goal temp)
        {
            Init();
            conn.Delete(temp);
        }
        public List<Goal> GetGoalByYear(int YearInput)
        {
            var temp = from u in conn.Table<Goal>()
                       where u.Year == YearInput
                       select u;

            return temp.ToList();
        }

        public void UpdateGoal(Goal temp)
        {
            Init();
            int result = 0;
            result = conn.Update(temp);
        }
    }
}
