using System;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Runner
{

    class Program
    {
        static void Main(string[] args)
        {
            var rebateDataStore = new RebateDataStore();
            var productDataStore = new ProductDataStore();

            var calculators = new IRebateCalculator[]
            {
            new FixedCashAmountCalculator(),
            new FixedRateRebateCalculator(),
            new AmountPerUomCalculator()
            };

            var rebateService = new RebateService(rebateDataStore, productDataStore, calculators);

            Console.Write("Enter Rebate Identifier: ");
            var rebateIdentifier = Console.ReadLine();

            Console.Write("Enter Product Identifier: ");
            var productIdentifier = Console.ReadLine();

            Console.Write("Enter Volume: ");
            var volume = decimal.Parse(Console.ReadLine()!);

            var request = new CalculateRebateRequest
            {
                RebateIdentifier = rebateIdentifier!,
                ProductIdentifier = productIdentifier!,
                Volume = volume
            };

            var result = rebateService.Calculate(request);

            Console.WriteLine($"Result: {(result.Success ? "Success" : "Failure")}");
        }
    }
}