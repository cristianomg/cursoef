using System;

namespace consoleAPp.Domain
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RG { get; set; }
        public string CPF { get; set; }
        public bool Excluido { get; set; }

        public int DepartamentId { get; set; }
        public Departament Departament { get; set; }

    }
}