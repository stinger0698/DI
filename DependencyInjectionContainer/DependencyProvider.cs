using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Collections;
using DependencyInjectionContainer.Exceptions;

namespace DependencyInjectionContainer
{
    public class DependencyProvider
    {
        private DependenciesConfiguration _configuration;
        private readonly ConcurrentStack<Type> _stack;
        private Type _currentGenericType;

        public DependencyProvider(DependenciesConfiguration configuration)
        {
            if (ValidateConfiguration(configuration))
            {
                _configuration = configuration;
                _stack = new ConcurrentStack<Type>();
            }
            else
            {
                throw new ConfigurationValidationException("Configuration is not valid!");
            }
        }

        //valudation for configuration
        private bool ValidateConfiguration(DependenciesConfiguration configuration)
        {
            foreach (Type tDependency in configuration.dependencies.Keys)
            {
                if (!tDependency.IsValueType)
                {
                    foreach (ImplementationInfo dependency in configuration.dependencies[tDependency])
                    {
                        Type tImplementation = dependency.implementationType;

                        if (tImplementation.IsAbstract || tImplementation.IsInterface || !tDependency.IsAssignableFrom(tImplementation))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        
        public T Resolve<T>() where T: class
        {
            Type t = typeof(T);

            return (T)Resolve(t);
        }

        //resolve method ( t - interface)
        private object Resolve(Type t)
        {
            List<ImplementationInfo> implementations;
            //IEnumerable of interfaces
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return CreateGeneric(t);
            }
            //single implemenation
            _configuration.dependencies.TryGetValue(t, out implementations);
            if (implementations != null)
                return GetInstance(implementations.First());
            else
            {
                //open generic
                Type genericDefinition;
                if (t.IsGenericType)
                {
                    _currentGenericType = t;
                    genericDefinition = t.GetGenericTypeDefinition();                    
                    _configuration.dependencies.TryGetValue(genericDefinition, out implementations);
                    if (implementations != null)
                        return GetInstance(implementations.First());
                }
                throw new TypeNotRegisterException("Unknown type " + t.Name);
            }
        }

        //crete list of interfaces
        private object CreateGeneric(Type t)
        {
            object result = null;
            List<ImplementationInfo> implementations;
            Type tResolve = t.GetGenericArguments()[0];
            
            _configuration.dependencies.TryGetValue(tResolve,out implementations);
            if (implementations != null)
            {
                result = Activator.CreateInstance(typeof(List<>).MakeGenericType(tResolve));
                foreach (ImplementationInfo tImplementation in implementations)
                {
                    ((IList)result).Add(GetInstance(tImplementation));
                }
            }
            else
                throw new TypeNotRegisterException("Unknown type "+t.Name);
            return result;           
        }
        
        //here validation for singleton
        private object GetInstance(ImplementationInfo tImplementation)
        {
            if (tImplementation.isSingleton)
            {
                return tImplementation.GetInstance(this);
            }
            else
            {
                return Create(tImplementation.implementationType);
            }
        }
              
        //here t is implementation  
        public object Create(Type t)
        {
            object result;
            if (!_stack.Contains(t))
            {
                _stack.Push(t);

                if (t.IsGenericTypeDefinition)
                {
                    t = t.MakeGenericType(_currentGenericType.GenericTypeArguments);
                }

                ConstructorInfo constructor = GetRightConstructor(t);

                if (constructor != null)
                {
                    result = constructor.Invoke(GetConstructorParametersValues(constructor.GetParameters()));
                }
                else
                {
                    throw new ConstructorNotFoundException("Cannot find right constructor!");
                }
                _stack.TryPop(out t);
            }
            else
            {
                result = null;
                //throw new CycleDependencyException("Cycle dependency ERROR!");
            }

            return result;
        }

        private ConstructorInfo GetRightConstructor(Type t)
        {
            ConstructorInfo result = null;
            ConstructorInfo[] constructors = t.GetConstructors();
            bool isRight;

            foreach (ConstructorInfo constructor in constructors)
            {
                ParameterInfo[] parameters = constructor.GetParameters();

                isRight = true;
                foreach (ParameterInfo parameter in parameters)
                {
                    if (!_configuration.dependencies.ContainsKey(parameter.ParameterType))
                    {
                        isRight = false;
                        break;
                    }
                }

                if (isRight)
                {
                    result = constructor;
                    break;
                }
            }
            return result;
        }

        private object[] GetConstructorParametersValues(ParameterInfo[] parameters)
        {
            object[] result = new object[parameters.Length];

            for (int i=0; i<parameters.Length; i++)
            {
                result[i] = Resolve(parameters[i].ParameterType);
            }
            return result;
        }

    }
}
