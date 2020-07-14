using System;

namespace WorkingWithWorker.Models
{
    public class Temperatura
    {
        public DateTime Horario { get; set; }
        public string Cidade { get; set; }
        public int? GrausCelsius { get; set; }
        public Object Exception { get; set; }
    }
}