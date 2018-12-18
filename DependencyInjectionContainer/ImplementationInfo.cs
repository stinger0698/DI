using System;

namespace DependencyInjectionContainer
{
    public class ImplementationInfo
    {
        private object _instance;
        private static object _syncRoot;
        public Type implementationType { get; }
        public bool isSingleton { get; }        

        public ImplementationInfo(Type dependencyType,bool isSingleton)
        {
            this.implementationType = dependencyType;
            this.isSingleton = isSingleton;
            _syncRoot = new object();
            _instance = null;
        }

        public object GetInstance(DependencyProvider provider)
        {
            if (_instance == null)
            {
                lock (_syncRoot)
                {
                    if (_instance == null)
                        _instance = provider.Create(implementationType);
                }
            }
            return _instance;
        }
    }
}
