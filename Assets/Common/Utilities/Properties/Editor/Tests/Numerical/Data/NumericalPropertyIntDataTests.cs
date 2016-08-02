using NUnit.Framework;
using System;
using Utilities.Properties.Numerical.Data;

namespace Utilities.Properties.Tests.Numerical.Data
{
    public class NumericalPropertyIntDataTests
    {
        [Test]
        public void NumericalPropertyIntDataConstructors()
        {
            NumericalPropertyIntData emptyPropertyDataValue = new NumericalPropertyIntData();
            int initialEmptyPropertyDataInt = emptyPropertyDataValue.Get();
            Assert.That(initialEmptyPropertyDataInt == 0);

            Random rnd = new Random();
            int randomIntSource = rnd.Next(1, 1000);
            NumericalPropertyIntData propertyDataValue = new NumericalPropertyIntData(randomIntSource);
            int initialPropertyDataValue = propertyDataValue.Get();
            Assert.That(randomIntSource == initialPropertyDataValue);

            INumericalPropertyData<int> createdPropertyDataValue = propertyDataValue.CreateZero();
            Assert.That(createdPropertyDataValue != propertyDataValue);
            Assert.That(createdPropertyDataValue.Get() == 0);
        }

        [Test]
        public void NumericalPropertyIntDataOperations()
        {
            Random rnd = new Random();
            int randomIntSource1 = rnd.Next(1, 1000);
            int randomIntSource2 = rnd.Next(1, 1000);
            int randomIntSource3 = rnd.Next(1, 1000);
            int randomIntSource4 = rnd.Next(1, 1000);
            int randomIntSource5 = rnd.Next(1, 1000);

            NumericalPropertyIntData propertyDataValue = new NumericalPropertyIntData(randomIntSource1);
            propertyDataValue.Add(randomIntSource2);
            int sum = propertyDataValue.Get();
            Assert.That(sum == (randomIntSource1 + randomIntSource2));

            propertyDataValue.Substract(randomIntSource3);
            int substracted = propertyDataValue.Get();
            Assert.That(substracted == (randomIntSource1 + randomIntSource2 - randomIntSource3));

            propertyDataValue.Multiply(randomIntSource4);
            int multiplied = propertyDataValue.Get();
            Assert.That(multiplied == ((randomIntSource1 + randomIntSource2 - randomIntSource3) * randomIntSource4));


            NumericalPropertyIntData tempPropertyDataValue = new NumericalPropertyIntData();
            tempPropertyDataValue.Set(multiplied);
            Assert.That(tempPropertyDataValue.Get() == propertyDataValue.Get());

            int additiveInverse = tempPropertyDataValue.AdditiveInverse();
            Assert.That(additiveInverse == -tempPropertyDataValue.Get());

            Assert.That(tempPropertyDataValue.MultiplicativeInverse() == 0);

            tempPropertyDataValue.ToZero();
            Assert.That(tempPropertyDataValue.Get() == 0);
            

            propertyDataValue.Divide(randomIntSource5);
            int divided = propertyDataValue.Get();
            Assert.That(divided == (((randomIntSource1 + randomIntSource2 - randomIntSource3) * randomIntSource4) / randomIntSource5));
        }
    }
}
