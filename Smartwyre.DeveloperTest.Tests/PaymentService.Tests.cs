using Moq;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests
{

    public class RebateServiceTests
    {
        private readonly Mock<IRebateDataStore> _rebateDataStore;
        private readonly Mock<IProductDataStore> _productDataStore;
        private readonly RebateService _service;

        public RebateServiceTests()
        {
            _rebateDataStore = new Mock<IRebateDataStore>();
            _productDataStore = new Mock<IProductDataStore>();

            var calculators = new IRebateCalculator[]
            {
            new FixedCashAmountCalculator(),
            new FixedRateRebateCalculator(),
            new AmountPerUomCalculator()
            };

            _service = new RebateService(_rebateDataStore.Object, _productDataStore.Object, calculators);
        }

        [Fact]
        public void Calculate_WhenRebateNotFound_ReturnsFailure()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns((Rebate)null!);
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product());

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 10
            });

            Assert.False(result.Success);
        }

        [Fact]
        public void Calculate_WhenProductNotFound_ReturnsFailure()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate());
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns((Product)null!);

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 10
            });

            Assert.False(result.Success);
        }

        // --- FixedCashAmount ---

        [Fact]
        public void Calculate_FixedCashAmount_WhenValid_ReturnsSuccess()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
            {
                Incentive = IncentiveType.FixedCashAmount,
                Amount = 100m
            });
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                SupportedIncentives = SupportedIncentiveType.FixedCashAmount
            });

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 10
            });

            Assert.True(result.Success);
            _rebateDataStore.Verify(x => x.StoreCalculationResult(It.IsAny<Rebate>(), 100m), Times.Once);
        }

        [Fact]
        public void Calculate_FixedCashAmount_WhenProductDoesNotSupportIncentive_ReturnsFailure()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
            {
                Incentive = IncentiveType.FixedCashAmount,
                Amount = 100m
            });
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                SupportedIncentives = SupportedIncentiveType.FixedRateRebate
            });

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 10
            });

            Assert.False(result.Success);
        }

        [Fact]
        public void Calculate_FixedCashAmount_WhenAmountIsZero_ReturnsFailure()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
            {
                Incentive = IncentiveType.FixedCashAmount,
                Amount = 0m
            });
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                SupportedIncentives = SupportedIncentiveType.FixedCashAmount
            });

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 10
            });

            Assert.False(result.Success);
        }

        // --- FixedRateRebate ---

        [Fact]
        public void Calculate_FixedRateRebate_WhenValid_ReturnsSuccessAndCorrectAmount()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
            {
                Incentive = IncentiveType.FixedRateRebate,
                Percentage = 0.1m
            });
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                Price = 200m,
                SupportedIncentives = SupportedIncentiveType.FixedRateRebate
            });

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 5
            });

            Assert.True(result.Success);
            // 200 * 0.1 * 5 = 100
            _rebateDataStore.Verify(x => x.StoreCalculationResult(It.IsAny<Rebate>(), 100m), Times.Once);
        }

        [Fact]
        public void Calculate_FixedRateRebate_WhenProductDoesNotSupportIncentive_ReturnsFailure()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
            {
                Incentive = IncentiveType.FixedRateRebate,
                Percentage = 0.1m
            });
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                Price = 200m,
                SupportedIncentives = SupportedIncentiveType.FixedCashAmount
            });

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 5
            });

            Assert.False(result.Success);
        }

        [Fact]
        public void Calculate_FixedRateRebate_WhenPercentageIsZero_ReturnsFailure()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
            {
                Incentive = IncentiveType.FixedRateRebate,
                Percentage = 0m
            });
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                Price = 200m,
                SupportedIncentives = SupportedIncentiveType.FixedRateRebate
            });

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 5
            });

            Assert.False(result.Success);
        }

        [Fact]
        public void Calculate_FixedRateRebate_WhenPriceIsZero_ReturnsFailure()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
            {
                Incentive = IncentiveType.FixedRateRebate,
                Percentage = 0.1m
            });
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                Price = 0m,
                SupportedIncentives = SupportedIncentiveType.FixedRateRebate
            });

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 5
            });

            Assert.False(result.Success);
        }

        [Fact]
        public void Calculate_FixedRateRebate_WhenVolumeIsZero_ReturnsFailure()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
            {
                Incentive = IncentiveType.FixedRateRebate,
                Percentage = 0.1m
            });
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                Price = 200m,
                SupportedIncentives = SupportedIncentiveType.FixedRateRebate
            });

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 0
            });

            Assert.False(result.Success);
        }

        // --- AmountPerUom ---

        [Fact]
        public void Calculate_AmountPerUom_WhenValid_ReturnsSuccessAndCorrectAmount()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
            {
                Incentive = IncentiveType.AmountPerUom,
                Amount = 10m
            });
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                SupportedIncentives = SupportedIncentiveType.AmountPerUom
            });

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 7
            });

            Assert.True(result.Success);
            // 10 * 7 = 70
            _rebateDataStore.Verify(x => x.StoreCalculationResult(It.IsAny<Rebate>(), 70m), Times.Once);
        }

        [Fact]
        public void Calculate_AmountPerUom_WhenProductDoesNotSupportIncentive_ReturnsFailure()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
            {
                Incentive = IncentiveType.AmountPerUom,
                Amount = 10m
            });
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                SupportedIncentives = SupportedIncentiveType.FixedCashAmount
            });

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 7
            });

            Assert.False(result.Success);
        }

        [Fact]
        public void Calculate_AmountPerUom_WhenAmountIsZero_ReturnsFailure()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
            {
                Incentive = IncentiveType.AmountPerUom,
                Amount = 0m
            });
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                SupportedIncentives = SupportedIncentiveType.AmountPerUom
            });

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 7
            });

            Assert.False(result.Success);
        }

        [Fact]
        public void Calculate_AmountPerUom_WhenVolumeIsZero_ReturnsFailure()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
            {
                Incentive = IncentiveType.AmountPerUom,
                Amount = 10m
            });
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                SupportedIncentives = SupportedIncentiveType.AmountPerUom
            });

            var result = _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 0
            });

            Assert.False(result.Success);
        }

        [Fact]
        public void Calculate_WhenSuccess_StoresCalculationResult()
        {
            var rebate = new Rebate
            {
                Incentive = IncentiveType.FixedCashAmount,
                Amount = 50m
            };
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(rebate);
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
            {
                SupportedIncentives = SupportedIncentiveType.FixedCashAmount
            });

            _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 1
            });

            _rebateDataStore.Verify(x => x.StoreCalculationResult(rebate, 50m), Times.Once);
        }

        [Fact]
        public void Calculate_WhenFailure_DoesNotStoreCalculationResult()
        {
            _rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns((Rebate)null!);
            _productDataStore.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product());

            _service.Calculate(new CalculateRebateRequest
            {
                RebateIdentifier = "R1",
                ProductIdentifier = "P1",
                Volume = 1
            });

            _rebateDataStore.Verify(x => x.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()), Times.Never);
        }
    }
}