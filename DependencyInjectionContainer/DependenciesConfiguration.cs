using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DependencyInjectionContainer
{
    public class DependenciesConfiguration
    {
        public ConcurrentDictionary<Type, List<ImplementationInfo>> dependencies {get; }

        public DependenciesConfiguration()
        {
            dependencies = new ConcurrentDictionary<Type, List<ImplementationInfo>>();
        }

        public void Register<TDependency, TImplementation>(bool isSingleton)
        {
            Register(typeof(TDependency), typeof(TImplementation),isSingleton);
        }

        public void Register(Type tDependency, Type tImplementation,bool isSingleton)
        {
            bool searchResult;

            dependencies.TryAdd(tDependency, new List<ImplementationInfo>());
            searchResult = false;
            foreach (ImplementationInfo implementation in dependencies[tDependency])
            {
                if (implementation.implementationType == tImplementation)
                {
                    searchResult = true;
                    break;
                }
            }
            if (!searchResult)
            {
                dependencies[tDependency].Add(new ImplementationInfo(tImplementation,isSingleton));
            }          
        }
    }
}
