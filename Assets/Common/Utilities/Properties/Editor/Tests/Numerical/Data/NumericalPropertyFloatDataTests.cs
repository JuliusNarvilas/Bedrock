using NUnit.Framework;
using System;
using Utilities.Properties.Numerical.Data;

namespace Utilities.Properties.Tests.Numerical.Data
{
    public class NumericalPropertyFloatDataTests
    {
        [Test]
        public void NumericalPropertyFloatDataConstructors()
        {
            NumericalPropertyFloatData emptyPropertyDataValue = new NumericalPropertyFloatData();
            float initialEmptyPropertyDataVal = emptyPropertyDataValue.Get();
            Assert.That(initialEmptyPropertyDataVal == 0);

            Random rnd = new Random();
            float randomNumberSource = rnd.Next(1, 1000);
            NumericalPropertyFloatData propertyDataValue = new NumericalPropertyFloatData(randomNumberSource);
            float initialPropertyDataValue = propertyDataValue.Get();
            Assert.That(randomNumberSource == initialPropertyDataValue);

            INumericalPropertyData<float> createdPropertyDataValue = propertyDataValue.CreateZero();
            Assert.That(createdPropertyDataValue != propertyDataValue);
            Assert.That(createdPropertyDataValue.Get() == 0);
        }

        [Test]
        public void NumericalPropertyFloatDataOperations()
        {
            Random rnd = new Random();
            float randomNumberSource1 = rnd.Next(1, 1000);
            float randomNumberSource2 = rnd.Next(1, 1000);
            float randomNumberSource3 = rnd.Next(1, 1000);
            float randomNumberSource4 = rnd.Next(1, 1000);
            float randomNumberSource5 = rnd.Next(1, 1000);

            NumericalPropertyFloatData propertyDataValue = new NumericalPropertyFloatData(randomNumberSource1);
            propertyDataValue.Add(randomNumberSource2);
            float sum = propertyDataValue.Get();
            Assert.That(sum == (randomNumberSource1 + randomNumberSource2));

            propertyDataValue.Substract(randomNumberSource3);
            float substracted = propertyDataValue.Get();
            Assert.That(substracted == (randomNumberSource1 + randomNumberSource2 - randomNumberSource3));

            propertyDataValue.Multiply(randomNumberSource4);
            float multiplied = propertyDataValue.Get();
            Assert.That(multiplied == ((randomNumberSource1 + randomNumberSource2 - randomNumberSource3) * randomNumberSource4));


            NumericalPropertyFloatData tempPropertyDataValue = new NumericalPropertyFloatData();
            tempPropertyDataValue.Set(multiplied);
            Assert.That(tempPropertyDataValue.Get() == propertyDataValue.Get());

            float additiveInverse = tempPropertyDataValue.AdditiveInverse();
            Assert.That(additiveInverse == -tempPropertyDataValue.Get());

            float multiplicativeInverse = 1.0f / tempPropertyDataValue.Get();
            Assert.That(tempPropertyDataValue.MultiplicativeInverse() == multiplicativeInverse);

            tempPropertyDataValue.ToZero();
            Assert.That(tempPropertyDataValue.Get() == 0);


            propertyDataValue.Divide(randomNumberSource5);
            float divided = propertyDataValue.Get();
            Assert.That(divided == (((randomNumberSource1 + randomNumberSource2 - randomNumberSource3) * randomNumberSource4) / randomNumberSource5));
        }

    }
}
