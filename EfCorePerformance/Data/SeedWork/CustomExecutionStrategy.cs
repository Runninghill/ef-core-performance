#nullable enable
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFTestApp.Data.SeedWork
{
    public class CustomExecutionStrategy : ExecutionStrategy
    {
        public CustomExecutionStrategy(DbContext context, int maxRetryCount, TimeSpan maxRetryDelay) 
            : base(context, maxRetryCount, maxRetryDelay)
        {
        }

        public CustomExecutionStrategy(ExecutionStrategyDependencies dependencies, int maxRetryCount, TimeSpan maxRetryDelay) 
            : base(dependencies, maxRetryCount, maxRetryDelay)
        {
        }

        protected override bool ShouldRetryOn(Exception? exception)
        {
            return exception != null && exception.GetType() == typeof(InvalidOperationException);
        }
    }
}