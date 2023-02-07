using Xunit;
using System;

namespace TestProject
{
    public class ReduceTests
    {
        [Fact]
        public void Test1()
        {
            System.Diagnostics.Debug.WriteLine("Hello World!");
			
			int a = 0;
			a = a + 1;
			
			System.Diagnostics.Debug.WriteLine("Hello World!");
			
			int b = 0;
			b = b + 1;
			
			Assert.False(a == 1);
        }
    }
}

