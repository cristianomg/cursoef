using System;
using System.Collections.Generic;

namespace consoleAPp.Domain
{
    public class Departament
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public bool Excluido { get; set; }

        public IEnumerable<Employee> Employees { get; set; }


    }
}
