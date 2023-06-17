using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MarcRocNy.Common.Extensions
{
    public class EnumerableExtTest
    {
        #region Safe

        [Theory]
        [MemberData(nameof(EnumerableExtCases.EmptyAndNullInts), MemberType = typeof(EnumerableExtCases))]
        public void Safe_Empty(IEnumerable<int>? nothingHere)
        {
            var safe = nothingHere.Safe();
            safe.Should().NotBeNull();
            safe.Should().BeEmpty();
        }

        [Theory]
        [MemberData(nameof(EnumerableExtCases.EnumsWithThings), MemberType = typeof(EnumerableExtCases))]
        public void Safe_Things_Same(IEnumerable<string>? source)
        {
            var safe = source.Safe();
            safe.Should().NotBeNull();
            safe.Should().BeSameAs(source);
            safe.Should().BeEquivalentTo(new[] { "one", "two", "three" });
        }

        [Fact]
        public void Safe_Active_Same()
        {
            var source = new List<string> { "one", "two", "three" }.NullIfEmpty();
            var safe = source.Safe();
            safe.Should().NotBeNull();
            safe.Should().BeSameAs(source);
            safe.Should().BeEquivalentTo(new[] { "one", "two", "three" });
        }

        #endregion

        #region Clone

        [Fact]
        public void Clone_EmptyList_ReturnsEmptyList()
        {
            //Setup
            List<int> list = new();

            //Act
            var empty = list.Clone();

            //Assert
            empty.Should().BeEmpty();
        }

        [Fact]
        public void Clone_NonEmptyList_ReturnsClonedList()
        {
            //Setup
            List<int> list = new() { 1 };

            //Act
            var empty = list.Clone();

            //Assert
            empty.Should().HaveCount(1);
        }

        [Fact]
        public void Clone_MultipleElements_ReturnsClonedList()
        {
            //Setup
            List<int> list = new() { 1, 2 };

            //Act
            var empty = list.Clone();

            //Assert
            empty.Should().HaveCount(2);
        }

        #endregion

        #region TrySelect

        [Fact]
        public void TrySelect_BasicUsage()
        {
            var tryParse = (new[] { "1", "two", "3" }).TrySelect(s => (int.TryParse(s, out int i), i));

            tryParse.Should().BeEquivalentTo(new[] { 1, 3 });
        }

        #endregion

        #region MaxT

        [Fact]
        public void MaxT_ChooseNumber()
        {
            var list = new[] { (1, "one"), (0, "zero"), (2, "two") };
            list.MaxT(t => t.Item1).Should().Be((2, "two"));
        }

        [Fact]
        public void MaxT_ChooseString()
        {
            var list = new[] { (1, "one"), (0, "zero"), (2, "two") };
            list.MaxT(t => t.Item2).Should().Be((0, "zero"));
        }

        [Fact]
        public void MaxT_ChooseWeird()
        {
            var list = new[] { (1, "one"), (0, "zero"), (2, "two") };
            list.MaxT(t => t.Item1 + t.Item2.Length).Should().Be((2, "two"));
        }

        #endregion

        #region Equality

        [Theory]
        [MemberData(nameof(EnumerableExtCases.EnumerableEqualityCases), MemberType = typeof(EnumerableExtCases))]
        public void SetEquals_Cases(IEnumerable<string> x, IEnumerable<string> y, bool expected)
        {
            x.SetEquals(y).Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(EnumerableExtCases.EnumerableEqualityCases), MemberType = typeof(EnumerableExtCases))]
        public void SetEquals_Comparer_Cases(IEnumerable<string> x, IEnumerable<string> y, bool expected)
        {
            x = x.Select(s => s.ToUpper());
            x.SetEquals(y, StringComparer.OrdinalIgnoreCase).Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(EnumerableExtCases.EnumerableEqualityCases), MemberType = typeof(EnumerableExtCases))]
        public void HashCodeSet_Cases(IEnumerable<string> x, IEnumerable<string> y, bool _)
        {
            var hashEqual = x.HashCodeSet() == y.HashCodeSet();
            var equal = x.SetEquals(y);

            hashEqual.Should().Be(equal);
        }

        [Theory]
        [MemberData(nameof(EnumerableExtCases.EnumerableEqualityCases), MemberType = typeof(EnumerableExtCases))]
        public void HashCodeSet_Comparer_Cases(IEnumerable<string> x, IEnumerable<string> y, bool _)
        {
            x = x.Select(s => s.ToUpper());
            var hashEqual = x.HashCodeSet(StringComparer.OrdinalIgnoreCase) == y.HashCodeSet(StringComparer.OrdinalIgnoreCase);
            var equal = x.SetEquals(y, StringComparer.OrdinalIgnoreCase);

            x.SetEquals(y).Should().BeFalse();
            hashEqual.Should().Be(equal);
        }

        [Theory]
        [MemberData(nameof(EnumerableExtCases.EnumerableEqualityCases), MemberType = typeof(EnumerableExtCases))]
        public void HashCodeSequence_Cases(IEnumerable<string> x, IEnumerable<string> y, bool _)
        {
            var hashEqual = x.HashCodeSequence() == y.HashCodeSequence();
            var equal = x.SequenceEqual(y);

            hashEqual.Should().Be(equal);
        }

        [Theory]
        [MemberData(nameof(EnumerableExtCases.EnumerableEqualityCases), MemberType = typeof(EnumerableExtCases))]
        public void HashCodeSequence_Comparer_Cases(IEnumerable<string> x, IEnumerable<string> y, bool _)
        {
            x = x.Select(s => s.ToUpper());
            var hashEqual = x.HashCodeSequence(StringComparer.OrdinalIgnoreCase) == y.HashCodeSequence(StringComparer.OrdinalIgnoreCase);
            var equal = x.SequenceEqual(y, StringComparer.OrdinalIgnoreCase);

            x.SequenceEqual(y).Should().BeFalse();
            hashEqual.Should().Be(equal);
        }

        #endregion

        #region SplitList

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void SplitList_InvalidSize_Throws(int splitSize)
        {
            var list = new List<int>();
            Action a = () => list.SplitList(splitSize);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void SplitList_ValidParams_ReturnExpectedSplitList()
        {
            IEnumerable<int> list = new List<int>() { 1, 2, 3, 4, 5 };
            var splitListExpected = new List<List<int>>()
            {
                new List<int>() {1,2},
                new List<int>() {3,4},
                new List<int>() {5}
            };

            var splitList = list.SplitList(2);
            splitList.Should().BeEquivalentTo(splitListExpected);
        }

        [Fact]
        public void SplitList_ValidParams_ExactSplit_ReturnExpectedSplitList()
        {
            IEnumerable<int> list = new List<int>() { 1, 2, 3, 4, 5, 6 };
            var splitListExpected = new List<List<int>>()
            {
                new List<int>() {1,2},
                new List<int>() {3,4},
                new List<int>() {5,6}
            };

            var splitList = list.SplitList(2);
            splitList.Should().BeEquivalentTo(splitListExpected);
        }

        [Fact]
        public void SplitList_SizeOfSplitsLarge_ReturnExpectedSplitList()
        {
            IEnumerable<int> list = new List<int>() { 1, 2, 3, 4, 5 };
            var splitListExpected = new List<List<int>>()
            {
                new List<int>() { 1, 2, 3, 4, 5 }
            };

            var splitList = list.SplitList(10);
            splitList.Should().BeEquivalentTo(splitListExpected);
        }

        [Fact]
        public void SplitList_EmptyList_ReturnExpectedSplitList()
        {
            IEnumerable<int> list = new List<int>() { };
            var splitListExpected = new List<List<int>>();

            var splitList = list.SplitList(2);
            splitList.Should().BeEquivalentTo(splitListExpected);
        }


        [Fact]
        public void SplitList_SizeOfSplitsZero_ReturnExpectedSplitList()
        {
            IEnumerable<int> list = new List<int>() { 1, 2, 3, 4, 5 };

            Action a = () => list.SplitList(0);
            a.Should().Throw<ArgumentException>();
        }

        #endregion

        #region GetPage

        [Fact]
        public void GetPage_SizeZero_Throws()
        {
            IEnumerable<int> source = new[] { 1, 2, 3, 4, 5 };
            var enumerator = source.GetEnumerator();

            Action a = () => _ = enumerator.GetPage(0).GetEnumerator().MoveNext();
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void GetPage_MoreThanPage_Expected()
        {
            IEnumerable<int> source = new [] { 1, 2, 3, 4, 5 };
            var expectedPage = new[] { 1, 2, 3 };

            var page = source.GetEnumerator().GetPage(3);
            page.Should().BeEquivalentTo(expectedPage);
        }

        [Fact]
        public void GetPage_SecondPage_Expected()
        {
            IEnumerable<int> source = new[] { 1, 2, 3, 4, 5 };
            var expectedPage = new[] { 4, 5 };

            var enumerator = source.GetEnumerator();
            _ = enumerator.GetPage(3).ToList();
            var page = enumerator.GetPage(3);
            page.Should().BeEquivalentTo(expectedPage);
        }

        [Fact]
        public void GetPage_SameSize_Expected()
        {
            IEnumerable<int> source = new[] { 1, 2, 3, 4, 5 };
            var expectedPage = new[] { 1, 2, 3, 4, 5 };

            var page = source.GetEnumerator().GetPage(5);
            page.Should().BeEquivalentTo(expectedPage);
        }

        [Fact]
        public void GetPage_Exhausted_Expected()
        {
            IEnumerable<int> source = new[] { 1, 2, 3, 4, 5 };

            var enumerator = source.GetEnumerator();
            _ = enumerator.GetPage(5).ToList();
            var page = enumerator.GetPage(5);
            page.Should().BeEmpty();
        }

        [Fact]
        public void GetPage_LessThanPage_ReturnExpected()
        {
            IEnumerable<int> source = new[] { 1, 2, 3, 4, 5 };
            var expectedPage = new[] { 1, 2, 3, 4, 5 };

            var page = source.GetEnumerator().GetPage(8);
            page.Should().BeEquivalentTo(expectedPage);
        }

        [Fact]
        public void GetPage_Empty_ReturnExpected()
        {
            IEnumerable<int> source = Array.Empty<int>();

            var page = source.GetEnumerator().GetPage(8);
            page.Should().BeEmpty();
        }

        #endregion

        #region Paginate

        [Fact]
        public void Paginate_SizeZero_Throws()
        {
            IEnumerable<int> source = new[] { 1, 2, 3, 4, 5 };

            Action a = () => source.Paginate(0).GetEnumerator().MoveNext();
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Paginate_IterateOuterOnly_Throws()
        {
            IEnumerable<int> source = new[] { 1, 2, 3, 4, 5 };

            var pages = source.Paginate(2);
            Action a = () => _ = pages.ToList();

            a.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Paginate_PartialIterateInner_Throws()
        {
            IEnumerable<int> source = new[] { 1, 2, 3, 4, 5 };

            var pages = source.Paginate(2);
            Action a = () => _ = pages.Select(p => p.First()).ToList();

            a.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Paginate_MoreThanPage_ReturnExpected()
        {
            IEnumerable<int> source = new[] { 1, 2, 3, 4, 5 };
            var expectedPages = new[] { new[] { 1, 2, 3 }, new[] { 4, 5 } };

            var pages = source.Paginate(3);
            pages.Select(p => p.ToList()).Should().BeEquivalentTo(expectedPages);
        }

        [Fact]
        public void Paginate_SameSize_ReturnExpected()
        {
            IEnumerable<int> source = new[] { 1, 2, 3, 4, 5 };
            var expectedPages = new[] { new[] { 1, 2, 3, 4, 5 } };

            var pages = source.Paginate(5);
            pages.Select(p => p.ToList()).Should().BeEquivalentTo(expectedPages);
        }

        [Fact]
        public void Paginate_LessThanPage_ReturnExpected()
        {
            IEnumerable<int> source = new[] { 1, 2, 3, 4, 5 };
            var expectedPages = new[] { new[] { 1, 2, 3, 4, 5 } };

            var pages = source.Paginate(8);
            pages.Select(p => p.ToList()).Should().BeEquivalentTo(expectedPages);
        }

        [Fact]
        public void Paginate_Empty_ReturnExpected()
        {
            IEnumerable<int> source = Array.Empty<int>();
            var expectedPages = Array.Empty<int[]>();

            var pages = source.Paginate(8);
            pages.Select(p => p.ToList()).Should().BeEquivalentTo(expectedPages);
        }

        #endregion

        #region AnyContinueOnce

        [Fact]
        public void AnyContinueOnce_EmptySource_False_Empty()
        {
            var source = Array.Empty<int>();

            source.AnyContinueOnce(out var ifAny).Should().BeFalse();
            ifAny.Should().BeEmpty();
        }

        [Fact]
        public void AnyContinueOnce_NonEmpty_True_SingleUse()
        {
            var source = new[] { 1, 2, 3 };

            source.AnyContinueOnce(out var ifAny).Should().BeTrue();
            ifAny.Should().BeEquivalentTo(new[] { 1, 2, 3 });

            // don't try again, the underlying enumerator is all used up
            // if you need to re-enumerate, just go back to the source
            Action act = () => _ = ifAny.Any();
            act.Should().Throw<InvalidOperationException>().WithMessage("*already fin*");
        }

        #endregion

        #region NullIfEmpty

        [Fact]
        public void NullIfEmpty_Empty_Null()
        {
            Enumerable.Empty<string>().NullIfEmpty().Should().BeNull();
        }

        [Fact]
        public void NullIfEmpty_NonEmpty_Same()
        {
            new[] { 1, 2, 3, 5, 8 }.NullIfEmpty().Should().BeEquivalentTo(new List<int> { 1, 2, 3, 5, 8 });
        }

        [Fact]
        public void ToListNullIfEmpty_Empty_Null()
        {
            Enumerable.Empty<string>().NullIfEmptyToList().Should().BeNull();
        }

        [Fact]
        public void ToListNullIfEmpty_NonEmpty_List()
        {
            new[] { 1, 2, 3, 5, 8 }.NullIfEmptyToList().Should().BeEquivalentTo(new List<int> { 1, 2, 3, 5, 8 });
        }

        [Fact]
        public void SafeAggregate_Empty_Null()
        {
            Enumerable.Empty<string>().SafeAggregate((l, r) => l + r).Should().BeNull();
        }

        [Fact]
        public void SafeAggregate_Empty_Default()
        {
            Enumerable.Empty<int>().SafeAggregate((l, r) => l + r).Should().Be(0);
        }

        [Fact]
        public void SafeAggregate_NonEmpty_Works()
        {
            new[] { 1, 2, 3, 5, 8 }.SafeAggregate((l, r) => l + r).Should().Be(19);
        }

        #endregion
    }
}
