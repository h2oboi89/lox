using System;
using System.Collections.Generic;

namespace LoxFramework.Evaluating
{
    interface ILoxCallable
    {
        int Arity { get; }

        object Call(AstInterpreter interpreter, IEnumerable<object> arguments);
    }

    abstract class LoxCallable : ILoxCallable
    {
        public virtual int Arity { get { return 0; } }

        public virtual object Call(AstInterpreter interpreter, IEnumerable<object> arguments)
        {
            throw new NotImplementedException();
        }
    }
}
