using System;
using System.Collections.Generic;
using DependencyInjectionContainer.Exceptions;

namespace DependencyInjectionContainer
{
    class Program
    {
        static void Main(string[] args)
        {
            DependenciesConfiguration c = new DependenciesConfiguration();
            c.Register<MySQLRepository, MySQLRepository>(false);
            c.Register(typeof(ServiceImpl<>), typeof(ServiceImpl<>),false);

            //c.Register<ClassForExample, ClassForExample>(true);
            //c.Register<ClassForExample66<ClassForExample>,ClassForExample66<ClassForExample>>(true);
            //c.Register<ClassForExample2, ClassForExample2>(true);
            //c.Register<ClassForExample3, ClassForExample3>(false);
            try
            {
                DependencyProvider p = new DependencyProvider(c);
                ServiceImpl<MySQLRepository> example = p.Resolve<ServiceImpl<MySQLRepository>>();
                example.GetNum();
            }
            catch (ConfigurationValidationException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ConstructorNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (CycleDependencyException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (TypeNotRegisterException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
