using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services
{
    public class AmountPerUomCalculator : IRebateCalculator
    {
        public IncentiveType SupportedIncentive => IncentiveType.AmountPerUom;

        public bool IsEligible(Rebate rebate, Product product, CalculateRebateRequest request)
        {
            if (rebate == null || product == null)
                return false;

            if (!product.SupportedIncentives.HasFlag(SupportedIncentiveType.AmountPerUom))
                return false;

            if (rebate.Amount == 0 || request.Volume == 0)
                return false;

            return true;
        }

        public decimal Calculate(Rebate rebate, Product product, CalculateRebateRequest request)
        {
            return rebate.Amount * request.Volume;
        }
    }
}
