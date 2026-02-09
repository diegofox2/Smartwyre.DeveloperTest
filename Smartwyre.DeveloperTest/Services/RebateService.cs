using System.Collections.Generic;
using System.Linq;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services
{
    public class RebateService : IRebateService
    {
        private readonly IRebateDataStore _rebateDataStore;
        private readonly IProductDataStore _productDataStore;
        private readonly Dictionary<IncentiveType, IRebateCalculator> _calculators;

        public RebateService(
            IRebateDataStore rebateDataStore,
            IProductDataStore productDataStore,
            IEnumerable<IRebateCalculator> calculators)
        {
            _rebateDataStore = rebateDataStore;
            _productDataStore = productDataStore;
            _calculators = calculators.ToDictionary(c => c.SupportedIncentive);
        }

        public CalculateRebateResult Calculate(CalculateRebateRequest request)
        {
            var result = new CalculateRebateResult();

            var rebate = _rebateDataStore.GetRebate(request.RebateIdentifier);
            var product = _productDataStore.GetProduct(request.ProductIdentifier);

            if (rebate == null || product == null)
            {
                result.Success = false;
                return result;
            }

            if (!_calculators.TryGetValue(rebate.Incentive, out var calculator))
            {
                result.Success = false;
                return result;
            }

            if (!calculator.IsEligible(rebate, product, request))
            {
                result.Success = false;
                return result;
            }

            var rebateAmount = calculator.Calculate(rebate, product, request);
            _rebateDataStore.StoreCalculationResult(rebate, rebateAmount);
            result.Success = true;

            return result;
        }
    }
}
