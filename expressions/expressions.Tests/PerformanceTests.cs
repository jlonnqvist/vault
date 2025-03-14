﻿using System;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace expressions.Tests
{
    public class PerformanceTests
    {
        private readonly ITestOutputHelper _output;

        public PerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]

        // Local:
        [InlineData(ExpressionFeatures.None, 10)]
        [InlineData(ExpressionFeatures.UseDynamicBinding, 10)]
        [InlineData(ExpressionFeatures.L1Cache, 1000)]
        [InlineData(ExpressionFeatures.L1Cache | ExpressionFeatures.UseDynamicBinding, 1000)]
        [InlineData(ExpressionFeatures.L1Cache | ExpressionFeatures.L2Cache, 1000)]
        [InlineData(ExpressionFeatures.L1Cache | ExpressionFeatures.L2Cache | ExpressionFeatures.UseDynamicBinding, 1000)]

        // Remote:
        [InlineData(ExpressionFeatures.L1Cache | ExpressionFeatures.IsolateCompilation, 1000)]
        [InlineData(ExpressionFeatures.L1Cache | ExpressionFeatures.L2Cache | ExpressionFeatures.IsolateCompilation, 1000)]
        
        public void Compare_constants_versus_native(ExpressionFeatures features, int trials)
        {
            ExpressionService service = new ExpressionService(features);

            // Constants:
            CompareTests(
                features.ToString(), () => service.Evaluate("100", null, null, null),
                "Native", () => ConstantNative(),
                trials, service, 100);
        }

        private void CompareTests(string usLabel, Func<object> us, string themLabel, Func<object> them, int trials, ExpressionService service, object result)
        {
            GC.Collect();
            them();
            us();
            
            Stopwatch sw = Stopwatch.StartNew();

            bool doWork = false;

            GC.Collect();
            sw.Restart();
            for (var i = 0; i < trials; i++)
                doWork = us() == result; if (doWork) { throw new Exception("test is not valid"); }
            double usTime = sw.Elapsed.TotalMilliseconds;
            
            GC.Collect();
            sw.Restart();
            for (var i = 0; i < trials; i++)
                doWork = them() == result; if(doWork) { throw new Exception("test is not valid"); }
            double themTime = sw.Elapsed.TotalMilliseconds;

            _output.WriteLine($"Constants: ({usLabel}) @ {trials}x: {usTime}ms, {usTime/themTime:P2}");
            _output.WriteLine($"Constants: ({themLabel}) @ {trials}x: {themTime}ms");
        }

        private static int ConstantNative()
        {
            return 100;
        }
    }
}