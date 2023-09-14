using NUBAN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace NUBAN
{
    /// <summary>
    /// Provides utility methods for generating and validating Nigerian NUBAN (Nigerian Uniform Bank Account Number) account numbers,
    /// according to the CBN revised standard (2020).
    /// </summary>
    public static class NUBANUtility
    {
        /// <summary>
        /// The seed used for generating check digits.
        /// </summary>
        private static readonly string Seed = "373373373373373";

        /// <summary>
        /// The length of the serial number portion in a NUBAN account number.
        /// </summary>
        private static readonly int SerialNumberLength = 9;

        /// <summary>
        /// The length of a complete NUBAN account number.
        /// </summary>
        private static readonly int NubanLength = 10;


        /// <summary>
        /// Gets a list of banks from a JSON file located at the specified URL.
        /// </summary>
        /// <returns>A list of Bank objects.</returns>
        /// <exception cref="HttpRequestException">Thrown if there is an issue with the HTTP request.</exception>
        /// <exception cref="Newtonsoft.Json.JsonException">Thrown if there is an issue with JSON deserialization.</exception>
        public static async Task<IEnumerable<Bank>> GetBanksAsync()
        {

            var source = "https://raw.githubusercontent.com/Chidiebube-Onah/BanksInNigeria/main/BanksInNigeria.min.json";
               
            IEnumerable<Bank> banks = await ReadBanksFromJsonUrlAsync(source);

            return banks;
            
        }

        /// <summary>
        /// Attempts to infer valid banks based on the provided account number.
        /// </summary>
        /// <param name="accountNumber">The account number to be used for inferring banks.</param>
        /// <returns>A list of Bank objects that match the criteria based on the account number.</returns>
        public static async Task<IEnumerable<Bank>> InferBanksAsync(string accountNumber)
        {
            var banks = (await GetBanksAsync()).ToList();

         
            foreach (var bank in banks.ToList())
            {
                // Criteria for inferring valid banks:
                // - The bank code consists of only digits.
                // - The bank code has a length of either 3 or 5 characters.
                // - The provided account number is valid for the bank code.
                var inferredUsingShortCode = bank.code.All(char.IsDigit) &&
                               (bank.code.Length == 3 || bank.code.Length == 5) &&
                               IsBankAccountValid(accountNumber, bank.code);

                var inferredUsingLongCode = bank.code.All(char.IsDigit) &&
                                             (bank.longcode.Length == 3 || bank.longcode.Length == 5) &&
                                             IsBankAccountValid(accountNumber, bank.longcode);

                // If the bank does not meet the criteria, remove it from the list
                if (!(inferredUsingShortCode || inferredUsingLongCode))
                {
                    banks.Remove(bank);
                }
            }

          
            return banks;
        }


        /// <summary>
        /// Generates a valid NUBAN account number based on the provided serial number and bank code.
        /// </summary>
        /// <param name="serialNumber">The serial number portion of the NUBAN account number.</param>
        /// <param name="bankCode">The code of the Nigerian commercial bank.</param>
        /// <returns>A valid NUBAN account number.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the serial number is longer than <see cref="SerialNumberLength"/>.</exception>
        public static string CreateAccount(string serialNumber, string bankCode)
        {
           
            // Pad the serial number to the specified length.
            serialNumber = serialNumber.PadLeft(SerialNumberLength, '0');

            // Generate the NUBAN by combining the serial number and check digit.
            string nuban = $"{serialNumber}{GenerateCheckDigit(serialNumber, bankCode)}";

            return nuban;
        }


        /// <summary>
        /// Validates a NUBAN account number.
        /// </summary>
        /// <param name="accountNumber">The NUBAN account number to validate.</param>
        /// <param name="bankCode">The code of the Nigerian commercial bank.</param>
        /// <returns>True if the NUBAN account number is valid; otherwise, false.</returns>
        public static bool IsBankAccountValid(string accountNumber, string bankCode)
        {
           
            if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length != NubanLength)
            {
                return false;
            }

            string serialNumber = accountNumber.Substring(0, SerialNumberLength);

            int checkDigit = GenerateCheckDigit(serialNumber, bankCode);

            // Compare the calculated check digit with the check digit in the account number.
            return checkDigit == (accountNumber[NubanLength - 1] - '0');
        }

        /// <summary>
        /// Generates a check digit for the provided serial number and bank code.
        /// </summary>
        /// <param name="serialNumber">The serial number portion of the NUBAN account number.</param>
        /// <param name="bankCode">The code of the Nigerian commercial bank.</param>
        /// <returns>The check digit for the NUBAN account number.</returns>
        /// <exception cref="Exception">Thrown if the serial number is longer than <see cref="SerialNumberLength"/>.</exception>
        private static int GenerateCheckDigit(string serialNumber, string bankCode)
        {
        
            if (serialNumber.Length > SerialNumberLength)
            {
                throw new InvalidOperationException($"Serial number should be at most {SerialNumberLength}-digits long.");
            }

          
            serialNumber = serialNumber.PadLeft(SerialNumberLength, '0');

            switch (bankCode.Length)
            {
                // Normalize the bank code to 6 characters according to CBN Revised Standard (2020).
                case 3:
                    bankCode = bankCode.PadLeft(6, '0');
                    break;
                case 5:
                    bankCode = bankCode.PadLeft(6, '9');
                    break;
                default:
                    throw new InvalidOperationException("Bank Code must be either 3 or 5 digits!");
            }

            // Combine the bank code and serial number to create the cipher.
            string cipher = bankCode + serialNumber;

            // Calculate the check digit using the seed.
            int sum = 0;
            for (int i = 0; i < cipher.Length; i++)
            {
                sum += (cipher[i] - '0') * (Seed[i] - '0');
            }

            int checkDigit = 10 - (sum % 10);

            return (checkDigit == 10) ? 0 : checkDigit;
        }



        /// <summary>
        /// Reads a JSON file containing bank data from a URL and returns a list of Bank objects.
        /// </summary>
        /// <param name="jsonFileUrl">The URL of the JSON file.</param>
        /// <returns>A list of Bank objects.</returns>
        /// <exception cref="HttpRequestException">Thrown if there is an issue with the HTTP request.</exception>
        /// <exception cref="JsonException">Thrown if there is an issue with JSON deserialization.</exception>
        private static async Task<IEnumerable<Bank>> ReadBanksFromJsonUrlAsync(string jsonFileUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                  
                    HttpResponseMessage response = await client.GetAsync(jsonFileUrl);

                   
                    response.EnsureSuccessStatusCode();

                  
                    string jsonData = await response.Content.ReadAsStringAsync();

                    
                    IEnumerable<Bank> banks = JsonConvert.DeserializeObject<IEnumerable<Bank>>(jsonData);

                    return banks;
                }
            }
            catch (Exception ex)
            {
              return new List<Bank>(0);
            }
           
        }
    }
}
