namespace Outcasts
{
    using NUnit.Framework;
    using System.Collections.Generic;

    public class BinarySearchTests
    {
        [Test]
        public void BinarySearch_InsertAtStart()
        {
            IList<double> list = new List<double> { 10, 20 };
            (int insertionIndex, bool existing) = list.BinarySearch(5);

            Assert.IsFalse(existing);
            Assert.AreEqual(0, insertionIndex);
        }

        [Test]
        public void BinarySearch_ReturnsInsertionPoint_WhenItemFound()
        {
            IList<double> list = new List<double> { 10, 20 };
            (int insertionIndex, bool existing) = list.BinarySearch(10);

            Assert.IsTrue(existing);
            Assert.AreEqual(0, insertionIndex);
        }

        [Test]
        public void BinarySearch_ReturnsInsertionPoint_WhenItemNotFound()
        {
            IList<double> list = new List<double> { 10, 20 };
            (int insertionIndex, bool existing) = list.BinarySearch(15);

            Assert.IsFalse(existing);
            Assert.AreEqual(1, insertionIndex); // before first element
        }


        [Test]
        public void BinarySearch_ReturnsInsertionPoint_WhenItemFound2()
        {
            IList<double> list = new List<double> { 10, 20 };
            (int insertionIndex, bool existing) = list.BinarySearch(20);

            Assert.IsTrue(existing);
            Assert.AreEqual(1, insertionIndex);
        }

        [Test]
        public void BinarySearch_InsertAtEnd()
        {
            IList<double> list = new List<double> { 10, 20 };
            (int insertionIndex, bool existing) = list.BinarySearch(25);

            Assert.IsFalse(existing);
            Assert.AreEqual(2, insertionIndex);
        }
    }
}
