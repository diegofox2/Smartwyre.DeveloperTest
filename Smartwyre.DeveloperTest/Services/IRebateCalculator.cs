using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services
{
    public interface IRebateCalculator
    {
        IncentiveType SupportedIncentive { get; }
        bool IsEligible(Rebate rebate, Product product, CalculateRebateRequest request);
        decimal Calculate(Rebate rebate, Product product, CalculateRebateRequest request);
    }
}
