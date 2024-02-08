using DLCQuestipelago;

namespace DLCQuestipelagoTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(800, 50, 825, new[] { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 750, 800 })]
        [TestCase(110, 40, 115, new[] { 40, 80 })]
        [TestCase(115, 40, 115, new[] { 40, 80, 115 })]
        [TestCase(10, 40, 825, new int[0])]
        [TestCase(40, 10, 825, new[] { 10, 20, 30, 40 })]
        [TestCase(10, 6, 825, new[] { 6 })]
        [TestCase(10, 6, 10, new[] { 6, 10 })]
        [TestCase(0, 1, 825, new int[0])]
        [TestCase(1, 1, 825, new[] { 1 })]
        [TestCase(1, 1, 1, new[] { 1 })]
        [TestCase(100, 100, 825, new[] { 100 })]
        [TestCase(100, 200, 825, new int[0])]
        [TestCase(200, 100, 825, new[] { 100, 200 })]
        [TestCase(500, 100, 825, new[] { 100, 200, 300, 400, 500 })]
        [TestCase(500, 200, 825, new[] { 200, 400 })]
        [TestCase(501, 200, 825, new[] { 200, 400 })]
        [TestCase(501, 200, 501, new[] { 200, 400, 501 })]
        [TestCase(502, 200, 501, new[] { 200, 400, 501 })]
        [TestCase(800, 200, 500, new[] { 200, 400, 500 })]
        [TestCase(1000, 199, 825, new[] { 199, 398, 597, 796, 825 })]
        [TestCase(1000, 200, 825, new[] { 200, 400, 600, 800, 825 })]
        [TestCase(1001, 200, 825, new[] { 200, 400, 600, 800, 825 })]
        [TestCase(15, 6, 18, new[] { 6, 12 })]
        [TestCase(16, 6, 18, new[] { 6, 12 })]
        [TestCase(18, 6, 18, new[] { 6, 12, 18 })]
        [TestCase(5, 6, 825, new int[0])]
        public void GenerateCampaignCoinsArray_ShouldReturnExpectedResult(int current, int step, int max, int[] expected)
        {
            var campaignName = "Test Campaign:";
            var coinLocations = CoinPickupPatch.GetAllCheckedCoinLocations(current, step, max, campaignName);
            var expectedLocations = expected.Select(x => $"{campaignName} {x} Coin");
            coinLocations.Should().BeEquivalentTo(expectedLocations);
        }

        [TestCase(0, 3, new int[0])]
        [TestCase(2, 3, new[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
        })]
        [TestCase(4, 3, new[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            21, 22, 23, 24, 25, 26, 27, 28, 29, 30
        })]
        public void GenerateCampaignCoinsPiecesArray_ShouldReturnExpectedResult(int current, int max, int[] expected)
        {
            const double step = 0.1;
            var campaignName = "Test Campaign:";
            var coinLocations = CoinPickupPatch.GetAllCheckedCoinLocations(current, step, max, campaignName);
            var expectedLocations = expected.Select(x => $"{campaignName} {x} Coin Piece");
            coinLocations.Should().BeEquivalentTo(expectedLocations);
        }
    }
}