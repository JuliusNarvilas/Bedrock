using NUnit.Framework;
using System;

namespace Common.Tests
{
    public class MathHelperTests
    {
        [Test]
        public void FloatEquals()
        {
            Random rnd = new Random();
            float randomNumberSource1 = rnd.Next(1, 1000);

            float limit = MathHelper.DEFAULT_FLOATING_POINT_EQUAL_ERROR_MARGIN;
            bool limitMinEquals = MathHelper.Equals(randomNumberSource1, randomNumberSource1 - limit);
            bool limitMaxEquals = MathHelper.Equals(randomNumberSource1, randomNumberSource1 + limit);

            bool overLimitMin = MathHelper.Equals(randomNumberSource1, randomNumberSource1 - limit - limit);
            bool overLimitMax = MathHelper.Equals(randomNumberSource1, randomNumberSource1 + limit + limit);

            Assert.That(limitMinEquals && limitMaxEquals && !overLimitMin && !overLimitMax);
        }
    }
}