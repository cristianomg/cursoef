using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using consoleAPp.Data;
using consoleAPp.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace consoleAPp
{
    class Program
    {
        static void Main(string[] args)
        {
            DivisaoDeConsultas();
        }
        static void DivisaoDeConsultas()
        {
            using var db = new ApplicationContext();
            Setup(db);
            var departamentos = db.Departaments
            .Include(x => x.Employees)
            .Where(x => x.Id < 3)
            .AsSingleQuery()
            //.AsSplitQuery()
            .ToList();
        }
        static void EntendendoConsulta1NxN1()
        {
            using var db = new ApplicationContext();
            Setup(db);
            var departaments = db.Departaments.TagWith("Consulta com tag")
            .Include(x => x.Employees)
            .ToList();
            foreach (var departament in departaments)
            {
                Console.WriteLine($"Descrição: {departament.Description} \t");
                foreach (var f in departament.Employees)
                {
                    Console.WriteLine($"Descrição: {f.Name} \t");
                }
            }
        }
        static void ConsultaComTag()
        {
            using var db = new ApplicationContext();
            Setup(db);
            var departaments = db.Departaments.TagWith("Consulta com tag")
            .ToList();
            foreach (var departament in departaments)
            {
                Console.WriteLine($"Descrição: {departament.Description} \t");
            }
        }
        static void ConsultaInterpolada()
        {
            using var db = new ApplicationContext();
            Setup(db);
            var id = 1;
            var departaments = db.Departaments.FromSqlInterpolated($"SELECT * FROM DEPARTAMENTS WHERE ID > {id}")
            .ToList();
            foreach (var departament in departaments)
            {
                Console.WriteLine($"Descrição: {departament.Description} \t");
            }
        }
        static void ConsultaParametrizada()
        {
            using var db = new ApplicationContext();
            Setup(db);
            var id = new SqlParameter
            {
                Value = 1,
                SqlDbType = SqlDbType.Int,
            };
            var departaments = db.Departaments.FromSqlRaw("SELECT * FROM DEPARTAMENTS WHERE ID > {0}", id)
            .Where(x => !x.Excluido).ToList();
            foreach (var departament in departaments)
            {
                Console.WriteLine($"Descrição: {departament.Description} \t");
            }
        }
        static void ConsultaProjetada()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departaments = db.Departaments.Where(x => x.Id > 0).Select(x => new { x.Description, Funcionario = x.Employees.Select(x => x.Name) }).ToList();
            Console.WriteLine(departaments.Count);
            foreach (var departament in departaments)
            {
                Console.WriteLine($"Descrição: {departament.Description} \t");
                foreach (var f in departament.Funcionario)
                {
                    Console.WriteLine($"Nome: {f}");
                }
            }
        }
        static void FiltroGlobal()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departaments = db.Departaments.Where(x => x.Id > 0).ToList();
            Console.WriteLine(departaments.Count);
            foreach (var departament in departaments)
            {
                Console.WriteLine($"Descrição: {departament.Description} \t Excluido: {departament.Excluido}");
            }
        }
        static void IgnoreFiltroGlobal()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departaments = db.Departaments.IgnoreQueryFilters().Where(x => x.Id > 0).ToList();
            Console.WriteLine(departaments.Count);
            foreach (var departament in departaments)
            {
                Console.WriteLine($"Descrição: {departament.Description} \t Excluido: {departament.Excluido}");
            }
        }

        static void Setup(ApplicationContext db)
        {
            if (db.Database.EnsureCreated())
            {
                db.Departaments.AddRange(new Departament
                {
                    Active = true,
                    Description = "Departamento 01",
                    Employees = new List<Employee>{
                    new Employee{
                        Name = "Cristiano Macedo",
                        CPF = "05897257507",
                        RG = "000000000",
                    }
                },
                    Excluido = true
                },
                                new Departament
                                {
                                    Active = true,
                                    Description = "Departamento 02",
                                    Employees = new List<Employee>{
                    new Employee{
                        Name = "Cristiano Macedo 02",
                        CPF = "05897257507",
                        RG = "000000000",
                    },
                    new Employee{
                        Name = "Cristiano Macedo 03",
                        CPF = "05897257507",
                        RG = "000000000",
                    }
                                    },
                                    Excluido = false
                                });
                db.SaveChanges();
                db.ChangeTracker.Clear();
            }
        }
        static void EnsureCreatedAndDeleted()
        {
            using var db = new ApplicationContext();
            // db.Database.EnsureCreated();
            db.Database.EnsureDeleted();
        }
        static void GapDoEnsuredCreated()
        {
            using var db1 = new ApplicationContext();
            db1.Database.EnsureCreated();

        }

        static void HealthCheckDatabase()
        {
            using var db = new ApplicationContext();
            var canConnect = db.Database.CanConnect();
            if (canConnect)
            {
                var connection = db.Database.GetDbConnection();
                connection.Open();
                Console.WriteLine("Posso me connectar;");
            }
            else
            {
                Console.WriteLine("Não posso me conectar.");
            }
        }
        static int _count;
        static void GerenciarEstadoDaConexao(bool gerenciarEstadoConexao)
        {
            using var db = new ApplicationContext();
            var time = Stopwatch.StartNew();

            var conexao = db.Database.GetDbConnection();
            conexao.StateChange += (_, __) => ++_count;
            if (gerenciarEstadoConexao)
            {
                conexao.Open();
            }
            for (var i = 0; i < 200; i++)
            {
                db.Departaments.AsNoTracking().Any();
            }

            time.Stop();
            var message = $"Tempo : {time.Elapsed.ToString()}, {gerenciarEstadoConexao}, Contador: {_count}";

            Console.WriteLine(message);
        }
        static void ExecuteSql()
        {
            using var db = new ApplicationContext();
            using (var cmd = db.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "SELECT 1";
                cmd.ExecuteNonQuery();
            }
            var descricao = "TESTE";
            db.Database.ExecuteSqlRaw("UPDATE DEPARTAMENT SET DESCRIPTION={0} WHERE ID=1", descricao);

            db.Database.ExecuteSqlInterpolated($"UPDATE DEPARTAMENT SET DESCRIPTION={descricao} WHERE ID=1");
        }
        static void MigracoesPendentes()
        {
            using var db = new ApplicationContext();
            var migracoesPendentes = db.Database.GetPendingMigrations();
            Console.WriteLine($"Total: {migracoesPendentes.Count()}");
        }
        static void AplicarMigracaoEmTempoDeExecucao()
        {
            using var db = new ApplicationContext();
            db.Database.Migrate();
        }
        static void TodasMigracoes()
        {
            using var db = new ApplicationContext();
            var migracoes = db.Database.GetMigrations();

            Console.WriteLine($"Total: {migracoes.Count()}");

            foreach (var migration in migracoes)
            {
                Console.WriteLine($"{migration}");
            }
        }
        static void MigracoesJaAplicadas()
        {
            using var db = new ApplicationContext();
            var migracoes = db.Database.GetAppliedMigrations();

            Console.WriteLine($"Total: {migracoes.Count()}");

            foreach (var migration in migracoes)
            {
                Console.WriteLine($"{migration}");
            }
        }
        static void ScriptGeralDoBancoDeDados()
        {
            using var db = new ApplicationContext();
            var script = db.Database.GenerateCreateScript();
            Console.WriteLine(script);
        }
        static void CarregamentoAdiantado()
        {
            using var db = new ApplicationContext();
            SetupTiposCarregamentos(db);

            var departaments = db.Departaments.Include(p => p.Employees);

            foreach (var departament in departaments)
            {
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine($"Departamento: {departament.Description}");
                if (departament.Employees.Any())
                {
                    foreach (var employee in departament.Employees)
                    {
                        Console.WriteLine($"Funcionario: {employee.Name}");
                    }
                }

            }

        }
        static void CarregamentoExplicito()
        {
            using var db = new ApplicationContext();
            SetupTiposCarregamentos(db);

            var departaments = db.Departaments.ToList();

            foreach (var departament in departaments)
            {
                if (departament.Id == 2)
                {
                    db.Entry(departament).Collection(x => x.Employees).Load();
                }
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine($"Departamento: {departament.Description}");
                if (departament.Employees?.Any() ?? false)
                {
                    foreach (var employee in departament.Employees)
                    {
                        Console.WriteLine($"Funcionario: {employee.Name}");
                    }
                }
                else
                {
                    Console.WriteLine("Nenhum funcionario cadastrado.");
                }

            }

        }
        static void CarregamentoLento()
        {
            using var db = new ApplicationContext();
            SetupTiposCarregamentos(db);

            //db.ChangeTracker.LazyLoadingEnabled = false;

            var departaments = db.Departaments.ToList();

            foreach (var departament in departaments)
            {
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine($"Departamento: {departament.Description}");
                if (departament.Employees?.Any() ?? false)
                {
                    foreach (var employee in departament.Employees)
                    {
                        Console.WriteLine($"Funcionario: {employee.Name}");
                    }
                }
                else
                {
                    Console.WriteLine("Nenhum funcionario cadastrado.");
                }

            }

        }
        static void SetupTiposCarregamentos(ApplicationContext context)
        {
            if (!context.Departaments.Any())
            {
                context.Departaments.AddRange(
                    new Departament
                    {
                        Description = "Departamento 01",
                        Employees = new List<Employee>{
                            new Employee{
                                Name = "Cristiano MAcedo",
                                CPF = "05897257507",
                                RG = "34363912",
                            }
                        }
                    },
                    new Departament
                    {
                        Description = "Departamento 02",
                        Employees = new List<Employee> {
                            new Employee{
                                CPF = "55555555555",
                                Name = "Cristiano @",
                                RG  = "36363914"
                            },
                            new Employee {
                                Name = "Eduardo Pires",
                                CPF = "0000000000",
                                RG = "00000000"
                            }
                        }
                    }
                );
                context.SaveChanges();
                context.ChangeTracker.Clear();
            }
        }
        /*
        comandos ef
        dotnet ef migrations add nomemigration --context nomecontexto -- add migration
        dotnet ef migrations list --context nomeContexto -- listar migrations
        dotnet ef migrations remove --context nomeContexto -- remover migrations 
        dotnet ef database update --context nomeContexto -- aplicar migrations
        dtonet ef database drop --context nomeContexto -- dropar banco

        comandos dotnet
        dotnet add package nome do pacote --version versão 
        */
    }
}
